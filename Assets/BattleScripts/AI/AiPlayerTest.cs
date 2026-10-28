using System.Collections;
using UnityEngine;

/// <summary>
/// ЛР2: тестовий сценарій композиції та агрегації.
/// Повісити на порожній GameObject у ПОРОЖНІЙ сцені (окремо від AIManagerTestScenario,
/// бо менеджер — Одинак і ділить стан між тестами) і натиснути Play.
/// Очікувані повідомлення: LogError у тестах 1 і 2, LogWarning у тесті 6 — це нормально.
/// </summary>
public class AIPlayerTestScenario : MonoBehaviour
{
    private int passed, failed;

    private void Check(bool condition, string name)
    {
        if (condition) { passed++; Debug.Log($"PASS: {name}"); }
        else { failed++; Debug.LogError($"FAIL: {name}"); }
    }

    private IEnumerator Start()
    {
        var manager = AIManager.Instance;

        Debug.Log("=== ТЕСТ 1: Гравці (AIManager *-- AIPlayer), очікується LogError ===");
        AIPlayer red = manager.CreatePlayer("Red", AIManager.AIDifficulty.Hard);
        AIPlayer blue = manager.CreatePlayer("Blue", AIManager.AIDifficulty.Easy);
        Check(red != null && blue != null, "Два гравці створено");
        Check(manager.GetPlayerCount() == 2, "У менеджері 2 гравці");
        Check(manager.CreatePlayer("Red", AIManager.AIDifficulty.Normal) == null, "Дублікат імені гравця відхилено");
        Check(manager.GetPlayer("Red") == red, "GetPlayer повертає того самого гравця");

        Debug.Log("=== ТЕСТ 2: Юніти як частини гравця (AIPlayer *-- AIUnit), очікується LogError ===");
        AIUnit knight = red.CreateUnit("Knight", new Vector3(0, 0, 0));
        red.CreateUnit("Archer", new Vector3(1, 0, 0));
        red.CreateUnit("Mage", new Vector3(2, 0, 0));
        AIUnit scout = blue.CreateUnit("Scout", new Vector3(5, 0, 0));
        Check(red.UnitCount == 3, "У Red 3 юніти");
        Check(blue.UnitCount == 1, "У Blue 1 юніт");
        Check(red.CreateUnit("Knight", Vector3.zero) == null, "Дублікат імені в межах гравця відхилено");
        Check(blue.CreateUnit("Knight", Vector3.zero) != null, "Те саме ім'я в іншого гравця дозволене");
        Check(knight.Difficulty == AIManager.AIDifficulty.Hard && scout.Difficulty == AIManager.AIDifficulty.Easy,
              "Юніти отримують складність профілю свого гравця");

        Debug.Log("=== ТЕСТ 3: Журнал рішень (AIPlayer *-- AIDecisionRecord) ===");
        red.RecordDecision(1, "Knight", "Attack");
        red.RecordDecision(1, "Archer", "Shoot");
        red.RecordDecision(2, "Knight", "Retreat");
        Check(red.HistoryCount == 3, "Журнал Red містить 3 записи");
        Check(red.GetHistory()[2].Action == "Retreat", "Порядок записів збережено");
        red.GetHistory().Clear();
        Check(red.HistoryCount == 3, "GetHistory повертає копію: зовнішня зміна не впливає");
        Check(blue.HistoryCount == 0, "Журнал Blue окремий і порожній");

        Debug.Log("=== ТЕСТ 4: Агрегація (AIPlayer o-- AIProfile) ===");
        AIPlayer red2 = manager.CreatePlayer("Red2", AIManager.AIDifficulty.Hard);
        Check(ReferenceEquals(red.Profile, red2.Profile), "Red та Red2 ділять один AIProfile");
        Check(!ReferenceEquals(red.Profile, blue.Profile), "Різна складність: різні профілі");
        Check(red.Profile.RetreatThreshold == 0.75f, "Профіль Hard: поріг 0.75");

        Debug.Log("=== ТЕСТ 5: Знищення цілого => знищення частин ===");
        Check(manager.DestroyPlayer("Red"), "DestroyPlayer(Red) = true");
        Check(manager.GetPlayerCount() == 2, "Red видалено з менеджера (лишились Blue, Red2)");
        Check(red.UnitCount == 0 && red.HistoryCount == 0, "Частини Red (юніти, журнал) очищені");
        yield return null; // Destroy відкладений до кінця кадру
        Check(knight == null, "GameObject юніта Knight знищено разом із гравцем");
        Check(blue.UnitCount == 2, "Юніти Blue не постраждали");
        Check(red2.Profile.RetreatThreshold == 0.75f
              && ReferenceEquals(red2.Profile, manager.GetProfile(AIManager.AIDifficulty.Hard)),
              "Агрегація: профіль пережив гравця Red і далі використовується Red2");

        Debug.Log("=== ТЕСТ 6: Видалення частини та помилкові виклики (очікується LogWarning) ===");
        Check(blue.RemoveUnit("Scout"), "RemoveUnit існуючого = true");
        Check(blue.UnitCount == 1, "У Blue лишився 1 юніт");
        Check(!blue.RemoveUnit("Ghost"), "RemoveUnit неіснуючого = false");
        yield return null;
        Check(scout == null, "Юніт Scout знищено");
        Check(!manager.DestroyPlayer("Ghost"), "DestroyPlayer неіснуючого = false");

        // Прибирання
        manager.DestroyPlayer("Blue");
        manager.DestroyPlayer("Red2");
        manager.LogAIStatus();

        Debug.Log($"=== ПІДСУМОК: пройдено {passed}, провалено {failed} ===");
    }
}