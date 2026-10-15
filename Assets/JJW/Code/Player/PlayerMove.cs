using UnityEngine;

namespace JJW.Code.Player
{
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

        public void Move(Vector2 direction)
        {
            if (!isActiveAndEnabled) return;
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
