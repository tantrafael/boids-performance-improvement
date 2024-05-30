using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
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

		/*
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			new MovementJob
			{
				// TODO: Remove magic number 10.
				Delta = SystemAPI.Time.DeltaTime * 10
			}.ScheduleParallel();
		}
		*/

		/*
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var targetQuery = SystemAPI.QueryBuilder().WithAll<LocalTransform>().Build();
			var targetEntities = targetQuery.ToEntityArray(state.WorldUpdateAllocator);
			var targetTransforms = targetQuery.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);
			var noPartitioning = new NoPartitioning { TargetEntities = targetEntities, TargetTransforms = targetTransforms };
			state.Dependency = noPartitioning.ScheduleParallel(state.Dependency);
		}
		*/

		/*
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			// Simple solution, but it requires creating temporary copies of all box translations and entity IDs
			var boxTransforms = boxQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
			var boxEntities = boxQuery.ToEntityArray(Allocator.Temp);

			foreach (var (transform, defaultColor, color, entity) in
			         SystemAPI.Query<RefRO<LocalTransform>, RefRO<DefaultColor>,
					         RefRW<URPMaterialPropertyBaseColor>>()
				         .WithEntityAccess())
			{
				// reset color of the box to its default
				color.ValueRW.Value = defaultColor.ValueRO.Value;

				// change the color if this box intersects another
				for (int i = 0; i < boxTransforms.Length; i++)
				{
					var otherEnt = boxEntities[i];
					var otherTrans = boxTransforms[i];

					// A box should not intersect with itself, so we check if the other entity's id matches the current entity's id.
					if (entity != otherEnt && math.distancesq(transform.ValueRO.Position, otherTrans.Position) < 1)
					{
						color.ValueRW.Value.y = 0.5f; // set green channel
						break;
					}
				}
			}
		}
		*/

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var boxQuery = SystemAPI.QueryBuilder().WithAll<LocalTransform>().Build();

			// More complex solution, but it avoids creating temporary copies of the box components
			new CollisionJob
			{
				// LocalTransformTypeHandle = SystemAPI.GetComponentTypeHandle<LocalTransform>(true),
				LocalTransformTypeHandle = SystemAPI.GetComponentTypeHandle<LocalTransform>(),
				// DefaultColorTypeHandle = SystemAPI.GetComponentTypeHandle<DefaultColor>(true),
				// BaseColorTypeHandle = SystemAPI.GetComponentTypeHandle<URPMaterialPropertyBaseColor>(),
				MovementTypeHandle = SystemAPI.GetComponentTypeHandle<Movement>(),
				EntityTypeHandle = SystemAPI.GetEntityTypeHandle(),
				OtherChunks = boxQuery.ToArchetypeChunkArray(state.WorldUpdateAllocator)
			}.ScheduleParallel(boxQuery, state.Dependency).Complete();
		}
	}

	/*
	[BurstCompile]
	public partial struct MovementJob : IJobEntity
	{
		public float Delta;

		private void Execute(ref LocalTransform transform, in Movement movement)
		{
			const float size = 100.0f;

			transform.Position.xyz += movement.Velocity * Delta;
			if (transform.Position.x < -size) transform.Position.x += size * 2;
			if (transform.Position.x > +size) transform.Position.x -= size * 2;
			if (transform.Position.y < -size) transform.Position.y += size * 2;
			if (transform.Position.y > +size) transform.Position.y -= size * 2;
			if (transform.Position.z < -size) transform.Position.z += size * 2;
			if (transform.Position.z > +size) transform.Position.z -= size * 2;
		}
	}
	*/

	/*
	[BurstCompile]
	public partial struct NoPartitioning : IJobEntity
	{
		[ReadOnly] public NativeArray<Entity> TargetEntities;
		[ReadOnly] public NativeArray<LocalTransform> TargetTransforms;

		// public void Execute(ref Target target, in LocalTransform translation)
		private void Execute(ref LocalTransform transform, in Movement movement)
		{
			// var closestDistSq = float.MaxValue;
			// var closestEntity = Entity.Null;
			//
			// for (int i = 0; i < TargetTransforms.Length; i += 1)
			// {
			// 	var distSq = math.distancesq(TargetTransforms[i].Position, translation.Position);
			// 	if (distSq < closestDistSq)
			// 	{
			// 		closestDistSq = distSq;
			// 		closestEntity = TargetEntities[i];
			// 	}
			// }
			//
			// target.Value = closestEntity;

			// allWithinRadius = new List<BoidBehaviour.IBoid>();
			// sameTeamWithinRadius = new List<BoidBehaviour.IBoid>();
			//
			// var sourceBoid = m_boids[sourceBoidIndex];
			//
			// for (int i = 0; i < m_boids.Length; i++)
			// {
			// 	if (i != sourceBoidIndex)
			// 	{
			// 		Vector3 dif = m_boids[i].Position - sourceBoid.Position;
			// 		if (dif.magnitude < radius)
			// 		{
			// 			allWithinRadius.Add(m_boids[i]);
			// 			if (sourceBoid.Team == m_boids[i].Team)
			// 			{
			// 				sameTeamWithinRadius.Add(m_boids[i]);
			// 			}
			// 		}
			// 	}
			// }

			// Find neighbors.
			// var allWithinRadius = new NativeArray<LocalTransform>();
			//
			// for (int i = 0; i < TargetEntities.Length; i++)
			// {
			// 	var entity = TargetEntities[i];
			//
			// 	for (int j = 0; j < TargetTransforms.Length; j++)
			// 	{
			// 		var otherEnt = TargetEntities[i];
			// 		var otherTrans = TargetTransforms[i];
			//
			// 		// A box should not intersect with itself, so we check if the other entity's id matches the current entity's id.
			// 		if (entity != otherEnt && math.distancesq(transform.ValueRO.Position, otherTrans.Position) < 1)
			// 		{
			// 			color.ValueRW.Value.y = 0.5f; // set green channel
			// 			break;
			// 		}
			// 	}
			// }
		}
	}
	*/

	[BurstCompile]
	public struct CollisionJob : IJobChunk
	{
		// [ReadOnly] public ComponentTypeHandle<LocalTransform> LocalTransformTypeHandle;
		public ComponentTypeHandle<LocalTransform> LocalTransformTypeHandle;
		public ComponentTypeHandle<Movement> MovementTypeHandle;
		[ReadOnly] public EntityTypeHandle EntityTypeHandle;

		[ReadOnly] public NativeArray<ArchetypeChunk> OtherChunks;

		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			const float viewRange = 3.0f;

			var transforms = chunk.GetNativeArray(ref LocalTransformTypeHandle);
			var movements = chunk.GetNativeArray(ref MovementTypeHandle);
			var entities = chunk.GetNativeArray(EntityTypeHandle);
			// var allWithinRadius = new NativeArray<LocalTransform>();

			for (int i = 0; i < transforms.Length; i++)
			{
				var transform = transforms[i];
				// var baseColor = baseColors[i];
				var movement = movements[i];
				var entity = entities[i];

				/*
				for (int j = 0; j < OtherChunks.Length; j++)
				{
					var otherChunk = OtherChunks[j];
					var otherTranslations = otherChunk.GetNativeArray(ref LocalTransformTypeHandle);
					var otherEntities = otherChunk.GetNativeArray(EntityTypeHandle);

					for (int k = 0; k < otherChunk.Count; k++)
					{
						var otherTranslation = otherTranslations[k];
						var otherEntity = otherEntities[k];
						var distance = math.distancesq(transform.Position, otherTranslation.Position);
						var isOtherEntity = (entity != otherEntity);
						var isWithinRadius = (distance < viewRange);
						var isNeighbor = (isOtherEntity && isWithinRadius);

						if (isNeighbor)
						{
							allWithinRadius.Append(otherTranslation);
						}
					}
				}
				*/

				/*
				// AvoidInsideBoundsOfCube
				const float halfWorldSize = 20.0f;
				const float avoidRange = 3.0f;
				const float dt = 0.01f;

				// TODO: Remove magic number 5.0f,
				var velocity = new float3
				{
					x = math.max(math.abs(transform.Position.x) + avoidRange - halfWorldSize, 0.0f) * math.sign(transform.Position.x) * 5.0f * dt,
					y = math.max(math.abs(transform.Position.y) + avoidRange - halfWorldSize, 0.0f) * math.sign(transform.Position.y) * 5.0f * dt,
					z = math.max(math.abs(transform.Position.z) + avoidRange - halfWorldSize, 0.0f) * math.sign(transform.Position.z) * 5.0f * dt
				};

				movement.Velocity -= velocity;
				movements[i] = movement;
				*/

				const float dt = 0.01f;
				transform.Position.xyz += movement.Velocity * dt;
				transforms[i] = transform;
			}
		}
	}
}
