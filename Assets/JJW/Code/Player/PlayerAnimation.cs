using UnityEngine;

namespace JJW.Code.Player
{
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
                facingDirection = Mathf.Abs(direction.x) >= Mathf.Abs(direction.y)
                    ? new Vector2(Mathf.Sign(direction.x), 0f)
                    : new Vector2(0f, Mathf.Sign(direction.y));
            }
            
            spriteRenderer.flipX = facingDirection.x < 0f;
            animator.SetFloat(MoveX, facingDirection.x);
            animator.SetFloat(MoveY, facingDirection.y);
            animator.SetBool(IsMoving, isMoving);
        }
    }
}
