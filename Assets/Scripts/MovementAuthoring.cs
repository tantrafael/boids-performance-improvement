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
			/*
			public override void Bake(MovementAuthoring authoring)
			{
				var entity = GetEntity(TransformUsageFlags.Dynamic);
				AddComponent<Movement>(entity);
				AddComponent<URPMaterialPropertyBaseColor>(entity);
			}
			*/
			public override void Bake(MovementAuthoring authoring)
			{
				var entity = GetEntity(TransformUsageFlags.Dynamic);
				AddComponent<Movement>(entity);
				AddComponent<URPMaterialPropertyBaseColor>(entity);
			}
		}
	}

	public struct Movement : IComponentData
	{
		public float3 Velocity;
	}
	/*
	public struct Movement : IComponentData
	{
		public float3 Velocity;
		public int Team;
	}
	*/

	public struct TeamRed : IComponentData {}

	public struct TeamGreen : IComponentData {}

	public struct TeamBlue : IComponentData {}
}
