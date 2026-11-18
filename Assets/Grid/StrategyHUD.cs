using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Runtime map HUD backed by the existing TeamManager and HeroController data.
public class StrategyHUD : MonoBehaviour
{
    private static StrategyHUD instance;
    private readonly Color background = new Color32(166, 127, 80, 255);
    private readonly Color brass = new Color32(239, 209, 143, 255);
    private readonly Color muted = new Color32(212, 193, 154, 255);
    [SerializeField, HideInInspector] private List<HeroCard> cards = new List<HeroCard>();
    [SerializeField, HideInInspector] private Text[] balances = new Text[ResourceCatalog.Count];
    [SerializeField, HideInInspector] private Font font;
    [SerializeField, HideInInspector] private Canvas canvas;
    [SerializeField, HideInInspector] private RectTransform topBar, sidebar, toast;
    [SerializeField, HideInInspector] private Transform heroContent;
    [SerializeField, HideInInspector] private Text emptyLabel, toastText;
    [SerializeField, HideInInspector] private Image toastIcon;
    [SerializeField, HideInInspector] private float toastUntil, refreshAt;
    [SerializeField, HideInInspector] private int displayedTeam = int.MinValue;

    [System.Serializable]
    private class HeroCard
    {
        public HeroController hero;
        public GameObject root;
        public Image background, manaFill, movementFill;
        public Text name, mana, movement;
    }

