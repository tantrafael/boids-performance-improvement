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
			var entities = state.EntityManager.Instantiate(prefab, boidCount, Allocator.Temp);

			foreach (var entity in entities)
			{
				// Position
				var relativePosition = new float3 { xyz = (random.NextFloat3() - 0.5f) * 2.0f };
				var magnitude = worldSize * 0.5f - viewRange;
				var absolutePosition = relativePosition * magnitude;
				var localTransform = new LocalTransform { Position = absolutePosition, Scale = 1.0f };
				state.EntityManager.SetComponentData(entity, localTransform);

				// Velocity
				var initialVelocity = random.NextFloat3Direction() * initialSpeed;

				// Team
				// TODO: Get team count from elsewhere.
				const int teamCount = 3;
				var teamIndex = random.NextInt(teamCount);

				// TODO: Assign team using component tag rather than storing it with each entity,
				// for efficient team selection.

				/*
				switch (teamIndex)
				{
					case 0:
						state.EntityManager.AddComponent<TeamRed>(entity);
						break;

					case 1:
						state.EntityManager.AddComponent<TeamGreen>(entity);
						break;

					case 2:
						state.EntityManager.AddComponent<TeamBlue>(entity);
						break;
				}
				*/

				var movement = new Movement { Velocity = initialVelocity, Team = teamIndex };
				state.EntityManager.SetComponentData(entity, movement);

				// TODO: Get team colors from elsewhere.
				var teamColors = new NativeList<float4>(Allocator.Temp);
				teamColors.Add(new float4(1.0f, 0.0f, 0.0f, 1.0f));
				teamColors.Add(new float4(0.0f, 1.0f, 0.0f, 1.0f));
				teamColors.Add(new float4(0.0f, 0.0f, 1.0f, 1.0f));

				var color = new URPMaterialPropertyBaseColor { Value = teamColors[teamIndex] };
				state.EntityManager.SetComponentData(entity, color);
			}
		}
	}
}
