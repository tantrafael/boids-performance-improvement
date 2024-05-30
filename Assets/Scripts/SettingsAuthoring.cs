using Unity.Entities;
using UnityEngine;

namespace Boids
{
	public class SettingsAuthoring : MonoBehaviour
	{
		public int boidDensity;
		public int boidCount;
		public float initialVelocity;
		public float viewRange;
		public GameObject boidPrefab;
		public int roundWorldSizeToMultiplesOf;

		private class Baker : Baker<SettingsAuthoring>
		{
			public override void Bake(SettingsAuthoring authoring)
			{
				var entity = GetEntity(TransformUsageFlags.None);

				var settings = new Settings
				{
					BoidDensity = authoring.boidDensity,
					BoidCount = authoring.boidCount,
					InitialVelocity = authoring.initialVelocity,
					ViewRange = authoring.viewRange,
					BoidPrefab = GetEntity(authoring.boidPrefab, TransformUsageFlags.Dynamic),
					RoundWorldSizeToMultiplesOf = authoring.roundWorldSizeToMultiplesOf
				};

				AddComponent(entity, settings);
			}
		}
	}

	public struct Settings : IComponentData
	{
		public int BoidDensity;
		public int BoidCount;
		public float InitialVelocity;
		public float ViewRange;
		public Entity BoidPrefab;
		public int RoundWorldSizeToMultiplesOf;
	}
}