    public static void EnsureExists()
    {
        if (instance == null) instance = FindAnyObjectByType<StrategyHUD>();
        if (instance == null) new GameObject("Strategy HUD").AddComponent<StrategyHUD>();
    }

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        font = Resources.Load<Font>("StrategyUI/ClassicSerif");
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Build();
        // The map's old UI has two separate Canvases. Disable both rendering and input.
        foreach (Canvas old in FindObjectsByType<Canvas>())
            if (old != canvas && old.gameObject.scene == gameObject.scene)
            {
                old.enabled = false;
                GraphicRaycaster raycaster = old.GetComponent<GraphicRaycaster>();
                if (raycaster != null) raycaster.enabled = false;
            }
    }

    private void OnEnable()
    {
        instance = this;
        // Runtime Button listeners are not serialized across an Editor script reload.
        if (sidebar == null) return;
        Button end = sidebar.Find("End turn").GetComponent<Button>();
        end.onClick.RemoveAllListeners();
        end.onClick.AddListener(EndTurn);
        foreach (HeroCard card in cards)
            if (card.root != null) BindCard(card.root.GetComponent<Button>(), card.hero);
    }

    private void EndTurn()
    {
        if (HeroSelectionManager.Instance != null) HeroSelectionManager.Instance.EndTurn();
        Refresh();
    }
    private void OnDestroy()
    {
        if (instance != this) return;
        instance = null;
    }

    public static bool BlocksMapInput(Vector2 point)
    {
        if (instance == null || !instance.isActiveAndEnabled || instance.canvas == null || !instance.canvas.enabled) return false;
        return RectTransformUtility.RectangleContainsScreenPoint(instance.topBar, point) ||
            RectTransformUtility.RectangleContainsScreenPoint(instance.sidebar, point);
    }

    public static void NotifyPickup(int teamId, ResourceType type, int amount)
    {
        if (instance == null || teamId != instance.CurrentTeamId) return;
        instance.toastText.text = $"+{amount:N0}  {ResourceCatalog.GetName(type)}";
        instance.toastIcon.sprite = ResourceCatalog.GetSprite(type);
        instance.toastUntil = Time.unscaledTime + 2f;
        instance.toast.gameObject.SetActive(true);
        instance.Refresh();
    }

    private int CurrentTeamId => HeroSelectionManager.Instance != null ? HeroSelectionManager.Instance.currentTeamId : 0;

    private void Update()
    {
        if (toast != null && toast.gameObject.activeSelf && Time.unscaledTime >= toastUntil) toast.gameObject.SetActive(false);
        if (Time.unscaledTime < refreshAt) return;
        refreshAt = Time.unscaledTime + 0.1f;
        Refresh();
    }

    private void Build()
    {
        GameObject ui = new GameObject("Map UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        ui.transform.SetParent(transform, false);
        canvas = ui.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvas.pixelPerfect = true;
        CanvasScaler scaler = ui.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600, 900);
        scaler.matchWidthOrHeight = 0.5f;

        topBar = Panel("Resource bar", ui.transform, background);
        Place(topBar, new Vector2(0, 1), Vector2.one, new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 44));
        RectTransform resourceRow = Panel("Resources", topBar, background);
        Place(resourceRow, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        resourceRow.offsetMin = new Vector2(12, 5); resourceRow.offsetMax = new Vector2(-12, -5);
        HorizontalLayoutGroup row = resourceRow.gameObject.AddComponent<HorizontalLayoutGroup>();
        row.spacing = 12; row.childControlWidth = true; row.childForceExpandWidth = true;
        row.childControlHeight = true; row.childForceExpandHeight = true;
        for (int i = 0; i < ResourceCatalog.Count; i++)
        {
            ResourceType type = (ResourceType)i;
            RectTransform item = Panel(type.ToString(), resourceRow, background);
            item.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            Image icon = Picture("Icon", item, ResourceCatalog.GetSprite(type));
            Place(icon.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(4, 0), new Vector2(32, 32));
            balances[i] = Label("Balance", item, "0", 16, Color.white, Vector2.zero, Vector2.zero);
            Place(balances[i].rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            balances[i].rectTransform.offsetMin = new Vector2(44, 0);
            balances[i].rectTransform.offsetMax = Vector2.zero;
            balances[i].resizeTextForBestFit = true; balances[i].resizeTextMinSize = 11; balances[i].resizeTextMaxSize = 16;
        }

        sidebar = Panel("Hero sidebar", ui.transform, background);
        Place(sidebar, new Vector2(1, 0), Vector2.one, new Vector2(1, 0.5f), new Vector2(0, -22), new Vector2(240, -44));
        RectTransform overview = Panel("Minimap frame", sidebar, background);
        Place(overview, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(10, -10), new Vector2(-20, 184));
        GameObject mini = new GameObject("Minimap", typeof(RectTransform), typeof(RawImage), typeof(StrategyMinimap));
        mini.transform.SetParent(overview, false);
        RectTransform miniRect = mini.GetComponent<RectTransform>();
        Place(miniRect, Vector2.zero, Vector2.one, Vector2.one * 0.5f, Vector2.zero, new Vector2(-12, -12));
        Label("Heading", sidebar, "Heroes", 17, brass, new Vector2(12, -204), new Vector2(210, 24));
        RectTransform view = Panel("Hero list", sidebar, background);
        Place(view, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        view.offsetMin = new Vector2(10, 66); view.offsetMax = new Vector2(-10, -236);
        view.gameObject.AddComponent<RectMask2D>();
        ScrollRect scroll = view.gameObject.AddComponent<ScrollRect>();
        scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped;
        RectTransform content = Panel("Content", view, background);
        Place(content, new Vector2(0, 1), Vector2.one, new Vector2(0.5f, 1), Vector2.zero, Vector2.zero);
        VerticalLayoutGroup column = content.gameObject.AddComponent<VerticalLayoutGroup>();
        column.spacing = 4; column.childControlHeight = true; column.childForceExpandHeight = false;
        column.childControlWidth = true; column.childForceExpandWidth = true;
        ContentSizeFitter fit = content.gameObject.AddComponent<ContentSizeFitter>(); fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.viewport = view; scroll.content = content; heroContent = content;
        emptyLabel = Label("Empty", view, "No heroes", 12, muted, new Vector2(8, -8), new Vector2(180, 30));
        RectTransform end = Panel("End turn", sidebar, new Color32(203, 164, 104, 255));
        Place(end, Vector2.zero, new Vector2(1, 0), Vector2.zero, new Vector2(12, 12), new Vector2(-24, 40));
        Button endButton = end.gameObject.AddComponent<Button>();
        endButton.onClick.AddListener(EndTurn);
        CenterLabel(end, "End turn", 17, brass);

        toast = Panel("Pickup notification", ui.transform, background);
        Place(toast, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(8, 8), new Vector2(220, 36));
        toastIcon = Picture("Icon", toast, null);
        Place(toastIcon.rectTransform, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(4, 2), new Vector2(32, 32));
        toastText = Label("Text", toast, "", 14, Color.white, new Vector2(44, -4), new Vector2(170, 28));
        toast.gameObject.SetActive(false);
    }

    private void Refresh()
    {
        Team team = TeamManager.Instance != null ? TeamManager.Instance.GetTeam(CurrentTeamId) : null;
        for (int i = 0; i < balances.Length; i++) if (balances[i] != null) balances[i].text = (team != null ? team.GetResource((ResourceType)i) : 0).ToString("N0");
        bool changed = displayedTeam != CurrentTeamId;
        int count = 0;
        if (team != null && team.heroes != null)
            foreach (HeroController hero in team.heroes)
                if (hero != null)
                {
                    if (count >= cards.Count || cards[count].hero != hero) changed = true;
                    count++;
                }
        if (changed || count != cards.Count)
        {
            foreach (HeroCard card in cards) if (card.root != null) Destroy(card.root);
            cards.Clear(); displayedTeam = CurrentTeamId;
            if (team != null && team.heroes != null)
                foreach (HeroController hero in team.heroes) if (hero != null) cards.Add(CreateCard(hero));
        }
        if (emptyLabel != null) emptyLabel.gameObject.SetActive(cards.Count == 0);
        foreach (HeroCard card in cards)
        {
            if (card.hero == null) continue;
            HeroController hero = card.hero;
            card.name.text = hero.army != null && !string.IsNullOrEmpty(hero.army.heroName) ? hero.army.heroName : hero.name.Replace("(Clone)", "");
            bool selected = HeroSelectionManager.Instance != null && HeroSelectionManager.Instance.selectedHero == hero;
            card.background.color = selected ? new Color32(235, 204, 143, 255) : new Color32(168, 137, 96, 255);
            int maximum = hero.stats != null ? hero.stats.MaxMana : 0;
            int mana = hero.stats != null ? Mathf.Clamp(hero.stats.mana, 0, maximum) : 0;
            card.mana.text = $"Mana  {mana}/{maximum}";
            card.movement.text = $"Move  {hero.movementPoints}/{hero.maxMovementPoints}";
            SetFill(card.manaFill, maximum > 0 ? (float)mana / maximum : 0);
            SetFill(card.movementFill, hero.maxMovementPoints > 0 ? (float)hero.movementPoints / hero.maxMovementPoints : 0);
        }
    }

    private HeroCard CreateCard(HeroController hero)
    {
        HeroCard card = new HeroCard { hero = hero };
        RectTransform root = Panel("Hero card", heroContent, new Color32(168, 137, 96, 255));
        card.root = root.gameObject; card.background = root.GetComponent<Image>();
        root.gameObject.AddComponent<LayoutElement>().preferredHeight = 102;
        Button button = root.gameObject.AddComponent<Button>();
        BindCard(button, hero);
        RectTransform portraitFrame = Panel("Portrait frame", root, background);
        Place(portraitFrame, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(8, -8), new Vector2(66, 86));
        Image portrait = Picture("Portrait", portraitFrame, Resources.Load<Sprite>("StrategyUI/zelensky-portrait"));
        Place(portrait.rectTransform, Vector2.zero, Vector2.one, Vector2.one * 0.5f, Vector2.zero, new Vector2(-8, -8));
        card.name = Label("Hero name", root, hero.name, 16, brass, new Vector2(82, -9), new Vector2(126, 25));
        card.mana = Label("Mana", root, "", 13, muted, new Vector2(82, -38), new Vector2(126, 20));
        card.manaFill = Meter(root, "Mana bar", 60, new Color32(70, 111, 183, 255));
        card.movement = Label("Movement", root, "", 13, muted, new Vector2(82, -67), new Vector2(126, 20));
        card.movementFill = Meter(root, "Movement bar", 89, new Color32(94, 146, 61, 255));
        return card;
    }

    private void BindCard(Button button, HeroController hero)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            if (hero == null || HeroSelectionManager.Instance == null || hero.teamId != CurrentTeamId) return;
            HeroSelectionManager.Instance.SelectHero(hero);
            Camera cam = Camera.main;
            if (cam != null) cam.transform.position = new Vector3(hero.transform.position.x + cam.orthographicSize * cam.aspect * 0.14f, hero.transform.position.y, cam.transform.position.z);
            Refresh();
        });
    }

    private Image Meter(Transform parent, string name, float y, Color color)
    {
        RectTransform back = Panel(name, parent, new Color32(37, 26, 20, 255));
        Place(back, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(82, -y), new Vector2(-94, 5));
        RectTransform fill = Panel("Fill", back, color);
        Place(fill, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Vector2.zero);
        return fill.GetComponent<Image>();
    }
    private static void SetFill(Image fill, float amount) { fill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(amount), 1); }
    private RectTransform Panel(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>(); image.color = color;
        bool plain = name == "Fill" || name.EndsWith(" bar") && name != "Resource bar" || name == "Icon" || name == "Portrait";
        if (!plain)
        {
            image.sprite = ClassicStrategySkin.Wood;
            image.type = Image.Type.Tiled;
        }
        bool framed = name == "Resource bar" || name == "Hero sidebar" || name == "Hero card" ||
            name == "End turn" || name == "Pickup notification" || name.EndsWith(" frame");
        if (framed)
        {
            GameObject border = new GameObject("Frame", typeof(RectTransform), typeof(Image));
            border.transform.SetParent(go.transform, false);
            Image frame = border.GetComponent<Image>(); frame.sprite = ClassicStrategySkin.Frame;
            frame.type = Image.Type.Sliced; frame.fillCenter = false; frame.raycastTarget = false;
            Place(frame.rectTransform, Vector2.zero, Vector2.one, Vector2.one * 0.5f, Vector2.zero, Vector2.zero);
        }
        return go.GetComponent<RectTransform>();
    }
    private Image Picture(string name, Transform parent, Sprite sprite)
    {
        RectTransform rect = Panel(name, parent, Color.white);
        Image image = rect.GetComponent<Image>(); image.sprite = sprite; image.preserveAspect = true; image.raycastTarget = false;
        return image;
    }
    private Text Label(string name, Transform parent, string value, int size, Color color, Vector2 position, Vector2 dimensions)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false);
        Text text = go.GetComponent<Text>(); text.font = font; text.text = value; text.fontSize = size; text.color = color;
        text.alignment = TextAnchor.MiddleLeft; text.raycastTarget = false; text.supportRichText = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate;
        Place(text.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), position, dimensions);
        return text;
    }
    private void CenterLabel(Transform parent, string value, int size, Color color)
    {
        Text text = Label("Label", parent, value, size, color, Vector2.zero, Vector2.zero);
        Place(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        text.alignment = TextAnchor.MiddleCenter;
    }
    private static void Place(RectTransform rect, Vector2 min, Vector2 max, Vector2 pivot, Vector2 position, Vector2 size)
    { rect.anchorMin = min; rect.anchorMax = max; rect.pivot = pivot; rect.sizeDelta = size; rect.anchoredPosition = position; }
}
