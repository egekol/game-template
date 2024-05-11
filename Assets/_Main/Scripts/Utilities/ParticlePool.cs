using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
namespace Utilities
{
	public class ParticlePool :  Singleton<ParticlePool>
	{
		[SerializeField] private List<ParticleClass> particles = new List<ParticleClass>();
		private Dictionary<string,ObjectPool<ParticleSystem>> _particleDictionary;

		private void Awake()
		{
			Initialize();
			CreateAtStart();
		}

		private void OnValidate()
		{
			if(!Application.isPlaying)
			{
				for(int i = 0; i < particles.Count; i++)
				{
					ParticleClass particleClass = particles[i];
					if(particleClass.prefab.Equals(null)) continue;
					var main = particleClass.prefab.main;
					main.stopAction = ParticleSystemStopAction.Disable;
					if(!particleClass.prefab.TryGetComponent(out ParticleCallBack particleCallBack))
					{
						particleClass.prefab.gameObject.AddComponent<ParticleCallBack>();
					}
				}
			}
		}

		private void CreateAtStart()
		{
			for(int i = 0; i < particles.Count; i++)
			{
				ParticleClass p = particles[i];
				if(!p.createAtStart) continue;
				for(int j = 0; j < p.softCap; j++)
				{
					_particleDictionary[p.tag].Get();
				}
			}
		}

		private void Initialize()
		{
			_particleDictionary = new Dictionary<string,ObjectPool<ParticleSystem>>();
			int listCount = particles.Count;
			for(int i = 0; i < listCount; i++)
			{
				ObjectPool<ParticleSystem> pool = new ObjectPool<ParticleSystem>(CreateFunction(i),OnParticleGet,
					OnParticleRelease,OnParticleDestroy,true,
					particles[i].softCap,particles[i].hardCap);
				string prefabName = particles[i].tag;
				_particleDictionary.Add(prefabName,pool);
			}
		}

		public ParticleSystem Spawn(string poolTag,Vector3 position)
		{
			ParticleSystem particle = _particleDictionary[poolTag].Get();
			Transform t = particle.transform;
			t.position = position;
			particle.Play();
			return particle;
		}

		public ParticleSystem Spawn(string poolTag,Vector3 position,Color color)
		{
			ParticleSystem particle = _particleDictionary[poolTag].Get();
			ChangeParticleColor(particle,color);
			Transform t = particle.transform;
			t.position = position;
			particle.Play();
			return particle;
		}

		public ParticleSystem Spawn(string poolTag,Vector3 position,Quaternion rotation)
		{
			ParticleSystem particle = _particleDictionary[poolTag].Get();
			Transform t = particle.transform;
			t.position = position;
			t.rotation = rotation;
			particle.Play();
			return particle;
		}

		public ParticleSystem Spawn(string poolTag,Transform parent)
		{
			ParticleSystem particle = _particleDictionary[poolTag].Get();
			Transform t = particle.transform;
			t.SetParent(parent);
			t.localPosition = Vector3.zero;
			particle.Play();
			return particle;
		}

		public ParticleSystem Spawn(string poolTag,Vector3 position,Transform parent)
		{
			ParticleSystem particle = _particleDictionary[poolTag].Get();
			Transform t = particle.transform;
			t.position = position;
			t.SetParent(parent);
			particle.gameObject.SetActive(true);
			particle.Play();
			return particle;
		}

		public ParticleSystem Spawn(string poolTag,Vector3 position,Quaternion rotation,Transform parent)
		{
			ParticleSystem particle = _particleDictionary[poolTag].Get();
			Transform t = particle.transform;
			t.position = position;
			t.rotation = rotation;
			t.SetParent(parent);
			particle.gameObject.SetActive(true);
			particle.Play();
			return particle;
		}

		private void OnParticleGet(ParticleSystem particle)
		{
			particle.gameObject.SetActive(true);
		}

		private void ChangeParticleColor(ParticleSystem particle,Color color)
		{
			ParticleSystem.MainModule mainSettings = particle.main;
			mainSettings.startColor = new ParticleSystem.MinMaxGradient(color);

			ParticleSystem[] particles = particle.GetComponentsInChildren<ParticleSystem>();

			for(int i = 0; i < particles.Length; i++)
			{
				mainSettings = particles[i].main;
				mainSettings.startColor = new ParticleSystem.MinMaxGradient(color);
			}
		}

		private Func<ParticleSystem> CreateFunction(int i)
		{
			return () =>
			{
				ParticleSystem particle = Instantiate(particles[i].prefab);
				ParticleCallBack callBack = particle.GetComponent<ParticleCallBack>();
				callBack.particleTag = particles[i].tag;
				return particle;
			};
		}

		private void OnParticleRelease(ParticleSystem particle)
		{
			particle.gameObject.SetActive(false);
		}

		private void OnParticleDestroy(ParticleSystem particle)
		{
			Destroy(particle.gameObject);
		}

		public void Release(ParticleSystem particle,string particleName)
		{
			_particleDictionary[particleName].Release(particle);
		}
	}

	[Serializable]
	public class ParticleClass
	{
		[Tooltip("Give a tag to the pool for calling it")]
		public string tag;

		[Tooltip("Prefab of the Particle to be pooled")]
		public ParticleSystem prefab;

		[Tooltip("The size (count) of the pool")]
		public int softCap,hardCap;

		[Tooltip("Whether the Particle create at Start")]
		public bool createAtStart;
	}
}