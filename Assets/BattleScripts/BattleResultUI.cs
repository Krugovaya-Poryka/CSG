using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleResultUI : MonoBehaviour
{
    [Header("Головні панелі")]
    public GameObject resultPanel;
    public TMP_Text titleText;
    public Button exitButton;

    [Header("Контейнери втрат HOMM3")]
    public Transform playerLossesContainer;
    public Transform enemyLossesContainer;
    public GameObject lossEntryPrefab;

    private void Awake()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitButtonClicked);
    }

    public void ShowResults(bool isPlayerVictory, List<BattleUnit> allRegisteredUnits)
    {
        if (resultPanel != null) resultPanel.SetActive(true);

        if (titleText != null)
        {
            titleText.text = isPlayerVictory ? "ПЕРЕМОГА!" : "ПОРАЗКА!";
            titleText.color = isPlayerVictory ? Color.green : Color.red;
        }

        ClearContainer(playerLossesContainer);
        ClearContainer(enemyLossesContainer);

        foreach (var unit in allRegisteredUnits)
        {
            if (unit == null || unit.data == null) continue;

            int lostCount = unit.initialStackSize - unit.stackSize;
            if (lostCount <= 0) continue;

            Transform targetContainer = (unit.teamId == 0) ? playerLossesContainer : enemyLossesContainer;
            CreateLossEntry(unit.data.idleSprite, lostCount, targetContainer);
        }
    }

    private void CreateLossEntry(Sprite unitSprite, int lostAmount, Transform container)
    {
        if (lossEntryPrefab == null || container == null) return;

        GameObject entry = Instantiate(lossEntryPrefab, container);

        // Пошук за конкретною назвою дочірніх об'єктів або першим підходящим
        Image icon = entry.transform.Find("Icon")?.GetComponent<Image>();
        if (icon == null) icon = entry.GetComponentInChildren<Image>();

        TMP_Text countText = entry.transform.Find("Text")?.GetComponent<TMP_Text>();
        if (countText == null) countText = entry.GetComponentInChildren<TMP_Text>();

        if (icon != null) icon.sprite = unitSprite;
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