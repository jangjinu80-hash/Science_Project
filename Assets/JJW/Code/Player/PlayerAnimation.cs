using UnityEngine;

namespace ScienceProject.PlayerSystem
{
    /// <summary>SRP: 이동 의도를 Animator 파라미터와 스프라이트 방향으로 표현한다.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
    public sealed class PlayerAnimation : MonoBehaviour
    {
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveY = Animator.StringToHash("MoveY");
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");

        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private Vector2 facingDirection = Vector2.down;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetMovement(Vector2 direction)
        {
            bool isMoving = direction.sqrMagnitude > 0.0001f;
            if (isMoving)
            {
                // 소스가 4방향 그림이므로 대각선 이동도 한 방향의 그림을 선택한다.
                // 두 축이 같으면 좌우 우선. 정지하면 기존 facingDirection을 유지한다.
                facingDirection = Mathf.Abs(direction.x) >= Mathf.Abs(direction.y)
                    ? new Vector2(Mathf.Sign(direction.x), 0f)
                    : new Vector2(0f, Mathf.Sign(direction.y));
            }

            // 좌우는 같은 클립을 쓰고 그림만 반전한다. 충돌체/루트는 반전하지 않는다.
            spriteRenderer.flipX = facingDirection.x < 0f;
            animator.SetFloat(MoveX, facingDirection.x);
            animator.SetFloat(MoveY, facingDirection.y);
            animator.SetBool(IsMoving, isMoving);
        }
    }
}
