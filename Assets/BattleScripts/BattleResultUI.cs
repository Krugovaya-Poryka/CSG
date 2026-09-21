using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum BattleOutcome
{
    Victory,
    Defeat,
    Surrender
}

public class BattleResultUI : MonoBehaviour
{
    [Header("Головні панелі")]
    public GameObject resultPanel;
    public TMP_Text titleText;
    public TMP_Text resultText; // Описовий текст результату
    public Button exitButton;

    [Header("Контейнери втрат HOMM3")]
    public Transform playerLossesContainer;
    public Transform enemyLossesContainer;
    public GameObject lossEntryPrefab;

    [Header("Фрази для результатів бою ({0} замінюється на ім'я героя)")]
    private readonly string[] victoryPhrases = new string[]
    {
        "{0} бився як лев і виборов заслужену перемогу!",
        "Вороги в паніці розбігаються перед величчю, яку продемонстрував {0}!",
        "Нащадки складатимуть легенди про шедевральну тактику, завдяки якій {0} виграв цей бій!",
        "Завдяки цій звитязі слава про {0}а лунатиме по всій землі!",
        "{0} розгромив ворога вщент, не залишивши йому жодного шансу!",
        "Поле бою належить нам! {0} святкує приголомшливу перемогу.",
        "Тактичний геній {0} знову взяв гору над ворожою ордою!",
        "Ворожий стяг упав. {0} здобуває тріумфальну перемогу!",
        "Ніхто не зміг встояти перед нищівною силою армії {0}а!",
        "Ура! {0} повертається зі щитом і славетною перемогою."
    };

    private readonly string[] defeatPhrases = new string[]
    {
    "{0} гідно бився, але зазнав важкої поразки...",
    "Сьогодні фортуна посміхнулася ворогам. {0} зазнає невдачі.",
    "Армія {0}а розсипалася під нищівним натиском супротивника.",
    "Поле бою вкрите тінями згаслої надії. {0} відступає.",
    "{0} віддав усе в бою, але не зміг стримати переважаючі сили.",
    "Не кожен бій закінчується тріумфом... {0} засвоює гіркий урок.",
    "Ворог виявився надто підступним для виснажених військ {0}.",
    "{0} поліг у жорстокій сутичці, але його хоробрість пам'ятатимуть завжди.",
    "Цей день увійде в історію як тяжкий удар для нашого королівства... {0} зазнав поразки.",
    "Армія {0}а загинула в бою разом зі своїм командиром."
    };

    private readonly string[] surrenderPhrases = new string[]
    {
    "{0} боягузливо втік із поля бою, залишивши своїх солдатів на вірну смерть.",
    "{0} вирушив доживати віку у спокої, щодня випиваючи в таверні.",
    "Більше {0}а ніхто ніколи не бачив... Ходять чутки, що він біжить і досі.",
    "Білий прапор піднято! {0} вирішив, що власна шкура дорожча за честь.",
    "{0} кинув зброю в кущі й непомітно розчинився в тумані.",
    "{0} зберіг своє життя, але назавжди втратив армію.",
    "{0} навіть не спробував здолати ворога і вирішив скласти зброю.",
    "Ганьба й забуття — це все, що залишилося від колишньої слави {0}а.",
    "{0} вирішив, що краще бути живим боягузом, ніж мертвим героєм.",
    "{0} подав у відставку в розпал найгарячішої фази бою."
    };

    private void Awake()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitButtonClicked);
    }

    public void ShowResults(BattleOutcome outcome, List<BattleUnit> allRegisteredUnits, string heroName = "Герой")
    {
        if (resultPanel != null) resultPanel.SetActive(true);

        // Налаштування заголовка та тексту опису
        if (titleText != null)
        {
            switch (outcome)
            {
                case BattleOutcome.Victory:
                    titleText.text = "ПЕРЕМОГА!";
                    titleText.color = Color.green;
                    SetRandomResultText(victoryPhrases, heroName);
                    break;

                case BattleOutcome.Defeat:
                    titleText.text = "ПОРАЗКА!";
                    titleText.color = Color.red;
                    SetRandomResultText(defeatPhrases, heroName);
                    break;

                case BattleOutcome.Surrender:
                    titleText.text = "Капітуляція!";
                    titleText.color = new Color(1.0f, 0.5f, 0.0f); // Помаранчевий
                    SetRandomResultText(surrenderPhrases, heroName);
                    break;
            }
        }

        ClearContainer(playerLossesContainer);
        ClearContainer(enemyLossesContainer);

        foreach (var unit in allRegisteredUnits)
        {
            if (unit == null || unit.data == null) continue;

            int lostCount = unit.initialStackSize - unit.stackSize;
            if (lostCount <= 0) continue;

            Transform targetContainer = (unit.teamId == 0) ? playerLossesContainer : enemyLossesContainer;
            CreateLossEntry(unit.data.lossSprite, lostCount, targetContainer);
        }
    }

    private void SetRandomResultText(string[] phrases, string heroName)
    {
        if (resultText == null || phrases == null || phrases.Length == 0) return;

        int index = Random.Range(0, phrases.Length);
        resultText.text = string.Format(phrases[index], heroName);
    }

    private void CreateLossEntry(Sprite unitSprite, int lostAmount, Transform container)
    {
        if (lossEntryPrefab == null || container == null) return;

        GameObject entry = Instantiate(lossEntryPrefab, container);

        Image icon = entry.transform.Find("Icon")?.GetComponent<Image>();
        if (icon == null) icon = entry.GetComponentInChildren<Image>();

        TMP_Text countText = entry.transform.Find("Text")?.GetComponent<TMP_Text>();
        if (countText == null) countText = entry.GetComponentInChildren<TMP_Text>();

        if (icon != null)
        {
            icon.sprite = unitSprite;
            icon.preserveAspect = true;
            icon.rectTransform.localScale = Vector3.one;
        }
        if (countText != null) countText.text = lostAmount.ToString();
    }

    private void ClearContainer(Transform container)
    {
        if (container == null) return;
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnExitButtonClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}