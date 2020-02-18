using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Boids.BoidBehaviour
{
    public class RotationSystem : JobComponentSystem
    {
        private const int turnSpeed = 10;

        private float timer;
        private RotationJob job;

        [BurstCompile]
        private struct RotationJob : IJobForEachWithEntity<LocalToWorld, Rotation, VelocityComponent>
        {
            public void Execute(
                Entity entity,
                int index,
                ref LocalToWorld localToWorld,
                ref Rotation rotation,
                ref VelocityComponent velocityComponent
            )
            {
                if (!velocityComponent.velocity.Equals(float3.zero) &&
                    !localToWorld.Forward.Equals(math.normalize(velocityComponent.velocity)))
                {
                    var rotateTowards = Vector3.RotateTowards(localToWorld.Forward, velocityComponent.velocity, turnSpeed, 1);
                    rotation.Value = Quaternion.LookRotation(rotateTowards);
                }
            }
        }

        protected override void OnStartRunning()
        {
            timer = UnityEngine.Random.value;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            timer -= Time.DeltaTime;
            if (timer > 0)
            {
                return default;
            }
            timer = 0.1f;

            job = new RotationJob();
            return job.Schedule(this, inputDeps);
        }
    }
}