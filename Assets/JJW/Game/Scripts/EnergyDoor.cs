using UnityEngine;

namespace JJW.Code.Player
{
    public sealed class EnergyDoor : LabDevice
    {
        public float cost = 15;
        public float requiredOutput = 10;
        public bool exit;
        public float openSeconds;
        public PressurePlate[] plates;
        public Collider2D barrier;
        public SpriteRenderer display;
        public Sprite closedSprite;
        public Sprite openSprite;
        private float remaining;
        public bool IsOpen { get; private set; }
        public bool PlatesReady
        {
            get
            {
                if (plates == null) return true;
                foreach (PressurePlate plate in plates)
                    if (plate != null && !plate.IsPressed) return false;
                return true;
            }
        }

        public override string Describe(CapacitorInventory inventory)
        {
            if (IsOpen) return openSeconds > 0 ? $"{deviceName} · 남은 시간 {remaining:0.0}초" : $"{deviceName} · 열림";
            string condition = !PlatesReady ? "연결된 발판을 모두 누르세요" : inventory.Check(cost, requiredOutput);
            string type = requiredOutput > 20 ? "높은 출력 필요" : requiredOutput > 8 ? "일반 출력 필요" : "모든 축전기 사용 가능";
            return $"{deviceName} · 에너지 {cost:0} / {type}\n" +
                (condition.Length == 0 ? $"[E] 작동  ·  사용 후 {inventory.Current - cost:0.0}" : condition);
        }

        public override void Interact(StageController stage)
        {
            if (IsOpen || !PlatesReady) return;
            if (!stage.Inventory.Spend(cost, requiredOutput))
            {
                stage.Hud.Notify(stage.Inventory.Check(cost, requiredOutput));
                return;
            }
            SetOpen(true);
            remaining = openSeconds;
            if (exit) stage.Complete();
        }

        private void Update()
        {
            if (!IsOpen || openSeconds <= 0 || exit) return;
            remaining = Mathf.Max(0, remaining - Time.deltaTime);
            if (remaining <= 0 && !HasOccupant()) SetOpen(false);
        }

        public bool HasOccupant()
        {
            foreach (Collider2D hit in Physics2D.OverlapBoxAll(transform.position, new Vector2(0.85f, 0.85f), 0))
                if (hit.attachedRigidbody != null && hit.attachedRigidbody.bodyType == RigidbodyType2D.Dynamic)
                    return true;
            return false;
        }

        private void SetOpen(bool value)
        {
            IsOpen = value;
            if (barrier != null) barrier.enabled = !value;
            if (display != null)
            {
                display.sprite = value ? openSprite : closedSprite;
                display.color = value ? new Color(0.55f, 1, 0.8f, 0.6f) : Color.white;
            }
        }
    }
}
