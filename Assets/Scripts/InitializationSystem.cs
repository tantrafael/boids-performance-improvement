using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
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
			// Determine world size from boid count and density.
			int worldSize =
				Mathf.CeilToInt(Mathf.Pow(boidCount, 1.0f / 3) * BOID_DENSITY / ROUND_WORLD_SIZE_TO_MULTIPLES_OF) *
				ROUND_WORLD_SIZE_TO_MULTIPLES_OF;
			*/
			// TODO: Calculate world size.
			const float worldSize = 40;

			var settings = SystemAPI.GetSingleton<Settings>();
			// TODO: Remove magic number 1234.
			var random = Random.CreateFromIndex(1234);

			Spawn(ref state, settings.BoidPrefab, settings.BoidCount, worldSize, settings.ViewRange,
				settings.InitialSpeed, ref random);
		}

		private void Spawn(ref SystemState state, Entity prefab, int boidCount, float worldSize, float viewRange,
			float initialSpeed, ref Random random)
		{
			// TODO: Get team settings from elsewhere.
			const int teamCount = 3;

			var teamColors = new NativeList<float4>(Allocator.Temp);
			teamColors.Add(new float4(1.0f, 0.0f, 0.0f, 1.0f));
			teamColors.Add(new float4(0.0f, 1.0f, 0.0f, 1.0f));
			teamColors.Add(new float4(0.0f, 0.0f, 1.0f, 1.0f));

			var teamAgentSizes = new NativeList<float>(Allocator.Temp);
			teamAgentSizes.Add(0.6f);
			teamAgentSizes.Add(1.0f);
			teamAgentSizes.Add(0.4f);

			var entities = state.EntityManager.Instantiate(prefab, boidCount, Allocator.Temp);

			foreach (var entity in entities)
			{
				// Team
				var teamIndex = random.NextInt(teamCount);

				// Position
				var relativePosition = new float3 { xyz = (random.NextFloat3() - 0.5f) * 2.0f };
				var magnitude = worldSize * 0.5f - viewRange;
				var absolutePosition = relativePosition * magnitude;

				// Size
				var scale = teamAgentSizes[teamIndex];

				var localTransform = new LocalTransform { Position = absolutePosition, Scale = scale };
				state.EntityManager.SetComponentData(entity, localTransform);

				// Velocity
				var initialVelocity = random.NextFloat3Direction() * initialSpeed;
				var movement = new Movement { Velocity = initialVelocity, Team = teamIndex };
				state.EntityManager.SetComponentData(entity, movement);

				// Color
				var color = new URPMaterialPropertyBaseColor { Value = teamColors[teamIndex] };
				state.EntityManager.SetComponentData(entity, color);
			}
		}
	}
}
