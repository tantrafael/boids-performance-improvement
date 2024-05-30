using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Boids
{
	public class MovementAuthoring : MonoBehaviour
	{
		private class Baker : Baker<MovementAuthoring>
		{
			public override void Bake(MovementAuthoring authoring)
			{
				var entity = GetEntity(TransformUsageFlags.Dynamic);
				AddComponent<Movement>(entity);
			}
		}
	}

	public struct Movement : IComponentData
	{
		public float3 Velocity;
	}
}
