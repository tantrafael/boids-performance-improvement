using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
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
				AddComponent<URPMaterialPropertyBaseColor>(entity);
			}
		}
	}

	// TODO: Consider assigning team using component tags rather than storing it with each entity.
	// Components tags are free and provide efficient selection.
	public struct Movement : IComponentData
	{
		public float3 Velocity;
		public int Team;
	}
}
