using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace JJW.Code.Player
{
    public sealed class LabHud : MonoBehaviour
    {
        public TMP_Text stageTitle, stageObjective, supply, prompt, restartText;
        public TMP_Text[] capacitorTexts;
        public Image[] capacitorBars, capacitorCards;
        public Image restartBar;
        public GameObject gameplay, modal, chargeControls;
        public TMP_Text modalTitle, modalBody, modalDetail;
        public Slider amount;
        public Button left, middle, right, close, minus, plus;
        public Button[] selectors;
        private StageController stage;
        private string devicePrompt;
        private string notice;
        private float noticeUntil;
        public bool IsCharging { get; private set; }
        public bool IsChargeScreen { get; private set; }
        private static readonly Color[] Colors =
        {
            new Color(0.35f, 0.8f, 1), new Color(1, 0.65f, 0.3f), new Color(0.45f, 1, 0.7f)
        };

        public void Bind(StageController controller)
        {
            stage = controller;
            amount.onValueChanged.AddListener(_ => UpdateChargeText());
            minus.onClick.AddListener(() => amount.value -= 5);
            plus.onClick.AddListener(() => amount.value += 5);
            for (int i = 0; i < selectors.Length; i++)
            {
                int index = i;
                selectors[i].onClick.AddListener(() => stage.Select(index));
            }
            close.onClick.AddListener(() => stage.CloseModal());
            HideModal();
        }

        public void SetStage(StageDefinition data)
        {
            string level = data.number <= 4 ? "기초 구역" : data.number <= 10 ? "실험 구역" : "중앙 제어 구역";
            stageTitle.text = $"<color=#72D8FF>{data.number:00} / 15</color>   {data.title}";
            stageObjective.text = level + "\n" + data.objective;
        }

        private void Update()
        {
            if (stage == null || stage.IsTitle) return;
            CapacitorInventory inventory = stage.Inventory;
            supply.text = $"공급 에너지\n<color=#F9D58B>{inventory.Supply:0.0}</color>";
            for (int i = 0; i < 3; i++)
            {
                bool unlocked = i < inventory.Unlocked;
                float value = inventory.GetEnergy(i);
                string output = i == 1 ? "높음" : i == 2 ? "낮음" : "일반";
                string loss = i == 1 ? "빠름" : i == 2 ? "매우 느림" : "느림";
                capacitorTexts[i].text = unlocked
                    ? $"<color=#{ColorUtility.ToHtmlStringRGB(Colors[i])}>[{i + 1}] {CapacitorInventory.Names[i]}</color>" +
                      (i == inventory.Selected ? "  선택" : "") +
                      $"\n<size=30>{value:0.0} <size=20>/ {CapacitorInventory.Capacities[i]:0}</size></size>\n<size=17>출력 {output} · 손실 {loss}</size>"
                    : $"[{i + 1}] {CapacitorInventory.Names[i]}\n<size=21>{(i == 1 ? "5" : "7")}단계에서 해금</size>";
                capacitorBars[i].fillAmount = value / CapacitorInventory.Capacities[i];
                capacitorCards[i].color = i == inventory.Selected
                    ? new Color(0.13f, 0.25f, 0.35f) : new Color(0.055f, 0.10f, 0.16f);
            }
            prompt.text = Time.unscaledTime < noticeUntil ? notice : devicePrompt;
        }

        public void SetPrompt(string value) => devicePrompt = value;
        public void Notify(string value, float duration = 3)
        {
            notice = value;
            noticeUntil = Time.unscaledTime + duration;
        }

        public void SetRestart(float progress)
        {
            restartBar.fillAmount = progress;
            restartText.text = progress > 0 ? $"재시작 준비  {progress * 3:0.0} / 3초 · 놓으면 취소" : "R 3초 재시작";
        }

        private void Show(string title, string body)
        {
            modalDetail.fontSize = 25;
            modalDetail.alignment = TextAlignmentOptions.MidlineLeft;
            modalDetail.rectTransform.anchoredPosition = new Vector2(0, -78);
            modalDetail.rectTransform.sizeDelta = new Vector2(980, 155);
            modal.SetActive(true);
            modalTitle.text = title;
            modalBody.text = body;
            modalDetail.text = "";
            IsChargeScreen = false;
            chargeControls.SetActive(false);
            close.gameObject.SetActive(false);
            left.gameObject.SetActive(false);
            middle.gameObject.SetActive(false);
            right.gameObject.SetActive(false);
        }

        private void Button(Button button, string label, UnityAction action)
        {
            button.gameObject.SetActive(true);
            button.interactable = true;
            button.GetComponentInChildren<TMP_Text>().text = label;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        public void HideModal()
        {
            modal.SetActive(false);
            IsChargeScreen = false;
        }

        public void ShowCharger()
        {
            Show("충전량 배분", "");
            IsChargeScreen = true;
            chargeControls.SetActive(true);
            close.gameObject.SetActive(true);
            Button(left, "충전 시작", () => { if (!IsCharging && amount.value > 0) StartCoroutine(Charge()); });
            Button(right, "배분 완료", () => stage.CloseModal());
            RefreshCharge();
        }

        public void RefreshCharge()
        {
            if (IsCharging) return;
            CapacitorInventory inventory = stage.Inventory;
            for (int i = 0; i < 3; i++)
            {
                selectors[i].interactable = i < inventory.Unlocked;
                selectors[i].GetComponentInChildren<TMP_Text>().text =
                    (i == inventory.Selected ? "● " : "") + CapacitorInventory.Names[i];
            }
            amount.minValue = 0;
            amount.maxValue = Mathf.Floor(inventory.ChargeLimit + 0.0001f);
            amount.wholeNumbers = true;
            amount.SetValueWithoutNotify(Mathf.Min(20, amount.maxValue));
            UpdateChargeText();
        }

        private void UpdateChargeText()
        {
            if (stage == null || !IsChargeScreen || IsCharging) return;
            CapacitorInventory inventory = stage.Inventory;
            float selected = amount.value;
            modalBody.text = $"공급 장치에 남은 에너지  <color=#F9D58B>{inventory.Supply:0.0}</color>\n" +
                $"선택: {CapacitorInventory.Names[inventory.Selected]}   " +
                $"{inventory.Current:0.0} → <color=#82E5FF>{inventory.Current + selected:0.0}</color> / {CapacitorInventory.Capacities[inventory.Selected]:0}";
            modalDetail.text = $"충전할 양  <b>{selected:0}</b>     공급 장치의 충전 후 잔량  {inventory.Supply - selected:0.0}\n" +
                "<size=20>시간 정지 중 · 1 / 2 / 3 교체 · 숫자는 게임용 에너지 단위입니다.</size>";
            left.interactable = selected > 0;
        }

        private IEnumerator Charge()
        {
            IsCharging = true;
            float pending = amount.value;
            float requested = pending;
            left.interactable = right.interactable = close.interactable = false;
            amount.interactable = minus.interactable = plus.interactable = false;
            foreach (Button selector in selectors) selector.interactable = false;
            while (pending > 0.001f)
            {
                float delivered = stage.Inventory.Charge(Mathf.Min(pending, Time.unscaledDeltaTime * 30));
                pending -= delivered;
                modalBody.text = $"{CapacitorInventory.Names[stage.Inventory.Selected]} 충전 중\n" +
                    $"<color=#82E5FF>{stage.Inventory.Current:0.0}</color> / {CapacitorInventory.Capacities[stage.Inventory.Selected]:0}";
                modalDetail.text = $"전달한 에너지 {requested - pending:0.0} / {requested:0}\n공급 잔량 {stage.Inventory.Supply:0.0}";
                if (delivered <= 0 && Time.unscaledDeltaTime > 0) break;
                yield return null;
            }
            IsCharging = false;
            right.interactable = close.interactable = true;
            amount.interactable = minus.interactable = plus.interactable = true;
            RefreshCharge();
        }

        public void ShowQuiz()
        {
            Show($"구역 {stage.definition.number:00} 해제 · OX 확인", stage.definition.question);
            modalDetail.text = "방금 사용한 축전기의 원리를 생각해 보세요.";
            Button(left, "O  맞다", () => stage.Answer(true));
            Button(right, "X  아니다", () => stage.Answer(false));
        }

        public void ShowAnswer(bool correct)
        {
            Show(correct ? "정답이에요" : "확인하고 넘어가요", stage.definition.question);
            modalDetail.fontSize = 24;
            modalDetail.alignment = TextAlignmentOptions.TopLeft;
            modalDetail.rectTransform.anchoredPosition = new Vector2(0, -65);
            modalDetail.rectTransform.sizeDelta = new Vector2(980, 220);
            modalDetail.text = $"<b>정답: {(stage.definition.answer ? "O" : "X")}</b>\n{stage.definition.explanation}";
            Button(middle, stage.definition.number == 15 ? "탈출 완료" : "다음 스테이지", () => stage.Advance());
        }

        public void ShowPause()
        {
            Show("잠시 정지", "충전된 에너지와 장치의 시간이 멈췄어요.");
            modalDetail.text = "WASD / 방향키 이동 · 1 / 2 / 3 교체 · E 상호작용\nR 3초 재시작 · H 힌트 · Esc 계속하기";
            Button(left, "계속하기", () => stage.CloseModal());
            Button(right, "타이틀", () => stage.ToTitle());
        }

        public void ShowTitle()
        {
            gameplay.SetActive(false);
            Show("미정", "전원이 끊긴 연구실.\n남은 에너지를 나눠 담아, 15개의 구역을 통과하세요.");
            modalDetail.text = "충전 · 교체 · 상자와 발판 · 매 구역 OX 퀴즈\n예상 플레이 15~20분  |  WASD 이동 · E 상호작용";
            Button(left, "새 게임", () => stage.NewGame());
            Button(middle, "이어하기", () => stage.ContinueGame());
            middle.interactable = PlayerPrefs.HasKey("JJW.NextStage");
            Button(right, "종료", () => stage.Quit());
        }

        public void ShowEnding()
        {
            int correct = 0;
            for (int i = 1; i <= 15; i++) correct += PlayerPrefs.GetInt("JJW.Answer." + i, 0);
            Show("연구실 밖으로", "작은 빛들을 모아 마지막 문을 열었습니다.\n15개 구역 탈출 완료!");
            modalDetail.text = $"개념 확인 {correct} / 15\n축전기는 에너지를 저장하고, 필요할 때 장치에 공급합니다.\n저장량과 출력은 다르고, 사용하면 남은 에너지가 줄어듭니다.";
            Button(middle, "타이틀로", () => stage.ToTitle());
        }
    }
}
