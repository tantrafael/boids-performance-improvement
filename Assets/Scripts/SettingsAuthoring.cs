using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Boids
{
	public class SettingsAuthoring : MonoBehaviour
	{
		public int boidCount;
		public float boidDensity;
		public int worldSizeRoundingIncrement;
		public float initialSpeed;
		public float viewRange;
		public float matchRate;
		public float coherenceRate;
		public float avoidanceRange;
		public float avoidanceRate;
		// TODO: Organize team settings in lists.
		public float thrustTeamRed;
		public float thrustTeamGreen;
		public float thrustTeamBlue;
		public float dragTeamRed;
		public float dragTeamGreen;
		public float dragTeamBlue;
		public float sizeTeamRed;
		public float sizeTeamGreen;
		public float sizeTeamBlue;
		public float4 colorTeamRed;
		public float4 colorTeamGreen;
		public float4 colorTeamBlue;
		public GameObject boidPrefab;

		private class Baker : Baker<SettingsAuthoring>
		{
			public override void Bake(SettingsAuthoring authoring)
			{
				var entity = GetEntity(TransformUsageFlags.None);

				var settings = new Settings
				{
					BoidCount = authoring.boidCount,
					WorldSize = GetWorldSize(authoring.boidCount, authoring.boidDensity, authoring.worldSizeRoundingIncrement),
					InitialSpeed = authoring.initialSpeed,
					ViewRange = authoring.viewRange,
					MatchRate = authoring.matchRate,
					CoherenceRate = authoring.coherenceRate,
					AvoidanceRange = authoring.avoidanceRange,
					AvoidanceRate = authoring.avoidanceRate,
					ThrustTeamRed = authoring.thrustTeamRed,
					ThrustTeamGreen = authoring.thrustTeamGreen,
					ThrustTeamBlue = authoring.thrustTeamBlue,
					DragTeamRed = authoring.dragTeamRed,
					DragTeamGreen = authoring.dragTeamGreen,
					DragTeamBlue = authoring.dragTeamBlue,
					SizeTeamRed = authoring.sizeTeamRed,
					SizeTeamGreen = authoring.sizeTeamGreen,
					SizeTeamBlue = authoring.sizeTeamBlue,
					ColorTeamRed = authoring.colorTeamRed,
					ColorTeamGreen = authoring.colorTeamGreen,
					ColorTeamBlue = authoring.colorTeamBlue,
					BoidPrefab = GetEntity(authoring.boidPrefab, TransformUsageFlags.Dynamic)
				};

				AddComponent(entity, settings);
			}

			private float GetWorldSize(int boidCount, float boidDensity, int worldSizeRoundingIncrement)
			{
				const float exponent = 1.0f / 3.0f;
				var power = math.pow(boidCount, exponent);
				var quotient = boidDensity / worldSizeRoundingIncrement;
				var ceiling = math.ceil(power * quotient);
				var worldSize = ceiling * worldSizeRoundingIncrement;

				return worldSize;
			}
		}
	}

	public struct Settings : IComponentData
	{
		public int BoidCount;
		public float WorldSize;
		public float InitialSpeed;
		public float ViewRange;
		public float MatchRate;
		public float CoherenceRate;
		public float AvoidanceRange;
		public float AvoidanceRate;
		// TODO: Organize team settings in lists.
		public float ThrustTeamRed;
		public float ThrustTeamGreen;
		public float ThrustTeamBlue;
		public float DragTeamRed;
		public float DragTeamGreen;
		public float DragTeamBlue;
		public float SizeTeamRed;
		public float SizeTeamGreen;
		public float SizeTeamBlue;
		public float4 ColorTeamRed;
		public float4 ColorTeamGreen;
		public float4 ColorTeamBlue;
		public Entity BoidPrefab;
	}
}
