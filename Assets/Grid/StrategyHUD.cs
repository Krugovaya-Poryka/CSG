using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Runtime map HUD backed by the existing TeamManager and HeroController data.
public class StrategyHUD : MonoBehaviour
{
    private static StrategyHUD instance;
    private readonly Color background = new Color32(19, 27, 31, 250);
    private readonly Color brass = new Color32(206, 172, 103, 255);
    private readonly Color muted = new Color32(151, 166, 169, 255);
    [SerializeField, HideInInspector] private List<HeroCard> cards = new List<HeroCard>();
    [SerializeField, HideInInspector] private List<Canvas> hiddenCanvases = new List<Canvas>();
    [SerializeField, HideInInspector] private Text[] balances = new Text[ResourceCatalog.Count];
    [SerializeField, HideInInspector] private Font font;
    [SerializeField, HideInInspector] private Canvas canvas;
    [SerializeField, HideInInspector] private RectTransform topBar, sidebar, toast;
    [SerializeField, HideInInspector] private Transform heroContent;
    [SerializeField, HideInInspector] private Text teamLabel, emptyLabel, toastText;
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
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Build();
        // Replace only the known unfinished map Canvas, leaving battle/castle UI alone.
        foreach (Canvas old in FindObjectsByType<Canvas>())
            if (old != canvas && old.gameObject.name == "Canvas" && old.transform.Find("HeroPanel") != null)
            {
                hiddenCanvases.Add(old);
                old.enabled = false;
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
        foreach (Canvas old in hiddenCanvases) if (old != null) old.enabled = true;
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
        instance.toastUntil = Time.unscaledTime + 3.5f;
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
        CanvasScaler scaler = ui.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600, 900);
        scaler.matchWidthOrHeight = 0.5f;

