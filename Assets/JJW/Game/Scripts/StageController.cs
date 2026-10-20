using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace JJW.Code.Player
{
    public sealed class StageController : MonoBehaviour
    {
        public StageDefinition definition;
        public Player player;
        public CapacitorInventory Inventory;
        public LabHud Hud;
        private LabDevice[] devices;
        private bool completed;
        private bool answered;
        public bool IsModal { get; private set; }
        public bool IsTitle => definition == null;
        private LabDevice nearest;

        private void Start()
        {
            if (SceneTransition.Instance == null) Time.timeScale = 1;
            Hud.Bind(this);
            if (IsTitle)
            {
                Hud.ShowTitle();
                IsModal = true;
                return;
            }
            Inventory.Initialize(definition);
            devices = FindObjectsByType<LabDevice>();
            Hud.SetStage(definition);
            Hud.Notify(definition.objective, 6);
        }

        private void Update()
        {
            if (SceneTransition.Instance == null || SceneTransition.Instance.IsBusy) return;
            Keyboard keys = Keyboard.current;
            if (keys == null || IsTitle) return;
            if (keys.escapeKey.wasPressedThisFrame && !completed && !Hud.IsCharging)
            {
                if (IsModal) CloseModal();
                else { SetModal(true); Hud.ShowPause(); }
            }
            if ((!IsModal || Hud.IsChargeScreen) && !Hud.IsCharging)
            {
                if (keys.digit1Key.wasPressedThisFrame) Select(0);
                if (keys.digit2Key.wasPressedThisFrame) Select(1);
                if (keys.digit3Key.wasPressedThisFrame) Select(2);
            }
            if (IsModal) return;
            if (keys.hKey.wasPressedThisFrame) Hud.Notify(definition.hint, 9);
            nearest = FindNearest();
            Hud.SetPrompt(nearest != null ? nearest.Describe(Inventory) : "충전기와 장치 가까이에서 E  ·  H 힌트");
            if (keys.eKey.wasPressedThisFrame && nearest != null) nearest.Interact(this);
        }

        private LabDevice FindNearest()
        {
            LabDevice result = null;
            float closest = float.PositiveInfinity;
            Vector2 origin = player.transform.position;
            foreach (LabDevice device in devices)
            {
                float distance = Vector2.Distance(origin, device.InteractionPosition);
                if (distance > device.reach || distance >= closest) continue;
                bool blocked = false;
                foreach (RaycastHit2D hit in Physics2D.LinecastAll(origin, device.InteractionPosition))
                {
                    if (hit.collider.isTrigger || hit.collider.GetComponentInParent<Player>() != null) continue;
                    if (hit.collider.GetComponentInParent<LabDevice>() == device) continue;
                    blocked = true;
                    break;
                }
                if (blocked) continue;
                result = device;
                closest = distance;
            }
            return result;
        }

        public void Select(int index)
        {
            Inventory.Select(index);
            if (Hud.IsChargeScreen) Hud.RefreshCharge();
        }

        public void OpenCharger()
        {
            SetModal(true);
            Hud.ShowCharger();
        }

        public void CloseModal()
        {
            if (Hud.IsCharging || completed || IsTitle) return;
            Hud.HideModal();
            SetModal(false);
        }

        private void SetModal(bool value)
        {
            IsModal = value;
            if (player != null) player.enabled = !value;
            Time.timeScale = value ? 0 : 1;
        }

        public void Complete()
        {
            if (completed) return;
            completed = true;
            SetModal(true);
            Hud.ShowQuiz();
        }

        public void Answer(bool choice)
        {
            if (!completed || answered) return;
            answered = true;
            bool correct = choice == definition.answer;
            PlayerPrefs.SetInt("JJW.Answer." + definition.number, correct ? 1 : 0);
            PlayerPrefs.SetInt("JJW.NextStage", Mathf.Min(15, definition.number + 1));
            PlayerPrefs.Save();
            Hud.ShowAnswer(correct);
        }

        public void Advance()
        {
            if (!answered) return;
            if (definition.number == 15)
            {
                PlayerPrefs.SetInt("JJW.Completed", 1);
                PlayerPrefs.Save();
                Hud.ShowEnding();
            }
            else SceneTransition.Instance.Load(StageName(definition.number + 1));
        }

        public void Restart() => SceneTransition.Instance.Load(SceneManager.GetActiveScene().name);
        public void ToTitle() => SceneTransition.Instance.Load("Title");
        public static string StageName(int number) => $"Stage{number:00}";

        public void NewGame()
        {
            for (int i = 1; i <= 15; i++) PlayerPrefs.DeleteKey("JJW.Answer." + i);
            PlayerPrefs.SetInt("JJW.NextStage", 1);
            PlayerPrefs.SetInt("JJW.Completed", 0);
            PlayerPrefs.Save();
            SceneTransition.Instance.Load(StageName(1));
        }

        public void ContinueGame() => SceneTransition.Instance.Load(StageName(Mathf.Clamp(PlayerPrefs.GetInt("JJW.NextStage", 1), 1, 15)));
        public void Quit() => Application.Quit();

        private void OnApplicationFocus(bool focus)
        {
            if (!focus && Hud != null && !IsTitle && !IsModal && SceneTransition.Instance != null && !SceneTransition.Instance.IsBusy)
            {
                SetModal(true);
                Hud.ShowPause();
            }
        }
    }
}
