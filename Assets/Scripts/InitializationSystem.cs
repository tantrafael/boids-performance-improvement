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

			var settings = SystemAPI.GetSingleton<Settings>();

			// TODO: Determine team count from settings.
			const int teamCount = 3;

			var teamSizes = new NativeList<float>(Allocator.Temp);
			teamSizes.Add(settings.SizeTeamRed);
			teamSizes.Add(settings.SizeTeamGreen);
			teamSizes.Add(settings.SizeTeamBlue);

			var teamColors = new NativeList<float4>(Allocator.Temp);
			teamColors.Add(settings.ColorTeamRed);
			teamColors.Add(settings.ColorTeamGreen);
			teamColors.Add(settings.ColorTeamBlue);

			// TODO: Remove magic number 1234.
			var random = Random.CreateFromIndex(1234);

			Spawn(ref state, settings.BoidPrefab, settings.BoidCount, settings.WorldSize, settings.ViewRange,
				settings.InitialSpeed, teamCount, teamSizes, teamColors, ref random);
		}

		private void Spawn(ref SystemState state, Entity prefab, int boidCount, float worldSize, float viewRange,
			float initialSpeed, int teamCount, NativeList<float> teamSizes, NativeList<float4> teamColors, ref Random random)
		{
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
				var scale = teamSizes[teamIndex];

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
