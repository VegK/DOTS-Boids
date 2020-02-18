using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace Boids.BoidBehaviour
{
    public class VelocitySystem : JobComponentSystem
    {
        private const int maxSpeed = 15;
        private const float cohesionRadius = 7;
        private const int maxBoids = 10;
        private const float separationDistance = 5;
        private const float separationDistance2 = separationDistance * separationDistance;
        private const float cohesionCoefficient = 1;
        private const float alignmentCoefficient = 4;
        private const float separationCoefficient = 10;
        private const float tick = 2;

        private VelocityJob job;
        private VelocityComponent velocityComponent;
        private CollisionFilter collisionFilter;
        private BuildPhysicsWorld buildPhysicsWorld;

        private NativeArray<Entity> allEntities;
        private NativeArray<Entity> entities;
        private NativeHashMap<Entity, Translation> translationMapData;
        private NativeHashMap<Entity, VelocityComponent> velocityMapData;

        [BurstCompile]
        private struct VelocityJob : IJobForEachWithEntity<VelocityComponent, Translation>
        {
            public float DeltaTime;
            [ReadOnly]
            public CollisionWorld CollisionWorld;
            [ReadOnly]
            public CollisionFilter CollisionFilter;
            [ReadOnly]
            public NativeHashMap<Entity, Translation> TranslationMapData;
            [ReadOnly]
            public NativeHashMap<Entity, VelocityComponent> VelocityMapData;

            public void Execute(
                Entity entity,
                int index,
                ref VelocityComponent velocityComponent,
                ref Translation translation
            )
            {
                velocityComponent.timerCalcVelocity -= DeltaTime;
                if (velocityComponent.timerCalcVelocity > 0)
                {
                    return;
                }
                velocityComponent.timerCalcVelocity = tick;

                var input = new PointDistanceInput
                {
                    Position = translation.Value,
                    MaxDistance = cohesionRadius,
                    Filter = CollisionFilter
                };

                var closestHits = new NativeList<DistanceHit>(Allocator.Temp);
                if (!CollisionWorld.CalculateDistance(input, ref closestHits))
                {
                    closestHits.Dispose();
                    return;
                }

                if (closestHits.Length < 2)
                {
                    closestHits.Dispose();
                    return;
                }

                velocityComponent.cohesion = float3.zero;
                velocityComponent.separation = float3.zero;
                velocityComponent.separationCount = 0;
                velocityComponent.alignment = float3.zero;

                for (int i = 0; i < closestHits.Length && i < maxBoids; i++)
                {
                    var entityHit = CollisionWorld.Bodies[closestHits[i].RigidBodyIndex].Entity;

                    velocityComponent.vector = translation.Value - TranslationMapData[entityHit].Value;
                    velocityComponent.sqrMagnitude = Vector3.SqrMagnitude(velocityComponent.vector);

                    velocityComponent.cohesion += TranslationMapData[entityHit].Value;
                    velocityComponent.alignment += VelocityMapData[entityHit].velocity;
                    if (velocityComponent.sqrMagnitude > 0 && velocityComponent.sqrMagnitude < separationDistance2)
                    {
                        velocityComponent.separation += velocityComponent.vector / velocityComponent.sqrMagnitude;
                        velocityComponent.separationCount++;
                    }
                }

                velocityComponent.cohesion /= closestHits.Length > maxBoids ? maxBoids : closestHits.Length;
                velocityComponent.cohesion = Vector3.ClampMagnitude(velocityComponent.cohesion - translation.Value, maxSpeed);
                velocityComponent.cohesion *= cohesionCoefficient;
                if (velocityComponent.separationCount > 0)
                {
                    velocityComponent.separation /= velocityComponent.separationCount;
                    velocityComponent.separation = Vector3.ClampMagnitude(velocityComponent.separation, maxSpeed);
                    velocityComponent.separation *= separationCoefficient;
                }
                velocityComponent.alignment /= closestHits.Length > maxBoids ? maxBoids : closestHits.Length;
                velocityComponent.alignment = Vector3.ClampMagnitude(velocityComponent.alignment, maxSpeed);
                velocityComponent.alignment *= alignmentCoefficient;

                velocityComponent.velocity = Vector3.ClampMagnitude(velocityComponent.cohesion + velocityComponent.separation + velocityComponent.alignment, maxSpeed);

                closestHits.Dispose();
            }
        }

        protected override void OnStartRunning()
        {
            allEntities = EntityManager.GetAllEntities(Allocator.Persistent);
            entities = new NativeArray<Entity>(Spawner.SwarmCount, Allocator.Persistent);
            translationMapData = new NativeHashMap<Entity, Translation>(Spawner.SwarmCount, Allocator.Persistent);
            velocityMapData = new NativeHashMap<Entity, VelocityComponent>(Spawner.SwarmCount, Allocator.Persistent);

            collisionFilter = new CollisionFilter
            {
                BelongsTo = ~(1u << 8),
                CollidesWith = ~(1u << 8),
                GroupIndex = 0
            };
            buildPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>();

            var index = 0;
            for (int i = 0; i < allEntities.Length; i++)
            {
                if (EntityManager.HasComponent<VelocityComponent>(allEntities[i]))
                {
                    entities[index] = allEntities[i];

                    velocityComponent = EntityManager.GetComponentData<VelocityComponent>(allEntities[i]);
                    velocityComponent.timerCalcVelocity = UnityEngine.Random.value * tick;
                    velocityComponent.velocity = UnityEngine.Random.onUnitSphere * maxSpeed;
                    EntityManager.SetComponentData(allEntities[i], velocityComponent);

                    index++;
                }
            }
        }

        protected override void OnStopRunning()
        {
            allEntities.Dispose();
            entities.Dispose();
            translationMapData.Dispose();
            velocityMapData.Dispose();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            translationMapData.Clear();
            velocityMapData.Clear();
            for (int i = 0; i < entities.Length; i++)
            {
                translationMapData.Add(entities[i], EntityManager.GetComponentData<Translation>(entities[i]));
                velocityMapData.Add(entities[i], EntityManager.GetComponentData<VelocityComponent>(entities[i]));
            }

            job = new VelocityJob
            {
                DeltaTime = Time.DeltaTime,
                CollisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld,
                CollisionFilter = collisionFilter,
                TranslationMapData = translationMapData,
                VelocityMapData = velocityMapData
            };

            return job.Schedule(this, inputDeps);
        }
    }
}