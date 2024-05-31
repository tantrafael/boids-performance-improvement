using Unity.Entities;
using UnityEngine;

namespace Boids
{
	public class SettingsAuthoring : MonoBehaviour
	{
		public int roundWorldSizeToMultiplesOf;
		public int boidDensity;
		public int boidCount;
		public float initialVelocity;
		public float viewRange;
		public float matchRate;
		public float coherenceRate;
		public float avoidanceRange;
		public float avoidanceRate;
		public float thrust;
		public float drag;
		public GameObject boidPrefab;

		private class Baker : Baker<SettingsAuthoring>
		{
			public override void Bake(SettingsAuthoring authoring)
			{
				var entity = GetEntity(TransformUsageFlags.None);

				var settings = new Settings
				{
					RoundWorldSizeToMultiplesOf = authoring.roundWorldSizeToMultiplesOf,
					BoidDensity = authoring.boidDensity,
					BoidCount = authoring.boidCount,
					InitialVelocity = authoring.initialVelocity,
					ViewRange = authoring.viewRange,
					MatchRate = authoring.matchRate,
					CoherenceRate = authoring.coherenceRate,
					AvoidanceRange = authoring.avoidanceRange,
					AvoidanceRate = authoring.avoidanceRate,
					Thrust = authoring.thrust,
					Drag = authoring.drag,
					BoidPrefab = GetEntity(authoring.boidPrefab, TransformUsageFlags.Dynamic)
				};

				AddComponent(entity, settings);
			}
		}
	}

	public struct Settings : IComponentData
	{
		public int RoundWorldSizeToMultiplesOf;
		public int BoidDensity;
		public int BoidCount;
		public float InitialVelocity;
		public float ViewRange;
		public float MatchRate;
		public float CoherenceRate;
		public float AvoidanceRange;
		public float AvoidanceRate;
		public float Thrust;
		public float Drag;
		public Entity BoidPrefab;
	}
}
