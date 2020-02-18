using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Boids.BoidBehaviour
{
    [GenerateAuthoringComponent]
    public struct VelocityComponent : IComponentData
    {
        [HideInInspector]
        public float timerCalcVelocity;
        [HideInInspector]
        public float3 velocity;
        [HideInInspector]
        public float3 cohesion;
        [HideInInspector]
        public float3 separation;
        [HideInInspector]
        public int separationCount;
        [HideInInspector]
        public float3 alignment;
        [HideInInspector]
        public float3 vector;
        [HideInInspector]
        public float sqrMagnitude;
    }
}