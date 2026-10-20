using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JJW.Code.Player
{
    public sealed class SceneTransition : MonoBehaviour
    {
        public static SceneTransition Instance { get; private set; }
        public Image shade;
        public bool IsBusy { get; private set; }
        private float holdShade;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(Reveal());
        }

        private void Update()
        {
            if (IsBusy) return;
            SetAlpha(Mathf.MoveTowards(shade.color.a, holdShade, Time.unscaledDeltaTime * 3));
        }

        public void SetHold(float progress) => holdShade = Mathf.Clamp01(progress) * 0.55f;

        public void Load(string scene)
        {
            if (IsBusy) return;
            StartCoroutine(ChangeScene(scene));
        }

        private IEnumerator Fade(float target, float seconds)
        {
            float start = shade.color.a;
            for (float t = 0; t < seconds; t += Time.unscaledDeltaTime)
            {
                SetAlpha(Mathf.Lerp(start, target, t / seconds));
                yield return null;
            }
            SetAlpha(target);
        }

        private IEnumerator Reveal()
        {
            IsBusy = true;
            shade.raycastTarget = true;
            Time.timeScale = 0;
            SetAlpha(1);
            yield return null;
            yield return Fade(0, 0.45f);
            shade.raycastTarget = false;
            IsBusy = false;
            Time.timeScale = 1;
        }

        private IEnumerator ChangeScene(string scene)
        {
            IsBusy = true;
            shade.raycastTarget = true;
            Time.timeScale = 0;
            yield return Fade(1, 0.3f);
            AsyncOperation load = SceneManager.LoadSceneAsync(scene);
            while (!load.isDone) yield return null;
            holdShade = 0;
            Time.timeScale = 0;
            yield return null;
            yield return Fade(0, 0.45f);
            shade.raycastTarget = false;
            IsBusy = false;
            Time.timeScale = 1;
        }

        private void SetAlpha(float alpha) => shade.color = new Color(0.015f, 0.025f, 0.05f, alpha);
        private void OnDestroy() { if (Instance == this) Instance = null; }
    }
}
