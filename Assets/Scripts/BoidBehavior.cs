using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids
{
	[BurstCompile]
	public static class BoidBehavior
	{
		/*
		public static NativeList<Neighbor> FindNeighbors(LocalTransform transform, Entity entity,
			NativeArray<ArchetypeChunk> otherChunks, ComponentTypeHandle<LocalTransform> localTransformTypeHandle,
			ComponentTypeHandle<Movement> movementTypeHandle, EntityTypeHandle entityTypeHandle, float viewRange)
		{
			var neighbors = new NativeList<Neighbor>(Allocator.Temp);
			var viewRangeSquared = math.square(viewRange);

			foreach (var otherChunk in otherChunks)
			{
				var otherTransforms = otherChunk.GetNativeArray(ref localTransformTypeHandle);
				var otherMovements = otherChunk.GetNativeArray(ref movementTypeHandle);
				var otherEntities = otherChunk.GetNativeArray(entityTypeHandle);

				for (var otherChunkIndex = 0; otherChunkIndex < otherChunk.Count; otherChunkIndex++)
				{
					var otherTransform = otherTransforms[otherChunkIndex];
					var otherMovement = otherMovements[otherChunkIndex];
					var otherEntity = otherEntities[otherChunkIndex];
					var distanceSquared = math.distancesq(transform.Position, otherTransform.Position);
					var isOtherEntity = (entity != otherEntity);
					var isWithinRadius = (distanceSquared < viewRangeSquared);
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

			return neighbors;
		}
		*/
		public static void FindNeighbors(LocalTransform transform, Movement movement, Entity entity,
			NativeArray<ArchetypeChunk> otherChunks, ComponentTypeHandle<LocalTransform> localTransformTypeHandle,
			ComponentTypeHandle<Movement> movementTypeHandle, EntityTypeHandle entityTypeHandle, float viewRange,
			out NativeList<Neighbor> neighbors, out NativeList<Neighbor> teamNeighbors)
		{
			neighbors = new NativeList<Neighbor>(Allocator.Temp);
			teamNeighbors = new NativeList<Neighbor>(Allocator.Temp);
			var viewRangeSquared = math.square(viewRange);

			foreach (var otherChunk in otherChunks)
			{
				var otherTransforms = otherChunk.GetNativeArray(ref localTransformTypeHandle);
				var otherMovements = otherChunk.GetNativeArray(ref movementTypeHandle);
				var otherEntities = otherChunk.GetNativeArray(entityTypeHandle);

				for (var otherChunkIndex = 0; otherChunkIndex < otherChunk.Count; otherChunkIndex++)
				{
					var otherTransform = otherTransforms[otherChunkIndex];
					var otherMovement = otherMovements[otherChunkIndex];
					var otherEntity = otherEntities[otherChunkIndex];
					var distanceSquared = math.distancesq(transform.Position, otherTransform.Position);
					var isOtherEntity = (entity != otherEntity);
					var isWithinRadius = (distanceSquared < viewRangeSquared);
					var isOtherEntityWithinRadius = (isOtherEntity && isWithinRadius);

					if (isOtherEntityWithinRadius)
					{
						var neighbor = new Neighbor
						{
							Position = otherTransform.Position,
							Velocity = otherMovement.Velocity
						};

						neighbors.Add(neighbor);

						var isSameTeam = otherMovement.Team == movement.Team;

						if (isSameTeam)
						{
							teamNeighbors.Add(neighbor);
						}
					}
				}
			}
		}

		/*
		public static float3 GetTotalAcceleration(float3 position, float3 velocity, float worldSize,
			NativeList<Neighbor> neighbors, Settings settings)
		{
			var boundRespectingAcceleration = GetBoundRespectingAcceleration(position, worldSize, settings.ViewRange);

			var velocityMatchingAcceleration = GetVelocityMatchingAcceleration(velocity, neighbors, settings.MatchRate);

			var spatialCoherenceAcceleration =
				GetSpatialCoherenceAcceleration(position, neighbors, settings.CoherenceRate);

			var collisionAvoidanceAcceleration = GetCollisionAvoidanceAcceleration(position, neighbors,
				settings.AvoidanceRange, settings.AvoidanceRate);

			var thrustAcceleration = GetThrustAcceleration(velocity, settings.Thrust);

			var dragAcceleration = GetDragAcceleration(velocity, settings.Drag);

			var totalAcceleration = boundRespectingAcceleration + velocityMatchingAcceleration +
			                        spatialCoherenceAcceleration + collisionAvoidanceAcceleration +
			                        thrustAcceleration + dragAcceleration;

			return totalAcceleration;
		}
		*/
		public static float3 GetTotalAcceleration(float3 position, float3 velocity, float worldSize,
			NativeList<Neighbor> neighbors, NativeList<Neighbor> teamNeighbors, Settings settings)
		{
			var boundRespectingAcceleration = GetBoundRespectingAcceleration(position, worldSize, settings.ViewRange);

			var velocityMatchingAcceleration =
				GetVelocityMatchingAcceleration(velocity, teamNeighbors, settings.MatchRate);

			var spatialCoherenceAcceleration =
				GetSpatialCoherenceAcceleration(position, teamNeighbors, settings.CoherenceRate);

			var collisionAvoidanceAcceleration = GetCollisionAvoidanceAcceleration(position, neighbors,
				settings.AvoidanceRange, settings.AvoidanceRate);

			var thrustAcceleration = GetThrustAcceleration(velocity, settings.Thrust);

			var dragAcceleration = GetDragAcceleration(velocity, settings.Drag);

			var totalAcceleration = boundRespectingAcceleration + velocityMatchingAcceleration +
			                        spatialCoherenceAcceleration + collisionAvoidanceAcceleration +
			                        thrustAcceleration + dragAcceleration;

			return totalAcceleration;
		}

		private static float3 GetBoundRespectingAcceleration(in float3 position, in float worldSize, in float avoidRange)
		{
			var halfWorldSize = worldSize * 0.5f;

			var boundMarginExcess = new float3
			{
				x = math.max(math.abs(position.x) + avoidRange - halfWorldSize, 0.0f) * math.sign(position.x),
				y = math.max(math.abs(position.y) + avoidRange - halfWorldSize, 0.0f) * math.sign(position.y),
				z = math.max(math.abs(position.z) + avoidRange - halfWorldSize, 0.0f) * math.sign(position.z)
			};

			// TODO: Remove magic number 5.0f,
			var acceleration = -boundMarginExcess * 5.0f;

			return acceleration;
		}

		private static float3 GetVelocityMatchingAcceleration(float3 velocity, in NativeList<Neighbor> neighbors,
			float matchRate)
		{
			if (neighbors.Length == 0)
			{
				return float3.zero;
			}

			var neighborCount = neighbors.Length;
			var neighborMeanVelocity = float3.zero;

			foreach (var neighbor in neighbors)
			{
				neighborMeanVelocity += neighbor.Velocity;
			}

			neighborMeanVelocity /= neighborCount;

			var acceleration = (neighborMeanVelocity - velocity) * matchRate;

			return acceleration;
		}

		private static float3 GetSpatialCoherenceAcceleration(float3 position, in NativeList<Neighbor> neighbors,
			float coherenceRate)
		{
			if (neighbors.Length == 0)
			{
				return float3.zero;
			}

			var neighborCount = neighbors.Length;
			var neighborMeanPosition = neighbors[0].Position;

			for (var neighborIndex = 1; neighborIndex < neighborCount; neighborIndex++)
			{
				neighborMeanPosition += neighbors[neighborIndex].Position;
			}

			neighborMeanPosition /= neighborCount;

			var acceleration = (neighborMeanPosition - position) * coherenceRate;

			return acceleration;
		}

		private static float3 GetCollisionAvoidanceAcceleration(float3 position, in NativeList<Neighbor> neighbors,
			float avoidanceRange, float avoidanceRate)
		{
			if (neighbors.Length == 0)
			{
				return float3.zero;
			}

			var minimumDistanceSquared = math.square(avoidanceRange);
			var totalAvoidanceVector = float3.zero;

			foreach (var neighbor in neighbors)
			{
				var deltaPosition = position - neighbor.Position;
				var distanceSquared = math.lengthsq(deltaPosition);
				var isWithinAvoidanceRange = ((distanceSquared > 0) && (distanceSquared < minimumDistanceSquared));

				if (isWithinAvoidanceRange)
				{
					// var avoidanceDirection = deltaPosition / math.sqrt(distanceSquared);
					var avoidanceDirection = math.normalize(deltaPosition);
					totalAvoidanceVector += avoidanceDirection;
				}
			}

			var acceleration = totalAvoidanceVector * avoidanceRate;

			return acceleration;
		}

		private static float3 GetThrustAcceleration(float3 velocity, float acc)
		{
			var velocityDirection = math.normalize(velocity);
			var acceleration = velocityDirection * acc;

			return acceleration;
		}

		private static float3 GetDragAcceleration(float3 velocity, float drag)
		{
			// var acceleration = -velocity * drag;

			var velocityDirection = math.normalize(velocity);
			var speedSquared = math.lengthsq(velocity);
			var acceleration = -velocityDirection * drag * speedSquared;

			return acceleration;
		}
	}

	[BurstCompile]
	public struct Neighbor
	{
		public float3 Position;
		public float3 Velocity;
	}
}
