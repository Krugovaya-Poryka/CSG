using UnityEngine;
using TMPro;

public class BattleLogUI : MonoBehaviour
{
    public static BattleLogUI Instance;

    public TMP_Text statusText; // Посилання на текст у нижній панелі

    private void Awake()
    {
        Instance = this;
    }

    // Відображення чий хід
    public void LogTurn(BattleUnit unit)
    {
        if (statusText == null || unit == null || unit.data == null) return;

        string teamName = unit.teamId == 0 ? "Гравця" : "Ворога";
        string colorHex = unit.teamId == 0 ? "#55FFFF" : "#FF5555";

        statusText.text = $"Хід юніта: <color={colorHex}>{unit.data.unitName}</color> ({teamName})";
    }

    // Відображення удару та шкоди
    public void LogAttack(BattleUnit attacker, BattleUnit target, int damage, bool isRetaliation = false)
    {
        if (statusText == null || attacker == null || target == null) return;

        string attackerColor = attacker.teamId == 0 ? "#55FFFF" : "#FF5555";
        string targetColor = target.teamId == 0 ? "#55FFFF" : "#FF5555";
        string actionText = isRetaliation ? "контратакує" : "завдає";

        statusText.text = $"<color={attackerColor}>{attacker.data.unitName}</color> {actionText} <color={targetColor}>{target.data.unitName}</color> — <color=#FFFF55><b>{damage}</b></color> шкоди!";
    }
}