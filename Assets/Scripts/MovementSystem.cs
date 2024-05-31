using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids
{
	public partial struct MovementSystem : ISystem
	{
		/*
		[BurstCompile]
		public void OnCreate(ref SystemState state) { }

		[BurstCompile]
		public void OnDestroy(ref SystemState state) { }
		*/

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var boidQuery = SystemAPI.QueryBuilder().WithAll<LocalTransform>().Build();

			var movementUpdateJob = new MovementUpdateJob
			{
				LocalTransformTypeHandle = SystemAPI.GetComponentTypeHandle<LocalTransform>(),
				MovementTypeHandle = SystemAPI.GetComponentTypeHandle<Movement>(),
				EntityTypeHandle = SystemAPI.GetEntityTypeHandle(),
				OtherChunks = boidQuery.ToArchetypeChunkArray(state.WorldUpdateAllocator),
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
		[ReadOnly] public float DeltaTime;

		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			// TODO: Get values from settings.
			const float worldSize = 80.0f;
			const float viewRange = 3.0f;
			const float matchRate = 1.0f;
			const float coherenceRate = 2.0f;
			const float avoidanceRange = 2.0f;
			const float avoidanceRate = 5.0f;
			const float thrust = 4.0f;
			const float drag = 0.02f;

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
						MovementTypeHandle, EntityTypeHandle, viewRange);

				var boundRespectingAcceleration =
					BoidBehavior.GetBoundRespectingAcceleration(transform.Position, worldSize, viewRange);

				var velocityMatchingAcceleration =
					BoidBehavior.GetVelocityMatvingAcceleration(movement.Velocity, neighbors, matchRate);

				var coherenceAcceleration =
					BoidBehavior.GetCoherenceAcceleration(transform.Position, neighbors, coherenceRate);

				var collisionAvoidanceAcceleration =
					BoidBehavior.GetCollisionAvoidanceAcceleration(transform.Position, neighbors, avoidanceRange,
						avoidanceRate);

				var thrustAcceleration = BoidBehavior.GetThrustAcceleration(movement.Velocity, thrust);

				var dragAcceleration = BoidBehavior.GetDragAcceleration(movement.Velocity, drag);

				var totalAcceleration = boundRespectingAcceleration + velocityMatchingAcceleration +
				                        coherenceAcceleration + collisionAvoidanceAcceleration + thrustAcceleration +
				                        dragAcceleration;

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

	public struct Neighbor
	{
		public float3 Position;
		public float3 Velocity;
	}
}
