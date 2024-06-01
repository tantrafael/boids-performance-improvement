using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Boids
{
	public class SettingsAuthoring : MonoBehaviour
	{
		public int roundWorldSizeToMultiplesOf;
		public int boidDensity;
		public int boidCount;
		public float initialSpeed;
		public float viewRange;
		public float matchRate;
		public float coherenceRate;
		public float avoidanceRange;
		public float avoidanceRate;
		public float thrust;
		public float drag;
		// public float[] thrustTable;
		// public NativeArray<float> thrustTable;
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

				/*
				var foo = new float[3];
				foo[0] = 1.0f;
				foo[1] = 10.0f;

				// var bar = new NativeArray<float>(foo, Allocator.Temp);
				var bar = new NativeArray<float>(foo, Allocator.Persistent);
				*/

				var settings = new Settings
				{
					RoundWorldSizeToMultiplesOf = authoring.roundWorldSizeToMultiplesOf,
					BoidDensity = authoring.boidDensity,
					BoidCount = authoring.boidCount,
					InitialSpeed = authoring.initialSpeed,
					ViewRange = authoring.viewRange,
					MatchRate = authoring.matchRate,
					CoherenceRate = authoring.coherenceRate,
					AvoidanceRange = authoring.avoidanceRange,
					AvoidanceRate = authoring.avoidanceRate,
					Thrust = authoring.thrust,
					Drag = authoring.drag,
					// ThrustTable = authoring.thrustTable,
					// ThrustTable = bar,
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

	public struct Settings : IComponentData
	{
		public int RoundWorldSizeToMultiplesOf;
		public int BoidDensity;
		public int BoidCount;
		public float InitialSpeed;
		public float ViewRange;
		public float MatchRate;
		public float CoherenceRate;
		public float AvoidanceRange;
		public float AvoidanceRate;
		public float Thrust;
		public float Drag;
		// public float[] ThrustTable;
		// public NativeArray<float> ThrustTable;
		public float ThrustTeamRed;
		public float ThrustTeamGreen;
		public float ThrustTeamBlue;
		public float DragTeamRed;
		public float DragTeamGreen;
		public float DragTeamBlue;
		public Entity BoidPrefab;
	}

	/*
	public struct Foo
	{
		public NativeArray<float> Bar;
	}
	*/
}
