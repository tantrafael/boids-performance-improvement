using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Boids
{
	[BurstCompile]
	public static class BoidBehavior
	{
		public static BoidMovementState GetUpdatedBoidMovementState(float3 position, float3 velocity, int teamIndex,
			NativeList<Neighbor> neighbors, NativeList<Neighbor> teamNeighbors, Settings settings, float deltaTime)
		{
			var totalAcceleration =
				GetTotalAcceleration(position, velocity, teamIndex, neighbors, teamNeighbors, settings);

			var deltaVelocity = totalAcceleration * deltaTime;
			var updatedVelocity = velocity + deltaVelocity;
			var deltaPosition = velocity * deltaTime;
			var updatedPosition = position + deltaPosition;

			var updatedRotation = GetBoidRotation(updatedVelocity);

			var boidMovementState = new BoidMovementState
			{
				Position = updatedPosition,
				Velocity = updatedVelocity,
				Rotation = updatedRotation
			};

			return boidMovementState;
		}

		private static float3 GetTotalAcceleration(float3 position, float3 velocity, int teamIndex,
			NativeList<Neighbor> neighbors, NativeList<Neighbor> teamNeighbors, Settings settings)
		{
			float thrust;
			float drag;

			switch (teamIndex)
			{
				case 0:
					thrust = settings.ThrustTeamRed;
					drag = settings.DragTeamRed;
					break;
				case 1:
					thrust = settings.ThrustTeamGreen;
					drag = settings.DragTeamGreen;
					break;
				default:
					thrust = settings.ThrustTeamBlue;
					drag = settings.DragTeamBlue;
					break;
			}
			/*
			var thrustTable = new NativeList<float>(Allocator.Temp);
			thrustTable.Add(settings.ThrustTeamRed);
			thrustTable.Add(settings.ThrustTeamGreen);
			thrustTable.Add(settings.ThrustTeamBlue);
			var thrust = thrustTable[teamIndex];

			var dragTable = new NativeList<float>(Allocator.Temp);
			dragTable.Add(settings.DragTeamRed);
			dragTable.Add(settings.DragTeamGreen);
			dragTable.Add(settings.DragTeamBlue);
			var drag = dragTable[teamIndex];
			*/

			var boundRespectingAcceleration =
				GetBoundRespectingAcceleration(position, settings.WorldSize, settings.ViewRange);

			var velocityMatchingAcceleration =
				GetVelocityMatchingAcceleration(velocity, teamNeighbors, settings.MatchRate);

			var spatialCoherenceAcceleration =
				GetSpatialCoherenceAcceleration(position, teamNeighbors, settings.CoherenceRate);

			var collisionAvoidanceAcceleration = GetCollisionAvoidanceAcceleration(position, neighbors,
				settings.AvoidanceRange, settings.AvoidanceRate);

			var thrustAcceleration = GetThrustAcceleration(velocity, thrust);

			var dragAcceleration = GetDragAcceleration(velocity, drag);

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

		private static quaternion GetBoidRotation(float3 velocity)
		{
			var velocityDirection = math.normalize(velocity);
			var worldUpDirection = new float3(0.0f, 1.0f, 0.0f);
			var rotation = quaternion.LookRotation(velocityDirection, worldUpDirection);
			// var rotation = quaternion.LookRotationSafe(velocity, worldUpDirection);

			return rotation;
		}
	}

	[BurstCompile]
	public struct BoidMovementState
	{
		public float3 Position;
		public float3 Velocity;
		public quaternion Rotation;
	}
}
