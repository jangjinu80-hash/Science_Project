using System.Collections.Generic;
using UnityEngine;

namespace JJW.Code.Player
{
    public sealed class PressurePlate : MonoBehaviour
    {
        public int requiredWeight = 1;
        public SpriteRenderer indicator;
        public Color offColor = new Color(0.45f, 0.36f, 0.19f);
        public Color onColor = new Color(0.45f, 1, 0.65f);
        private readonly HashSet<Collider2D> contacts = new HashSet<Collider2D>();
        private readonly HashSet<PlateWeight> weights = new HashSet<PlateWeight>();
        public bool IsPressed { get; private set; }

        private void OnTriggerEnter2D(Collider2D other) => contacts.Add(other);
        private void OnTriggerStay2D(Collider2D other) => contacts.Add(other);
        private void OnTriggerExit2D(Collider2D other) => contacts.Remove(other);

        private void FixedUpdate()
        {
            contacts.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy);
            weights.Clear();
            int total = 0;
            foreach (Collider2D contact in contacts)
            {
                PlateWeight weight = contact.GetComponentInParent<PlateWeight>();
                if (weight != null && weights.Add(weight)) total += weight.weight;
            }
            IsPressed = total >= requiredWeight;
            if (indicator != null) indicator.color = IsPressed ? onColor : offColor;
        }
    }
}