        topBar = Panel("Resource bar", ui.transform, background);
        Place(topBar, new Vector2(0, 1), Vector2.one, new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 88));
        RectTransform brand = Panel("Realm", topBar, new Color32(28, 39, 42, 255));
        Place(brand, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(12, 0), new Vector2(182, -12));
        Label("Title", brand, "ТВОЄ КОРОЛІВСТВО", 15, brass, new Vector2(12, -17), new Vector2(165, 24));
        teamLabel = Label("Team", brand, "Команда 1", 13, muted, new Vector2(12, -44), new Vector2(165, 20));
        RectTransform resourceRow = Panel("Resources", topBar, Color.clear);
        Place(resourceRow, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        resourceRow.offsetMin = new Vector2(210, 9); resourceRow.offsetMax = new Vector2(-12, -9);
        HorizontalLayoutGroup row = resourceRow.gameObject.AddComponent<HorizontalLayoutGroup>();
        row.spacing = 6; row.childControlWidth = true; row.childForceExpandWidth = true;
        row.childControlHeight = true; row.childForceExpandHeight = true;
        for (int i = 0; i < ResourceCatalog.Count; i++)
        {
            ResourceType type = (ResourceType)i;
            RectTransform item = Panel(type.ToString(), resourceRow, new Color32(31, 42, 45, 255));
            item.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            Image icon = Picture("Icon", item, ResourceCatalog.GetSprite(type));
            Place(icon.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(5, 0), new Vector2(43, 43));
            Text caption = Label("Name", item, ResourceCatalog.GetName(type), 12, muted, new Vector2(50, -10), new Vector2(112, 19));
            caption.resizeTextForBestFit = true; caption.resizeTextMinSize = 9; caption.resizeTextMaxSize = 12;
            RectTransform cap = caption.rectTransform; cap.anchorMax = Vector2.one; cap.offsetMax = new Vector2(-4, -10); cap.offsetMin = new Vector2(50, -29);
            balances[i] = Label("Balance", item, "0", 21, Color.white, new Vector2(50, -31), new Vector2(105, 29));
            RectTransform value = balances[i].rectTransform; value.anchorMax = Vector2.one; value.offsetMax = new Vector2(-4, -31); value.offsetMin = new Vector2(50, -61);
            balances[i].resizeTextForBestFit = true; balances[i].resizeTextMinSize = 12; balances[i].resizeTextMaxSize = 21;
        }
        RectTransform edge = Panel("Gold rule", topBar, brass);
        Place(edge, Vector2.zero, new Vector2(1, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(0, 2));

        sidebar = Panel("Hero sidebar", ui.transform, background);
        Place(sidebar, new Vector2(1, 0), Vector2.one, new Vector2(1, 0.5f), new Vector2(-12, -42), new Vector2(284, -108));
        Label("Heading", sidebar, "ТВОЇ ГЕРОЇ", 22, brass, new Vector2(18, -20), new Vector2(250, 32));
        Label("Hint", sidebar, "Обери героя, щоб перейти до нього", 12, muted, new Vector2(18, -56), new Vector2(250, 28));
        RectTransform view = Panel("Hero list", sidebar, Color.clear);
        Place(view, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        view.offsetMin = new Vector2(12, 160); view.offsetMax = new Vector2(-12, -96);
        view.gameObject.AddComponent<RectMask2D>();
        ScrollRect scroll = view.gameObject.AddComponent<ScrollRect>();
        scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped;
        RectTransform content = Panel("Content", view, Color.clear);
        Place(content, new Vector2(0, 1), Vector2.one, new Vector2(0.5f, 1), Vector2.zero, Vector2.zero);
        VerticalLayoutGroup column = content.gameObject.AddComponent<VerticalLayoutGroup>();
        column.spacing = 10; column.childControlHeight = true; column.childForceExpandHeight = false;
        column.childControlWidth = true; column.childForceExpandWidth = true;
        ContentSizeFitter fit = content.gameObject.AddComponent<ContentSizeFitter>(); fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.viewport = view; scroll.content = content; heroContent = content;
        emptyLabel = Label("Empty", view, "У цієї команди ще немає героїв", 14, muted, new Vector2(8, -12), new Vector2(230, 70));
        Label("Instructions", sidebar, "Двічі натисни клітинку для руху.\nH — характеристики та інвентар.\nРесурси збираються при прибутті.", 13, muted, new Vector2(18, 0), new Vector2(250, 68));
        RectTransform instructions = sidebar.Find("Instructions").GetComponent<RectTransform>();
        Place(instructions, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(18, 82), new Vector2(250, 68));
        RectTransform end = Panel("End turn", sidebar, brass);
        Place(end, Vector2.zero, new Vector2(1, 0), Vector2.zero, new Vector2(14, 16), new Vector2(-28, 48));
        Button endButton = end.gameObject.AddComponent<Button>();
        endButton.onClick.AddListener(EndTurn);
        CenterLabel(end, "ЗАВЕРШИТИ ХІД", 16, background);

        toast = Panel("Pickup notification", ui.transform, background);
        Place(toast, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-140, 24), new Vector2(400, 65));
        toastIcon = Picture("Icon", toast, null);
        Place(toastIcon.rectTransform, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(15, 8), new Vector2(48, 48));
        toastText = Label("Text", toast, "", 23, brass, new Vector2(80, -16), new Vector2(300, 36));
        toast.gameObject.SetActive(false);
    }

    private void Refresh()
    {
        Team team = TeamManager.Instance != null ? TeamManager.Instance.GetTeam(CurrentTeamId) : null;
        if (teamLabel != null) teamLabel.text = team != null && !string.IsNullOrEmpty(team.teamName) && !team.teamName.StartsWith("Team ") ? team.teamName : "Команда " + (CurrentTeamId + 1);
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
            card.background.color = selected ? new Color32(66, 66, 45, 255) : new Color32(32, 45, 49, 255);
            int maximum = hero.stats != null ? hero.stats.MaxMana : 0;
            int mana = hero.stats != null ? Mathf.Clamp(hero.stats.mana, 0, maximum) : 0;
            card.mana.text = $"Мана  {mana} / {maximum}";
            card.movement.text = $"Хід  {hero.movementPoints} / {hero.maxMovementPoints}";
            SetFill(card.manaFill, maximum > 0 ? (float)mana / maximum : 0);
            SetFill(card.movementFill, hero.maxMovementPoints > 0 ? (float)hero.movementPoints / hero.maxMovementPoints : 0);
        }
    }

    private HeroCard CreateCard(HeroController hero)
    {
        HeroCard card = new HeroCard { hero = hero };
        RectTransform root = Panel("Hero card", heroContent, new Color32(32, 45, 49, 255));
        card.root = root.gameObject; card.background = root.GetComponent<Image>();
        root.gameObject.AddComponent<LayoutElement>().preferredHeight = 150;
        Button button = root.gameObject.AddComponent<Button>();
        BindCard(button, hero);
        card.name = Label("Hero name", root, hero.name, 19, brass, new Vector2(14, -12), new Vector2(225, 30));
        card.mana = Label("Mana", root, "", 14, Color.white, new Vector2(14, -49), new Vector2(230, 23));
        card.manaFill = Meter(root, "Mana bar", 78, new Color32(75, 145, 217, 255));
        card.movement = Label("Movement", root, "", 14, Color.white, new Vector2(14, -93), new Vector2(230, 23));
        card.movementFill = Meter(root, "Movement bar", 122, new Color32(123, 174, 89, 255));
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
            if (cam != null) cam.transform.position = new Vector3(hero.transform.position.x + cam.orthographicSize * cam.aspect * 0.17f, hero.transform.position.y, cam.transform.position.z);
            Refresh();
        });
    }

    private Image Meter(Transform parent, string name, float y, Color color)
    {
        RectTransform back = Panel(name, parent, new Color32(10, 18, 21, 255));
        Place(back, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(14, -y), new Vector2(-28, 7));
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
        // Transparent layout containers still need raycasts for scrolling/buttons.
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
