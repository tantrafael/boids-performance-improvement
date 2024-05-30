using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids
{
    public partial struct InitializationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Settings>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;

            var settings = SystemAPI.GetSingleton<Settings>();
            // TODO: Remove magic number.
            var random = Random.CreateFromIndex(1234);

            Spawn(ref state, settings.UnitPrefab, settings.UnitCount, ref random);
        }

        private void Spawn(ref SystemState state, Entity prefab, int count, ref Random random)
        {
            var units = state.EntityManager.Instantiate(prefab, count, Allocator.Temp);

            foreach (var unit in units)
            {
                // TODO: Remove magic numbers.
                var position = new float3 { xyz = (random.NextFloat3() - 0.5f) * 100.0f };
                var localTransform = new LocalTransform { Position = position, Scale = 1.0f };
                state.EntityManager.SetComponentData(unit, localTransform);

                var movement = new Movement { Value = random.NextFloat3Direction() };
                state.EntityManager.SetComponentData(unit, movement);
            }
        }
    }
}
