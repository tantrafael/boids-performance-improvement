using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Boids
{
	public class SettingsAuthoring : MonoBehaviour
	{
		/*
		public int roundWorldSizeToMultiplesOf;
		public float boidDensity;
		public int boidCount;
		public float initialSpeed;
		public float viewRange;
		public float matchRate;
		public float coherenceRate;
		public float avoidanceRange;
		public float avoidanceRate;
		// public float[] thrustTable;
		public float thrustTeamRed;
		public float thrustTeamGreen;
		public float thrustTeamBlue;
		public float dragTeamRed;
		public float dragTeamGreen;
		public float dragTeamBlue;
		public GameObject boidPrefab;
		*/
		public int boidCount;
		public float boidDensity;
		public int roundWorldSizeToMultiplesOf;
		public float initialSpeed;
		public float viewRange;
		public float matchRate;
		public float coherenceRate;
		public float avoidanceRange;
		public float avoidanceRate;
		// public float[] thrustTable;
		public float thrustTeamRed;
		public float thrustTeamGreen;
		public float thrustTeamBlue;
		public float dragTeamRed;
		public float dragTeamGreen;
		public float dragTeamBlue;
		public GameObject boidPrefab;

		private class Baker : Baker<SettingsAuthoring>
		{
			public override void Bake(SettingsAuthoring authoring)
			{
				var entity = GetEntity(TransformUsageFlags.None);

				const float exponent = 1.0f / 3.0f;
				var a = math.pow(authoring.boidCount, exponent);
				var b = authoring.boidDensity / authoring.roundWorldSizeToMultiplesOf;
				var c = math.ceil(a * b);
				var worldSize = c * authoring.roundWorldSizeToMultiplesOf;

				/*
				var settings = new Settings
				{
					// RoundWorldSizeToMultiplesOf = authoring.roundWorldSizeToMultiplesOf,
					// BoidDensity = authoring.boidDensity,
					BoidCount = authoring.boidCount,
					InitialSpeed = authoring.initialSpeed,
					ViewRange = authoring.viewRange,
					MatchRate = authoring.matchRate,
					CoherenceRate = authoring.coherenceRate,
					AvoidanceRange = authoring.avoidanceRange,
					AvoidanceRate = authoring.avoidanceRate,
					// ThrustTable = authoring.thrustTable,
					ThrustTeamRed = authoring.thrustTeamRed,
					ThrustTeamGreen = authoring.thrustTeamGreen,
					ThrustTeamBlue = authoring.thrustTeamBlue,
					DragTeamRed = authoring.dragTeamRed,
					DragTeamGreen = authoring.dragTeamGreen,
					DragTeamBlue = authoring.dragTeamBlue,
					BoidPrefab = GetEntity(authoring.boidPrefab, TransformUsageFlags.Dynamic),
					// WorldSize = 40.0f
					WorldSize = worldSize
				};
				*/
				var settings = new Settings
				{
					BoidCount = authoring.boidCount,
					WorldSize = worldSize,
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
					BoidPrefab = GetEntity(authoring.boidPrefab, TransformUsageFlags.Dynamic)
				};

				AddComponent(entity, settings);
			}
		}
	}

	/*
	public struct Settings : IComponentData
	{
		// public int RoundWorldSizeToMultiplesOf;
		// public float BoidDensity;
		public int BoidCount;
		public float InitialSpeed;
		public float ViewRange;
		public float MatchRate;
		public float CoherenceRate;
		public float AvoidanceRange;
		public float AvoidanceRate;
		public float ThrustTeamRed;
		public float ThrustTeamGreen;
		public float ThrustTeamBlue;
		public float DragTeamRed;
		public float DragTeamGreen;
		public float DragTeamBlue;
		public Entity BoidPrefab;
		public float WorldSize;
	}
	*/
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
		public float ThrustTeamRed;
		public float ThrustTeamGreen;
		public float ThrustTeamBlue;
		public float DragTeamRed;
		public float DragTeamGreen;
		public float DragTeamBlue;
		public Entity BoidPrefab;
	}
}
