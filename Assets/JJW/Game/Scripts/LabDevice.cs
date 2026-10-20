using UnityEngine;

namespace JJW.Code.Player
{
    public abstract class LabDevice : MonoBehaviour
    {
        public string deviceName;
        public Vector2 interactionOffset;
        public float reach = 1.65f;
        public Vector2 InteractionPosition => (Vector2)transform.position + interactionOffset;
        public abstract string Describe(CapacitorInventory inventory);
        public abstract void Interact(StageController stage);
    }
}
