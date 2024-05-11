using UnityEngine;
namespace Utilities
{
	public class ParticleCallBack : MonoBehaviour
	{
		[SerializeField] private ParticleSystem particle;
		public string particleTag;
		private void OnDisable()
		{
			if(ParticlePool.Instance) ParticlePool.Instance.Release(particle,particleTag);
		}

		private void OnValidate()
		{
			particle = GetComponent<ParticleSystem>();
		}
	}
}