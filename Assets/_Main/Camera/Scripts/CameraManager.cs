using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cinemachine;
using UnityEngine;
using Utilities;

namespace _Main.Camera.Scripts
{
    public class CameraManager : Singleton<CameraManager>
    {
        [Header("Camera Brain")] [SerializeField]
        private CinemachineBrain cinemachineBrain;

        [SerializeField] private CameraType startCam;

        [Header("Virtual Cameras")] [SerializeField]
        private List<VirtualCameraData> virtualCameraData;


        private readonly Dictionary<CameraType, CinemachineVirtualCamera> _virtualCameraDictionary = new();
        private readonly Dictionary<CinemachineVirtualCamera, VirtualCameraData> _virtualCameraDataDictionary = new();

        private Task _assignCoroutine;
        private CinemachineVirtualCamera _currentCamera;
        private CinemachineVirtualCamera _targetCamera;
        private CameraType _currentCamType;


        private void Awake()
        {
            InitCams();
            InitCameraShakers();
            SetPriorityToZero();
            SetCam(startCam, 0, CinemachineBlendDefinition.Style.Cut);
        }


        private void InitCams()
        {
            virtualCameraData.ForEach(cameraData =>
            {
                _virtualCameraDictionary.Add(cameraData.cameraType, cameraData.virtualCamera);
                _virtualCameraDataDictionary.Add(cameraData.virtualCamera, cameraData);
            });
            _currentCamType = virtualCameraData.First().cameraType;
        }

        private void SetPriorityToZero()
        {
            foreach (var virtualCamera in _virtualCameraDictionary)
            {
                virtualCamera.Value.m_Priority = 0;
            }
        }

        private void InitCameraShakers()
        {
            var shakeCams = virtualCameraData
                .Where(cameraData => cameraData.canShake).ToList();

            shakeCams.ForEach(camData =>
            {
                var shake = camData.virtualCamera.gameObject.AddComponent<CameraShake>();
                camData.cameraShake = shake;
            });
        }

        private CinemachineVirtualCamera GetCam(CameraType cameraType)
        {
            return _virtualCameraDictionary[_currentCamType];
        }

        private void SetCam(CameraType cameraType, float blendDuration, CinemachineBlendDefinition.Style blendStyle)
        {
            cinemachineBrain.m_DefaultBlend.m_Time = blendDuration;
            cinemachineBrain.m_DefaultBlend.m_Style = blendStyle;
            _currentCamera = GetCam(_currentCamType);
            _targetCamera = GetCam(cameraType);
            _currentCamera.Priority = 0;
            _targetCamera.Priority = 1;
            _currentCamType = cameraType;
            _currentCamera = _targetCamera;
        }


        public void AssignCamera(CameraType cameraType, Action onBlendStart = null, Action onBlendEnd = null,
            float blendDuration = 1f,
            float blendDelay = 0,
            CinemachineBlendDefinition.Style blendStyle = CinemachineBlendDefinition.Style.EaseInOut)
        {
            _assignCoroutine?.Dispose();

            _assignCoroutine =  AssignCameraCoroutine(cameraType, onBlendStart, onBlendEnd, blendDuration,
                blendDelay, blendStyle);
        }

        private async Task AssignCameraCoroutine(CameraType cameraType, Action onStart, Action onEnd,
            float blendDuration,
            float blendDelay,
            CinemachineBlendDefinition.Style blendStyle)
        {
            await Task.Delay(TimeSpan.FromSeconds(blendDelay));//WaiterHelpers.GetWaitRealTime(blendDelay);
            onStart?.Invoke();
            SetCam(cameraType, blendDuration, blendStyle);
            await Task.Delay(TimeSpan.FromSeconds(blendDuration));

            // yield return WaiterHelpers.GetWaitRealTime(blendDuration);
            onEnd?.Invoke();
            _assignCoroutine = null;
        }


        public void ShakeCurrentCam(Action onShakeComplete = null, float intensity = 5f, float duration = .5f)
        {
            var cameraData = _virtualCameraDataDictionary[_currentCamera];
            if (!cameraData.canShake) return;
            cameraData.cameraShake.Shake(intensity, duration);
            onShakeComplete?.Invoke();
        }


        public void StopShakes()
        {
            var cameraData = _virtualCameraDataDictionary[_currentCamera];
            if (!cameraData.canShake) return;
            cameraData.cameraShake.StopShake();
        }
    }

    #region Camera Data

    [Serializable]
    public class VirtualCameraData
    {
        [Tooltip("Give a tag to the camera for calling it")]
        public CameraType cameraType;

        public CinemachineVirtualCamera virtualCamera;

        public bool canShake;

        protected internal CameraShake cameraShake;
    }

    public enum CameraType
    {
        Main,
    }

    #endregion
}