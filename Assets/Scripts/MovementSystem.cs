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
			var boxQuery = SystemAPI.QueryBuilder().WithAll<LocalTransform>().Build();

			// More complex solution, but it avoids creating temporary copies of the box components
			new CollisionJob
			{
				LocalTransformTypeHandle = SystemAPI.GetComponentTypeHandle<LocalTransform>(),
				MovementTypeHandle = SystemAPI.GetComponentTypeHandle<Movement>(),
				EntityTypeHandle = SystemAPI.GetEntityTypeHandle(),
				OtherChunks = boxQuery.ToArchetypeChunkArray(state.WorldUpdateAllocator)
			}.ScheduleParallel(boxQuery, state.Dependency).Complete();
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
			const float halfWorldSize = 20.0f;
			const float viewRange = 3.0f;
			const float matchRate = 1.0f;
			const float coherenceRate = 2.0f;
			const float avoidanceRange = 2.0f;
			const float avoidanceRate = 5.0f;
			const float dt = 0.01f;

			var transforms = chunk.GetNativeArray(ref LocalTransformTypeHandle);
			var movements = chunk.GetNativeArray(ref MovementTypeHandle);
			var entities = chunk.GetNativeArray(EntityTypeHandle);

			for (int i = 0; i < transforms.Length; i++)
			{
				var transform = transforms[i];
				var movement = movements[i];
				var entity = entities[i];

				// Find neighbors.
				///////////////////////////////////////////////////////////////
				// var neighborVelocities = new NativeList<float3>(Allocator.Temp);
				var neighbors = new NativeList<Neighbor>(Allocator.Temp);

				for (var j = 0; j < OtherChunks.Length; j++)
				{
					var otherChunk = OtherChunks[j];
					var otherTransforms = otherChunk.GetNativeArray(ref LocalTransformTypeHandle);
					var otherMovements = otherChunk.GetNativeArray(ref MovementTypeHandle);
					var otherEntities = otherChunk.GetNativeArray(EntityTypeHandle);

					for (var k = 0; k < otherChunk.Count; k++)
					{
						var otherTransform = otherTransforms[k];
						var otherMovement = otherMovements[k];
						var otherEntity = otherEntities[k];
						// TODO: Use distance and viewRange squared.
						// var distance = math.distancesq(transform.Position, otherTranslation.Position);
						var distance = math.distance(transform.Position, otherTransform.Position);
						var isOtherEntity = (entity != otherEntity);
						var isWithinRadius = (distance < viewRange);
						var isOtherEntityWithinRadius = (isOtherEntity && isWithinRadius);

						if (isOtherEntityWithinRadius)
						{
							// neighborVelocities.Add(otherMovement.Velocity);
							var neighbor = new Neighbor
							{
								Position = otherTransform.Position,
								Velocity = otherMovement.Velocity
							};

							neighbors.Add(neighbor);
						}
					}
				}

				// Keep within world bounds.
				///////////////////////////////////////////////////////////////
				// TODO: Remove magic number 5.0f,
				var velocity = new float3
				{
					x = math.max(math.abs(transform.Position.x) + viewRange - halfWorldSize, 0.0f) * math.sign(transform.Position.x) * 5.0f * dt,
					y = math.max(math.abs(transform.Position.y) + viewRange - halfWorldSize, 0.0f) * math.sign(transform.Position.y) * 5.0f * dt,
					z = math.max(math.abs(transform.Position.z) + viewRange - halfWorldSize, 0.0f) * math.sign(transform.Position.z) * 5.0f * dt
				};

				movement.Velocity -= velocity;
				movements[i] = movement;

				var neighborCount = neighbors.Length;

				// Match velocity.
				///////////////////////////////////////////////////////////////
				if (neighborCount > 0)
				{
					var neighborMeanVelocity = float3.zero;

					foreach (var neighbor in neighbors)
					{
						neighborMeanVelocity += neighbor.Velocity;
					}

					// var neighborMeanVelocity = neighborVelocities.Aggregate(float3.zero, (current, neighbor) => current + neighbor);

					// neighborMeanVelocity /= neighborVelocities.Length;
					neighborMeanVelocity /= neighborCount;
					movement.Velocity += (neighborMeanVelocity - movement.Velocity) * matchRate * dt;
					movements[i] = movement;
				}

				// Update coherence.
				///////////////////////////////////////////////////////////////
				if (neighborCount > 0)
				{
					var neighborMeanPosition = neighbors[0].Position;

					for (var neighborIndex = 1; neighborIndex < neighborCount; neighborIndex++)
					{
						neighborMeanPosition += neighbors[neighborIndex].Position;
					}

					neighborMeanPosition /= neighborCount;
					movement.Velocity += (neighborMeanPosition - transform.Position) * coherenceRate * dt;
					movements[i] = movement;
				}

				// Avoid others.
				///////////////////////////////////////////////////////////////
				if (neighborCount > 0)
				{
					/*
					var myPosition = boid.Position;
					var minDistSqr = minDist * minDist;
					Vector3 step = Vector3.zero;
					for (int i = 0; i < neighbours.Count; ++i)
					{
						var delta = myPosition - neighbours[i].Position;
						var deltaSqr = delta.sqrMagnitude;
						if (deltaSqr > 0 && deltaSqr < minDistSqr)
						{
							step += delta / Mathf.Sqrt(deltaSqr);
						}
					}
					boid.Velocity += step * avoidanceRate * dt;
					*/

					var minDist = avoidanceRange;
					var myPosition = transform.Position;
					var minDistSqr = minDist * minDist;
					var step = float3.zero;

					for (int neigborIndex = 0; neigborIndex < neighborCount; neigborIndex++)
					{
						var delta = myPosition - neighbors[neigborIndex].Position;
						var deltaSqr = math.length(delta);

						if ((deltaSqr > 0) && (deltaSqr < minDistSqr))
						{
							step += delta / math.sqrt(deltaSqr);
						}
					}

					movement.Velocity += step * avoidanceRate * dt;
					movements[i] = movement;
				}

				// Update position.
				///////////////////////////////////////////////////////////////
				transform.Position.xyz += movement.Velocity * dt;
				transforms[i] = transform;
			}
		}
	}

	public struct Neighbor
	{
		public float3 Position;
		public float3 Velocity;
	}
}
