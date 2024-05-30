/*
using UnityEngine;

public static class BoidBehaviour
{
	public static void AvoidInsideBoundsOfCube(IBoid boid, Vector3 halfCubeSize, float avoidRange, float dt)
	{
		boid.Velocity -= new Vector3(
			Mathf.Max(Mathf.Abs(boid.Position.x) - halfCubeSize.x + avoidRange, 0) * Mathf.Sign(boid.Position.x) * 5f * dt,
			Mathf.Max(Mathf.Abs(boid.Position.y) - halfCubeSize.y + avoidRange, 0) * Mathf.Sign(boid.Position.y) * 5f * dt,
			Mathf.Max(Mathf.Abs(boid.Position.z) - halfCubeSize.z + avoidRange, 0) * Mathf.Sign(boid.Position.z) * 5f * dt);
	}
}
*/
