using UnityEngine;

namespace JJW.Code.Player
{
    public sealed class PlateGate : MonoBehaviour
    {
        public PressurePlate[] plates;
        public Collider2D barrier;
        public SpriteRenderer display;
        public Sprite closedSprite;
        public Sprite openSprite;
        private bool open;

        private void Update()
        {
            bool requested = plates.Length > 0;
            foreach (PressurePlate plate in plates) requested &= plate != null && plate.IsPressed;
            if (requested == open) return;
            if (!requested)
            {
                foreach (Collider2D hit in Physics2D.OverlapBoxAll(transform.position, new Vector2(0.85f, 0.85f), 0))
                    if (hit.attachedRigidbody != null) return;
            }
            open = requested;
            barrier.enabled = !open;
            display.sprite = open ? openSprite : closedSprite;
            display.color = open ? new Color(0.5f, 1, 0.8f, 0.6f) : Color.white;
        }
    }
}
