using UnityEngine;

namespace Boids
{
    public class Spawner : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        private Transform boidPrefab;
        [SerializeField]
        private int swarmCount = 100;
        [SerializeField]
        private Transform target;
#pragma warning restore CS0649

        public static int SwarmCount { get; private set; }

        private void Awake()
        {
            SwarmCount = swarmCount;

            for (var i = 0; i < swarmCount; i++)
            {
                var position = target.position + Random.insideUnitSphere * 25;
                Instantiate(boidPrefab, position, Quaternion.identity);
            }
        }
    }
}