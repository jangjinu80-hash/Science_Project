using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JJW.Code.Player
{
    public sealed class LabHudStyle
    {
        public static readonly Color Background = Hex("171D23");
        public static readonly Color Surface = Hex("303C48");
        public static readonly Color Raised = Hex("445362");
        public static readonly Color Border = Hex("7A8791");
        public static readonly Color Text = Hex("F1EBD9");
        public static readonly Color Muted = Hex("C7CCCB");
        public static readonly Color Accent = Hex("D5B66C");
        public static readonly Color Amber = Hex("E5C985");
        public static readonly Color[] Capacitors = { Hex("ADCADB"), Hex("E0B28D"), Hex("B8C998") };
        private readonly LabHud hud;
        private readonly Image[] markers = new Image[3];
        private readonly TMP_Text[] badges = new TMP_Text[3];
        private readonly Image[] steps = new Image[15];
        private readonly TMP_Text category;
        private readonly GameObject titleArt;
        private readonly Image paper;
        private bool reading;

        public LabHudStyle(LabHud target)
        {
            hud = target;
            var scaler = hud.GetComponent<CanvasScaler>();
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            foreach (TMP_Text text in hud.GetComponentsInChildren<TMP_Text>(true)) text.color = Text;
            Image top = hud.stageTitle.transform.parent.GetComponent<Image>();
            Flat(top, Background);
            Place(top.rectTransform, 0, -86, 1600, 172);
            top.rectTransform.anchorMin = top.rectTransform.anchorMax = new Vector2(.5f, 1);
            Place(hud.stageTitle.rectTransform, -552, 35, 432, 44);
            hud.stageTitle.fontSize = 25;
            hud.stageTitle.enableAutoSizing = true;
            hud.stageTitle.fontSizeMin = 20;
            hud.stageTitle.fontSizeMax = 25;
            Place(hud.stageObjective.rectTransform, -552, -22, 432, 66);
            hud.stageObjective.color = Muted;
            hud.stageObjective.fontSizeMin = 18;
            hud.stageObjective.fontSizeMax = 20;
            Panel(top.transform, "HeaderRule", Border, 0, -83, 1600, 4);
            for (int i = 0; i < 15; i++)
                steps[i] = Panel(top.transform, "StageProgress" + i, Raised, -756 + i * 28, 73, 18, 6);

            for (int i = 0; i < 3; i++)
            {
                Image card = hud.capacitorCards[i];
                Place(card.rectTransform, -180 + i * 280, 0, 264, 140);
                Flat(card, Surface);
                Frame(card);
                Place(hud.capacitorTexts[i].rectTransform, 0, 8, 232, 106);
                hud.capacitorTexts[i].fontSize = 20;
                hud.capacitorTexts[i].alignment = TextAlignmentOptions.TopLeft;
                hud.capacitorTexts[i].lineSpacing = 4;
                markers[i] = Panel(card.transform, "SelectionLamp", Capacitors[i], 110, -32, 8, 8);
                badges[i] = Label(card.transform, "SelectionLabel", "", 15, 77, 48, 78, 24);
                badges[i].alignment = TextAlignmentOptions.Right;
                var track = card.transform.Find("Track");
                if (track != null)
                {
                    Flat(track.GetComponent<Image>(), Background);
                    Place((RectTransform)track, 0, -55, 232, 10);
                }
                Place(hud.capacitorBars[i].rectTransform, 0, -55, 232, 10);
                hud.capacitorBars[i].color = Capacitors[i];
                for (int tick = 1; tick < 12; tick++)
                    Panel(card.transform, "MeterGap" + tick, Surface, -116 + tick * 232f / 12, -55, 3, 10);
            }
            Transform supplyParent = hud.supply.transform.parent;
            if (supplyParent == top.transform)
            {
                Image card = Panel(top.transform, "SupplyCard", Surface, 650, 0, 236, 140);
                hud.supply.transform.SetParent(card.transform, false);
                supplyParent = card.transform;
            }
            Place((RectTransform)supplyParent, 650, 0, 236, 140);
            Flat(supplyParent.GetComponent<Image>(), Surface);
            Frame(supplyParent.GetComponent<Image>());
            Place(hud.supply.rectTransform, 0, 0, 208, 112);
            hud.supply.fontSize = 20;
            hud.supply.alignment = TextAlignmentOptions.Center;
            hud.supply.textWrappingMode = TextWrappingModes.NoWrap;

            Image bottom = hud.prompt.transform.parent.GetComponent<Image>();
            Flat(bottom, Background);
            Place(bottom.rectTransform, 0, 54, 1600, 108);
            bottom.rectTransform.anchorMin = bottom.rectTransform.anchorMax = new Vector2(.5f, 0);
            Panel(bottom.transform, "FooterRule", Border, 0, 51, 1600, 4);
            Place(hud.prompt.rectTransform, -200, 7, 1120, 72);
            hud.prompt.fontSize = 23;
            hud.prompt.enableAutoSizing = true;
            hud.prompt.fontSizeMin = 18;
            hud.prompt.fontSizeMax = 23;
            Place(hud.restartText.rectTransform, 594, 8, 348, 52);
            hud.restartText.fontSize = 18;
            hud.restartText.color = Muted;
            Place(hud.restartBar.rectTransform, 594, -30, 348, 4);
            hud.restartBar.color = Amber;
            var controls = hud.gameplay.transform.Find("Controls") as RectTransform;
            if (controls != null)
            {
                Place(controls, 0, 126, 1500, 28);
                controls.anchorMin = controls.anchorMax = new Vector2(.5f, 0);
                controls.GetComponent<TMP_Text>().color = Muted;
            }
            Flat(hud.modal.GetComponent<Image>(), new Color(.02f, .04f, .06f, .97f));
            Image content = hud.modalTitle.transform.parent.GetComponent<Image>();
            Flat(content, Surface);
            Place(content.rectTransform, 0, 0, 1280, 720);
            Frame(content);
            Transform oldAccent = content.transform.Find("Accent");
            if (oldAccent != null) oldAccent.gameObject.SetActive(false);
            for (int side = -1; side <= 1; side += 2)
                for (int row = -1; row <= 1; row += 2)
                {
                    Panel(content.transform, "Bolt" + side + "_" + row, Border, side * 612, row * 332, 12, 12);
                    Panel(content.transform, "BoltSlot" + side + "_" + row, Background, side * 612, row * 332, 8, 3);
                }
            for (int i = 0; i < 12; i++)
                Panel(content.transform, "WarningStripe" + i, i % 2 == 0 ? Accent : Background, -538 + i * 14, 210, 14, 8);
            Panel(content.transform, "HeaderDivider", Background, 106, 210, 908, 8);
            Panel(content.transform, "FooterDivider", Background, 0, -226, 1120, 4);
            paper = Panel(content.transform, "QuizPaper", Hex("E5DDC6"), 0, 0, 1160, 404);
            paper.transform.SetAsFirstSibling();
            category = Label(content.transform, "ScreenCategory", "연구실 제어반", 19, -85, 302, 950, 28);
            category.color = Accent;
            hud.modalTitle.fontSize = 38;
            hud.modalBody.fontSize = 28;
            hud.modalBody.lineSpacing = 8;
            hud.modalDetail.lineSpacing = 7;
            Place((RectTransform)hud.close.transform, 508, 300, 104, 44);
            StyleButton(hud.close, false);
            Place((RectTransform)hud.left.transform, -384, -286, 352, 64);
            Place((RectTransform)hud.middle.transform, 0, -286, 352, 64);
            Place((RectTransform)hud.right.transform, 384, -286, 352, 64);
            Place((RectTransform)hud.chargeControls.transform, 0, -10, 1120, 120);
            for (int i = 0; i < 3; i++)
                Place((RectTransform)hud.selectors[i].transform, -384 + i * 384, 45, 352, 52);
            Place((RectTransform)hud.amount.transform, 0, -35, 880, 44);
            RectTransform trackRect = hud.amount.transform.Find("Track") as RectTransform;
            if (trackRect != null)
            {
                Place(trackRect, 0, 0, 880, 8);
                Flat(trackRect.GetComponent<Image>(), Border);
            }
            Place((RectTransform)hud.amount.fillRect.parent, 0, 0, 880, 8);
            hud.amount.fillRect.GetComponent<Image>().color = Amber;
            hud.amount.handleRect.GetComponent<Image>().color = Text;
            Place((RectTransform)hud.minus.transform, -516, -35, 88, 48);
            Place((RectTransform)hud.plus.transform, 516, -35, 88, 48);
            StyleButton(hud.minus, false);
            StyleButton(hud.plus, false);
            titleArt = CreateTitleArt(content.transform);
            Screen("연구실 제어반");
        }

        public void Stage(int number)
        {
            for (int i = 0; i < steps.Length; i++)
                steps[i].color = i < number ? Accent : Raised;
        }

        public void Capacitor(int index, bool selected, bool unlocked)
        {
            hud.capacitorCards[index].color = selected ? Raised : Background;
            markers[index].color = selected ? Capacitors[index] : Border;
            badges[index].text = unlocked ? selected ? "사용 중" : "대기" : "잠김";
            badges[index].color = selected ? Capacitors[index] : Muted;
            hud.capacitorTexts[index].color = unlocked ? Text : Muted;
        }

        public void Screen(string label, bool title = false)
        {
            category.text = label;
            reading = label == "개념 확인" || label == "해설";
            paper.gameObject.SetActive(reading);
            titleArt.SetActive(title);
            Place(hud.modalTitle.rectTransform, title ? -224 : 0, title ? 150 : 247, title ? 672 : 1120, title ? 100 : 64);
            hud.modalTitle.fontSize = title ? 72 : 38;
            Place(hud.modalBody.rectTransform, title ? -224 : 0, title ? 38 : 136, title ? 672 : 1120, 128);
            Place(hud.modalDetail.rectTransform, title ? -224 : 0, title ? -65 : -108, title ? 672 : 1120, title ? 84 : 176);
            hud.modalTitle.transform.parent.Find("FooterDivider").gameObject.SetActive(!title);
            Place((RectTransform)hud.left.transform, title ? -350 : -384, title ? -160 : -286, title ? 420 : 352, title ? 52 : 64);
            Place((RectTransform)hud.middle.transform, title ? -350 : 0, title ? -230 : -286, title ? 420 : 352, title ? 52 : 64);
            Place((RectTransform)hud.right.transform, title ? -350 : 384, title ? -300 : -286, title ? 420 : 352, title ? 52 : 64);
            hud.modalDetail.fontSize = 23;
            hud.modalDetail.color = Muted;
            hud.modalBody.color = reading ? Background : Text;
            if (reading) hud.modalDetail.color = Hex("39434A");
            hud.modalDetail.alignment = TextAlignmentOptions.TopLeft;
            StyleButton(hud.left, true);
            StyleButton(hud.middle, true);
            StyleButton(hud.right, false);
        }

        public void Answer()
        {
            Place(hud.modalDetail.rectTransform, 0, -84, 1120, 232);
            hud.modalDetail.fontSize = 24;
            hud.modalDetail.color = reading ? Background : Text;
        }

        public void Charge()
        {
            Place(hud.modalDetail.rectTransform, 0, -150, 1120, 112);
        }

        public void Selector(int index, bool selected)
        {
            StyleButton(hud.selectors[index], selected);
        }

        public static void StyleButton(Button button, bool primary)
        {
            Image image = button.GetComponent<Image>();
            Flat(image, Color.white);
            Frame(image);
            ColorBlock colors = button.colors;
            colors.normalColor = primary ? Accent : Raised;
            colors.highlightedColor = primary ? Hex("E8CD8C") : Hex("607181");
            colors.selectedColor = primary ? Hex("F4DEAA") : Hex("6C7D8B");
            colors.pressedColor = primary ? Hex("AD8B4A") : Hex("344250");
            colors.disabledColor = Hex("303840");
            colors.fadeDuration = .06f;
            button.colors = colors;
            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            label.color = primary ? Background : Text;
            label.fontSize = 23;
            var rect = label.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(12, 4);
            rect.offsetMax = new Vector2(-12, -4);
        }

        private GameObject CreateTitleArt(Transform parent)
        {
            var root = new GameObject("CapacitorSchematic", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            Place((RectTransform)root.transform, 364, 5, 328, 370);
            Label(root.transform, "DiagramLabel", "휴대용 축전기 보관함", 21, 0, 170, 310, 28).color = Text;
            Image tray = Panel(root.transform, "StorageTray", Background, 0, 10, 328, 264);
            tray.transform.SetAsFirstSibling();
            Frame(tray);
            for (int i = 0; i < 3; i++)
            {
                float x = -104 + i * 104;
                Image casing = Panel(root.transform, "Cell" + i, Raised, x, 16, 72, 128);
                Frame(casing);
                Panel(root.transform, "PositiveLead" + i, Muted, x - 18, 103, 6, 42);
                Panel(root.transform, "NegativeLead" + i, Muted, x + 18, 96, 6, 28);
                Panel(root.transform, "CellBand" + i, Capacitors[i], x, 45, 72, 18);
                Panel(root.transform, "CellFoot" + i, Border, x, -48, 72, 8);
                Label(root.transform, "CellNumber" + i, "0" + (i + 1), 26, x, 0, 58, 36).alignment = TextAlignmentOptions.Center;
                Label(root.transform, "CellType" + i, CapacitorInventory.Names[i], 18, x, -88, 84, 28).alignment = TextAlignmentOptions.Center;
            }
            Label(root.transform, "DiagramCaption", "필요한 만큼 담고, 남김없이 쓰기", 18, 0, -160, 328, 32).alignment = TextAlignmentOptions.Center;
            return root;
        }

        private TMP_Text Label(Transform parent, string name, string value, float size, float x, float y, float w, float h)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent, false);
            TMP_Text label = obj.GetComponent<TMP_Text>();
            label.font = hud.stageTitle.font;
            label.text = value;
            label.fontSize = size;
            label.color = Text;
            label.raycastTarget = false;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            Place(label.rectTransform, x, y, w, h);
            return label;
        }

        private static Image Panel(Transform parent, string name, Color color, float x, float y, float w, float h)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            Image image = obj.GetComponent<Image>();
            image.raycastTarget = false;
            Flat(image, color);
            Place(image.rectTransform, x, y, w, h);
            return image;
        }

        private static void Frame(Image image)
        {
            Outline outline = image.GetComponent<Outline>();
            if (outline == null) outline = image.gameObject.AddComponent<Outline>();
            outline.effectColor = Background;
            outline.effectDistance = new Vector2(4, -4);
            outline.useGraphicAlpha = false;
            if (image.transform.Find("FrameLight") == null)
            {
                Edge(image.transform, "FrameLight", Border, true);
                Edge(image.transform, "FrameShade", Hex("202830"), false);
            }
        }

        private static void Edge(Transform parent, string name, Color color, bool top)
        {
            Image edge = Panel(parent, name, color, 0, 0, 0, 3);
            RectTransform rect = edge.rectTransform;
            rect.anchorMin = new Vector2(0, top ? 1 : 0);
            rect.anchorMax = new Vector2(1, top ? 1 : 0);
            rect.pivot = new Vector2(.5f, top ? 1 : 0);
            rect.sizeDelta = new Vector2(0, 3);
        }

        private static void Flat(Image image, Color color)
        {
            image.sprite = null;
            image.type = Image.Type.Simple;
            image.color = color;
        }

        private static void Place(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString("#" + value, out Color color);
            return color;
        }
    }
}
