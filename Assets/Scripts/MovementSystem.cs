using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Boids
{
	public partial struct MovementSystem : ISystem
	{
		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<Settings>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			// var boidQuery = SystemAPI.QueryBuilder().WithAll<LocalTransform>().Build();
			var boidQuery = SystemAPI.QueryBuilder().WithAll<Movement>().Build();
			// var boidQuery = SystemAPI.QueryBuilder().WithAll<TeamRed>().Build();

			var movementUpdateJob = new MovementUpdateJob
			{
				LocalTransformTypeHandle = SystemAPI.GetComponentTypeHandle<LocalTransform>(),
				MovementTypeHandle = SystemAPI.GetComponentTypeHandle<Movement>(),
				EntityTypeHandle = SystemAPI.GetEntityTypeHandle(),
				OtherChunks = boidQuery.ToArchetypeChunkArray(state.WorldUpdateAllocator),
				Settings = SystemAPI.GetSingleton<Settings>(),
				DeltaTime = SystemAPI.Time.DeltaTime
			};

			movementUpdateJob.ScheduleParallel(boidQuery, state.Dependency).Complete();
		}
	}

	[BurstCompile]
	public struct MovementUpdateJob : IJobChunk
	{
		public ComponentTypeHandle<LocalTransform> LocalTransformTypeHandle;
		public ComponentTypeHandle<Movement> MovementTypeHandle;
		[ReadOnly] public EntityTypeHandle EntityTypeHandle;
		[ReadOnly] public NativeArray<ArchetypeChunk> OtherChunks;
		[ReadOnly] public Settings Settings;
		[ReadOnly] public float DeltaTime;

		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			// TODO: Get world size from elsewhere.
			const float worldSize = 40;

			var transforms = chunk.GetNativeArray(ref LocalTransformTypeHandle);
			var movements = chunk.GetNativeArray(ref MovementTypeHandle);
			var entities = chunk.GetNativeArray(EntityTypeHandle);

			for (var boidIndex = 0; boidIndex < transforms.Length; boidIndex++)
			{
				var transform = transforms[boidIndex];
				var movement = movements[boidIndex];
				var entity = entities[boidIndex];

				var neighbors =
					BoidBehavior.FindNeighbors(transform, entity, OtherChunks, LocalTransformTypeHandle,
						MovementTypeHandle, EntityTypeHandle, Settings.ViewRange);

				var totalAcceleration = BoidBehavior.GetTotalAcceleration(transform.Position, movement.Velocity,
					worldSize, neighbors, Settings);

				var deltaVelocity = totalAcceleration * DeltaTime;
				var velocity = movement.Velocity + deltaVelocity;
				var deltaPosition = velocity * DeltaTime;
				var position = transform.Position + deltaPosition;

				transform.Position = position;
				movement.Velocity = velocity;

				transforms[boidIndex] = transform;
				movements[boidIndex] = movement;
			}
		}
	}
}
