using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids
{
	public partial struct InitializationSystem : ISystem
	{
		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<Settings>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			state.Enabled = false;

			/*
			// Decide world size based on boid count and density
			int worldSize =
				Mathf.CeilToInt(Mathf.Pow(boidCount, 1.0f / 3) * BOID_DENSITY / ROUND_WORLD_SIZE_TO_MULTIPLES_OF) *
				ROUND_WORLD_SIZE_TO_MULTIPLES_OF;
			*/
			const int worldSize = 40;

			var settings = SystemAPI.GetSingleton<Settings>();
			// TODO: Remove magic number.
			var random = Random.CreateFromIndex(1234);

			Spawn(ref state, settings.BoidPrefab, settings.BoidCount, worldSize, settings.InitialVelocity, ref random);
		}

		private void Spawn(ref SystemState state, Entity prefab, int count, int worldSize, float initialVelocity, ref Random random)
		{
			var units = state.EntityManager.Instantiate(prefab, count, Allocator.Temp);

			foreach (var unit in units)
			{
				// Position
				var relativePosition = new float3 { xyz = (random.NextFloat3() - 0.5f) * 2.0f };
				// TODO: Remove magic number 3.0f.
				var magnitude = worldSize * 0.5f - 3.0f;
				var absolutePosition = relativePosition * magnitude;
				var localTransform = new LocalTransform { Position = absolutePosition, Scale = 1.0f };
				state.EntityManager.SetComponentData(unit, localTransform);

				// Velocity
				var movement = new Movement { Velocity = random.NextFloat3Direction() * initialVelocity };
				state.EntityManager.SetComponentData(unit, movement);
			}
		}
	}
}
