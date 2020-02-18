using Unity.Entities;
using UnityEngine;

namespace Boids
{
    [GenerateAuthoringComponent]
    public struct MouseInputComponent : IComponentData
    {
        [HideInInspector]
        public Vector2 mousePosition;
    }
}