using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Boids.TargetBehaviour
{
    public class MouseInputSystem : ComponentSystem
    {
        private PlayerInput playerInput;
        private Camera camera;
        private Vector3 rawMousePositionInWorld;

        protected override void OnStartRunning()
        {
            playerInput = PlayerInput.GetPlayerByIndex(0);
            camera = Camera.main;
            rawMousePositionInWorld = new Vector3(0, 0, -camera.transform.position.z);
        }

        protected override void OnUpdate()
        {
            var moveValue = playerInput.actions["Move"].ReadValue<Vector2>();
            rawMousePositionInWorld.x = moveValue.x;
            rawMousePositionInWorld.y = moveValue.y;
            var mousePosition = camera.ScreenToWorldPoint(rawMousePositionInWorld);

            Entities.WithAll<MouseInputComponent>().ForEach(
                (
                    ref MouseInputComponent mouseInputComponent
                ) =>
                {
                    mouseInputComponent.mousePosition = mousePosition;
                }
            );
        }
    }
}