using Boids.TargetBehaviour;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;

namespace Boids.BoidBehaviour
{
    public class MoveSystem : JobComponentSystem
    {
        private const int two25 = 25 * 25;

        private MoveJob job;

        [BurstCompile]
        private struct MoveJob : IJobForEachWithEntity<VelocityComponent, Translation>
        {
            public float DeltaTime;
            public Translation TargetTranslation;

            public void Execute(
                Entity entity,
                int index,
                ref VelocityComponent velocityComponent,
                ref Translation translation
            )
            {
                var offset = translation.Value - TargetTranslation.Value;
                if (Vector3.SqrMagnitude(offset) > two25)
                {
                    velocityComponent.velocity -= offset / 25;
                }
                translation.Value += velocityComponent.velocity * DeltaTime;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            Translation targetTranslation = default;
            Entities.WithAll<TargetComponent>().ForEach(
                (
                    Translation translation
                ) =>
                {
                    targetTranslation = translation;
                }
            ).Run();

            job = new MoveJob()
            {
                DeltaTime = Time.DeltaTime,
                TargetTranslation = targetTranslation
            };

            return job.Schedule(this, inputDeps);
        }
    }
}