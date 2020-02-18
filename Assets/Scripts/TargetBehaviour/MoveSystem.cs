using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids.TargetBehaviour
{
    public class MoveSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<MouseInputComponent>().ForEach(
                (
                    ref MouseInputComponent mouseInputComponent
                ) =>
                {
                    var mousePosition = mouseInputComponent.mousePosition;
                    var mousePosition3 = new float3(mousePosition.x, mousePosition.y, 0);
                    Entities.WithAll<TargetComponent>().ForEach(
                        (
                            ref Translation translation
                        ) =>
                        {
                            translation.Value = mousePosition3;
                        }
                    );
                }
            );
        }
    }
}