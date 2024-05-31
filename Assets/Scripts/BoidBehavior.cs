using Unity.Collections;
using Unity.Mathematics;

namespace Boids
{
	public static class BoidBehavior
	{
		public static float3 GetBoundRespectingAcceleration(float3 position, float worldSize, float avoidRange)
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

		public static float3 GetVelocityMatvingAcceleration(float3 velocity, in NativeList<Neighbor> neighbors, float matchRate)
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

			// var neighborMeanVelocity = neighborVelocities.Aggregate(float3.zero, (current, neighbor) => current + neighbor);

			neighborMeanVelocity /= neighborCount;

			var acceleration = (neighborMeanVelocity - velocity) * matchRate;

			return acceleration;
		}

		public static float3 GetCoherenceAcceleration(float3 position, in NativeList<Neighbor> neighbors, float coherenceRate)
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

		public static float3 GetCollisionAvoidanceAcceleration(float3 position, in NativeList<Neighbor> neighbors, float avoidanceRange, float avoidanceRate)
		{
			if (neighbors.Length == 0)
			{
				return float3.zero;
			}

			var minDist = avoidanceRange;
			// var myPosition = position;
			var minDistSqr = minDist * minDist;
			var step = float3.zero;

			// for (int neighborIndex = 0; neighborIndex < neighborCount; neighborIndex++)
			foreach (var neighbor in neighbors)
			{
				// var delta = position - neighbors[neighborIndex].Position;
				var delta = position - neighbor.Position;
				var deltaSqr = math.lengthsq(delta);

				if ((deltaSqr > 0) && (deltaSqr < minDistSqr))
				{
					step += delta / math.sqrt(deltaSqr);
				}
			}

			// movement.Velocity += step * avoidanceRate * dt;
			var acceleration = step * avoidanceRate;

			return acceleration;
		}

		/*
		public static float3 AccelerationAndDrag(float3 velocity, float acc, float drag)
		{
			var acceleration = math.normalize(velocity) * acc;
			// TODO: Remove magic number 30.0f.
			acceleration *= 1.0f - 30.0f * drag;

			return acceleration;
		}
		*/

		public static float3 GetThrustAcceleration(float3 velocity, float acc)
		{
			var velocityDirection = math.normalize(velocity);
			var acceleration = velocityDirection * acc;

			return acceleration;
		}

		public static float3 GetDragAcceleration(float3 velocity, float drag)
		{
			// var acceleration = -velocity * drag;

			var velocityDirection = math.normalize(velocity);
			var speedSquared = math.lengthsq(velocity);
			var acceleration = -velocityDirection * drag * speedSquared;

			return acceleration;
		}
	}
}
