using UnityEngine;
using UnityEngine.InputSystem;

namespace ScienceProject.PlayerSystem
{
    /// <summary>SRP: 입력을 이동 방향으로 읽는 역할만 담당한다.</summary>
    [DisallowMultipleComponent]
    public sealed class PlayerInput : MonoBehaviour
    {
        [SerializeField] private InputActionReference moveAction;
        private InputAction runtimeMoveAction;

        private void Awake()
        {
            if (moveAction == null || moveAction.action == null)
            {
                Debug.LogError("PlayerInput에 Player/Move 액션을 연결해 주세요.", this);
                enabled = false;
                return;
            }

            // 공유 에셋의 액션을 직접 끄지 않도록 이 플레이어 전용 복사본을 소유한다.
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

            // (1, 1)의 길이는 sqrt(2). 최대 길이를 1로 제한해 대각선 가속을 막는다.
            // normalized와 달리 아날로그 스틱의 작은 입력 크기는 그대로 유지된다.
            return Vector2.ClampMagnitude(runtimeMoveAction.ReadValue<Vector2>(), 1f);
        }
    }
}
