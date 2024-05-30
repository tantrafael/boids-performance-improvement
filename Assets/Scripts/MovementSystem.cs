using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace Boids
{
    public partial struct MovementSystem : ISystem
    {
        // [BurstCompile]
        // public void OnCreate(ref SystemState state)
        // {
        //     // state.RequireForUpdate<ExecuteClosestTarget>();
        //     state.RequireForUpdate<Movement>();
        // }

        // [BurstCompile]
        // public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new MovementJob
            {
                // TODO: Remove magic number.
                Delta = SystemAPI.Time.DeltaTime * 10
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct MovementJob : IJobEntity
    {
        public float Delta;

        private void Execute(ref LocalTransform transform, in Movement movement)
        // private void Execute(ref LocalTransform transform)
        {
            const float size = 100.0f;

            // transform.Position.xz += movement.Value * Delta;
            transform.Position.xyz += movement.Value * Delta;
            if (transform.Position.x < -size) transform.Position.x += size * 2;
            if (transform.Position.x > +size) transform.Position.x -= size * 2;
            if (transform.Position.y < -size) transform.Position.y += size * 2;
            if (transform.Position.y > +size) transform.Position.y -= size * 2;
            if (transform.Position.z < -size) transform.Position.z += size * 2;
            if (transform.Position.z > +size) transform.Position.z -= size * 2;

            // transform.Position.y -= 0.1f;
        }
    }
}