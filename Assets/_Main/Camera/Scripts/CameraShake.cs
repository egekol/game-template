using System.Collections;
using Cinemachine;
using UnityEngine;

namespace _Main.Camera.Scripts
{
    public class CameraShake : MonoBehaviour
    {
        private CinemachineVirtualCamera virtualCamera;
        private CinemachineBasicMultiChannelPerlin perlin;

        private float shakeIntensity = 5;
        private float shakeFrequency = 1;
        private float shakeDuration = .5f;
        private float timer;

        private Coroutine shakeCoroutine;


        private void Awake()
        {
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
            perlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (perlin is null)
            {
                Debug.LogError("Virtual Camera noise must BasicMultiChannelPerlin and Noise Profile must 6D Shake",
                    transform);
                return;
            }

            perlin.m_AmplitudeGain = 0;
        }

        public void StopShake()
        {
            if (shakeCoroutine is not null)
                StopCoroutine(shakeCoroutine);
        }


        public void Shake(float intensity, float duration, float frequency = 1)
        {
            if (perlin is null)
            {
                Debug.LogError("Virtual Camera noise must BasicMultiChannelPerlin and Noise Profile must 6D Shake",
                    transform);
                return;
            }

            shakeIntensity = intensity;
            shakeFrequency = frequency;
            shakeDuration = duration;
            timer = duration;

            StopShake();
            shakeCoroutine = StartCoroutine(ShakerCor());
        }

        public void LerpShake(float intensity, float duration, float frequency = 1)
        {
            if (perlin is null)
            {
                Debug.LogError("Virtual Camera noise must BasicMultiChannelPerlin and Noise Profile must 6D Shake",
                    transform);
                return;
            }

            shakeIntensity = intensity;
            shakeFrequency = frequency;
            shakeDuration = duration;
            timer = duration;

            StopShake();
            shakeCoroutine = StartCoroutine(LerpShakerCor());
        }

        #region Coroutines

        private IEnumerator LerpShakerCor()
        {
            perlin.m_AmplitudeGain = shakeIntensity;
            perlin.m_FrequencyGain = shakeFrequency;
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                perlin.m_AmplitudeGain = Mathf.Lerp(shakeIntensity, 0, 1 - (timer / shakeDuration));
                yield return new WaitForSeconds(Time.deltaTime); 
            }

            perlin.m_AmplitudeGain = 0;
            perlin.m_FrequencyGain = 1;
        }

        private IEnumerator ShakerCor()
        {
            perlin.m_AmplitudeGain = shakeIntensity;
            perlin.m_FrequencyGain = shakeFrequency;
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                perlin.m_AmplitudeGain = shakeIntensity;
                yield return new WaitForSeconds(Time.deltaTime); 
            }

            perlin.m_AmplitudeGain = 0;
            perlin.m_FrequencyGain = 1;
        }

        #endregion
    }
}