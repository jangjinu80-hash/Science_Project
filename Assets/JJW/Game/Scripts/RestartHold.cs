using UnityEngine;
using UnityEngine.InputSystem;

namespace JJW.Code.Player
{
    public sealed class RestartHold : MonoBehaviour
    {
        [SerializeField] private float holdDuration = 3;
        public StageController stage;
        public float Progress { get; private set; }
        private bool committed;
        private bool requireRelease;

        private void Awake()
        {
            requireRelease = Keyboard.current != null && Keyboard.current.rKey.isPressed;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            bool held = keyboard != null && keyboard.rKey.isPressed;
            if (requireRelease)
            {
                if (!held) requireRelease = false;
                Tick(false, 0, false);
                return;
            }
            Tick(held, Time.unscaledDeltaTime,
                stage != null && !stage.IsModal && !SceneTransition.Instance.IsBusy && Application.isFocused);
        }

        public void Tick(bool held, float elapsed, bool allowed)
        {
            if (committed) return;
            if (!allowed || !held) Progress = 0;
            else Progress = Mathf.Min(1, Progress + Mathf.Max(0, elapsed) / holdDuration);
            if (SceneTransition.Instance != null) SceneTransition.Instance.SetHold(Progress);
            if (stage != null) stage.Hud.SetRestart(Progress);
            if (Progress < 1) return;
            committed = true;
            stage.Restart();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused && !committed)
            {
                requireRelease = true;
                Progress = 0;
                if (SceneTransition.Instance != null) SceneTransition.Instance.SetHold(0);
            }
        }
    }
}
