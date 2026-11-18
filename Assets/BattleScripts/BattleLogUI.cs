using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleLogUI : MonoBehaviour
{
    public static BattleLogUI Instance;

    [Header("Нижня панель (Попередній перегляд)")]
    [SerializeField] private TMP_Text previewText; // Один рядок у нижній панелі

    [Header("Спливаюче вікно історії")]
    [SerializeField] private GameObject historyModal; // Вся модальна панель
    [SerializeField] private TMP_Text fullHistoryText; // Текст всередині ScrollView
    [SerializeField] private ScrollRect scrollRect;   // Скрол-панель
    [SerializeField] private int maxHistoryLines = 100;

    private List<string> logHistory = new List<string>();

    private void Awake()
    {
        Instance = this;

        if (historyModal != null)
        {
            historyModal.SetActive(false);
        }
    }

    public void ToggleHistoryModal()
    {
        if (historyModal == null) return;

        bool isOpening = !historyModal.activeSelf;
        historyModal.SetActive(isOpening);

        if (isOpening)
        {
            StartCoroutine(OpenAndScrollRoutine());
        }
    }

    private System.Collections.IEnumerator OpenAndScrollRoutine()
    {
        yield return null;

        ScrollToBottom();
    }

    public void CloseHistoryModal()
    {
        if (historyModal != null)
        {
            historyModal.SetActive(false);
        }
    }

    public void AddLog(string message)
    {
        if (string.IsNullOrEmpty(message)) return;

        // 1. Оновлюємо однорядковий прев'ю-текст у нижній панелі
        if (previewText != null)
        {
            previewText.text = message;
        }

        // 2. Додаємо в загальну історію
        logHistory.Add(message);
        if (logHistory.Count > maxHistoryLines)
        {
            logHistory.RemoveAt(0);
        }

        // 3. Оновлюємо повний текст у модальному вікні
        if (fullHistoryText != null)
        {
            fullHistoryText.text = string.Join("\n", logHistory);
        }

        if (historyModal != null && historyModal.activeSelf)
        {
            ScrollToBottom();
        }
    }

    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    public void LogTurn(BattleUnit unit)
    {
        if (unit == null || unit.data == null) return;

        string teamName = unit.teamId == 0 ? "Гравця" : "Ворога";
        string colorHex = unit.teamId == 0 ? "#55FFFF" : "#FF5555";

        AddLog($"Хід юніта: <color={colorHex}>{unit.data.unitName}</color> ({teamName})");
    }

    public void LogAttack(BattleUnit attacker, BattleUnit target, int damage, bool isRetaliation = false)
    {
        if (attacker == null || target == null) return;

        string attackerColor = attacker.teamId == 0 ? "#55FFFF" : "#FF5555";
        string targetColor = target.teamId == 0 ? "#55FFFF" : "#FF5555";
        string actionText = isRetaliation ? "контратакує на" : "завдає";

        AddLog($"<color={attackerColor}>{attacker.data.unitName}</color> {actionText} <color={targetColor}>{target.data.unitName}</color> <color=#FFFF55><b>{damage}</b></color> шкоди!");
    }

    public void LogCustomMessage(string message)
    {
        AddLog(message);
    }
}