using UnityEngine;

namespace JJW.Code.Player
{
    [RequireComponent(typeof(Camera))]
    public sealed class LabCamera : MonoBehaviour
    {
        public float roomWidth = 21;
        public float roomHeight = 13;
        private Camera view;

        private void Awake() => view = GetComponent<Camera>();
        private void LateUpdate()
        {
            view.orthographicSize = Mathf.Max(roomHeight * 0.5f + 0.6f,
                roomWidth / (2 * Mathf.Max(0.1f, view.aspect)) + 0.6f);
        }
    }
}
