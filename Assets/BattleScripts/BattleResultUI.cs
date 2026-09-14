using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class BattleResultUI : MonoBehaviour
{
    [Header("Головні панелі")]
    public GameObject resultPanel;      // Напівпрозорий затемнений фон + вікно
    public TMP_Text titleText;          // Текст "ПЕРЕМОГА" / "ПОРАЗКА"
    public Button exitButton;           // Кнопка виходу

    [Header("Контейнери втрат HOMM3")]
    public Transform playerLossesContainer;
    public Transform enemyLossesContainer;
    public GameObject lossEntryPrefab;  // Префаб: Image (іконка) + Text (кількість втрат)

    private void Awake()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitButtonClicked);
    }

    public void ShowResults(bool isPlayerVictory, List<BattleUnit> allRegisteredUnits)
    {
        if (resultPanel != null) resultPanel.SetActive(true);

        // 1. Заголовок
        if (titleText != null)
        {
            titleText.text = isPlayerVictory ? "ПЕРЕМОГА!" : "ПОРАЗКА!";
            titleText.color = isPlayerVictory ? Color.green : Color.red;
        }

        // 2. Очищення старих записів UI
        ClearContainer(playerLossesContainer);
        ClearContainer(enemyLossesContainer);

        // 3. Формування списку втрат
        foreach (var unit in allRegisteredUnits)
        {
            if (unit == null || unit.data == null) continue;

            int lostCount = unit.initialStackSize - unit.stackSize;
            if (lostCount <= 0) continue; // Якщо втрат немає — не виводимо

            Transform targetContainer = (unit.teamId == 0) ? playerLossesContainer : enemyLossesContainer;
            CreateLossEntry(unit.data.idleSprite, lostCount, targetContainer);
        }
    }

    private void CreateLossEntry(Sprite unitSprite, int lostAmount, Transform container)
    {
        if (lossEntryPrefab == null || container == null) return;

        GameObject entry = Instantiate(lossEntryPrefab, container);

        Image icon = entry.GetComponentInChildren<Image>();
        TMP_Text countText = entry.GetComponentInChildren<TMP_Text>();

        if (icon != null) icon.sprite = unitSprite;
        if (countText != null) countText.text = $"-{lostAmount}";
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
        // Перезавантаження поточної сцени (або завантаження головного меню)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}