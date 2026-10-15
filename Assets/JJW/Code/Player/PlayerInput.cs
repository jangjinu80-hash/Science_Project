using UnityEngine;
using UnityEngine.InputSystem;

namespace JJW.Code.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerInput : MonoBehaviour
    {
        [SerializeField] private InputActionReference moveAction;
        private InputAction runtimeMoveAction;

        private void Awake()
        {
            runtimeMoveAction = moveAction.action.Clone();
        }

        private void OnEnable()
        {
            runtimeMoveAction?.Enable();
        }

        private void OnDisable()
        {
            runtimeMoveAction?.Disable();
        }

        private void OnDestroy()
        {
            runtimeMoveAction?.Dispose();
        }

        public Vector2 ReadDirection()
        {
            if (!isActiveAndEnabled || runtimeMoveAction == null)
                return Vector2.zero;

            return Vector2.ClampMagnitude(runtimeMoveAction.ReadValue<Vector2>(), 1f);
        }
    }
}
