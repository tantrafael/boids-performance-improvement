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

			// More complex solution, but it avoids creating temporary copies of the box components.
			new CollisionJob
			{
				LocalTransformTypeHandle = SystemAPI.GetComponentTypeHandle<LocalTransform>(),
				MovementTypeHandle = SystemAPI.GetComponentTypeHandle<Movement>(),
				EntityTypeHandle = SystemAPI.GetEntityTypeHandle(),
				OtherChunks = boidQuery.ToArchetypeChunkArray(state.WorldUpdateAllocator)
			}.ScheduleParallel(boidQuery, state.Dependency).Complete();
		}
	}

	[BurstCompile]
	public struct CollisionJob : IJobChunk
	{
		public ComponentTypeHandle<LocalTransform> LocalTransformTypeHandle;
		public ComponentTypeHandle<Movement> MovementTypeHandle;
		[ReadOnly] public EntityTypeHandle EntityTypeHandle;

		[ReadOnly] public NativeArray<ArchetypeChunk> OtherChunks;

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
			// const float drag = 0.02f;
			const float drag = 0.01f;
			const float deltaTime = 0.01f;

			var transforms = chunk.GetNativeArray(ref LocalTransformTypeHandle);
			var movements = chunk.GetNativeArray(ref MovementTypeHandle);
			var entities = chunk.GetNativeArray(EntityTypeHandle);

			for (var boidIndex = 0; boidIndex < transforms.Length; boidIndex++)
			{
				var transform = transforms[boidIndex];
				var movement = movements[boidIndex];
				var entity = entities[boidIndex];

				// Find neighbors.
				///////////////////////////////////////////////////////////////
				var neighbors = new NativeList<Neighbor>(Allocator.Temp);

				foreach (var otherChunk in OtherChunks)
				{
					var otherTransforms = otherChunk.GetNativeArray(ref LocalTransformTypeHandle);
					var otherMovements = otherChunk.GetNativeArray(ref MovementTypeHandle);
					var otherEntities = otherChunk.GetNativeArray(EntityTypeHandle);

					for (var otherChunkIndex = 0; otherChunkIndex < otherChunk.Count; otherChunkIndex++)
					{
						var otherTransform = otherTransforms[otherChunkIndex];
						var otherMovement = otherMovements[otherChunkIndex];
						var otherEntity = otherEntities[otherChunkIndex];
						// TODO: Use distance and viewRange squared.
						// var distance = math.distancesq(transform.Position, otherTranslation.Position);
						var distance = math.distance(transform.Position, otherTransform.Position);
						var isOtherEntity = (entity != otherEntity);
						var isWithinRadius = (distance < viewRange);
						var isOtherEntityWithinRadius = (isOtherEntity && isWithinRadius);

						if (isOtherEntityWithinRadius)
						{
							var neighbor = new Neighbor
							{
								Position = otherTransform.Position,
								Velocity = otherMovement.Velocity
							};

							neighbors.Add(neighbor);
						}
					}
				}

				var boundRespectingAcceleration = BoidBehavior.GetBoundRespectingAcceleration(transform.Position, worldSize, viewRange);
				var velocityMatchingAcceleration = BoidBehavior.GetVelocityMatvingAcceleration(movement.Velocity, neighbors, matchRate);
				var coherenceAcceleration = BoidBehavior.GetCoherenceAcceleration(transform.Position, neighbors, coherenceRate);
				var collisionAvoidanceAcceleration = BoidBehavior.GetCollisionAvoidanceAcceleration(transform.Position, neighbors, avoidanceRange, avoidanceRate);
				var thrustAcceleration = BoidBehavior.GetThrustAcceleration(movement.Velocity, thrust);
				var dragAcceleration = BoidBehavior.GetDragAcceleration(movement.Velocity, drag);

				var totalAcceleration = boundRespectingAcceleration + velocityMatchingAcceleration +
				                        coherenceAcceleration + collisionAvoidanceAcceleration + thrustAcceleration +
				                        dragAcceleration;

				var deltaVelocity = totalAcceleration * deltaTime;
				var velocity = movement.Velocity + deltaVelocity;
				var deltaPosition = velocity * deltaTime;
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
