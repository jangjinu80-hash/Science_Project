using UnityEngine;

namespace ScienceProject.PlayerSystem
{
    /// <summary>SRP: 전달받은 방향을 Rigidbody2D의 물리 이동으로 바꾼다.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMove : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed = 4f;
        private Rigidbody2D body;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        // Player의 FixedUpdate에서 호출한다. 이 클래스는 키보드나 Animator를 모른다.
        public void Move(Vector2 direction)
        {
            if (!isActiveAndEnabled) return;
            // linearVelocity의 단위는 초당 거리이므로 deltaTime을 곱하지 않는다.
            body.linearVelocity = Vector2.ClampMagnitude(direction, 1f) * moveSpeed;
        }

        public void Stop()
        {
            if (body != null)
                body.linearVelocity = Vector2.zero;
        }

        private void OnDisable()
        {
            Stop();
        }
    }
}
