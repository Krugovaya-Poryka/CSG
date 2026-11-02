using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// ЛР3: тестовий сценарій наслідування та поліморфізму.
/// Повісити на порожній GameObject у ПОРОЖНІЙ сцені (окремо від тестів ЛР1 і ЛР2,
/// бо менеджер це Одинак і ділить стан) і натиснути Play.
/// Очікувані повідомлення в тесті 8: 2 LogError (пошкоджені файли) і 1 LogWarning
/// (відсутній файл) — це нормально.
/// </summary>
public class AIPolymorphismTestScenario : MonoBehaviour
{
    private int passed, failed;

    private void Check(bool condition, string name)
    {
        if (condition) { passed++; Debug.Log($"PASS: {name}"); }
        else { failed++; Debug.LogError($"FAIL: {name}"); }
    }

    private static string Names(List<AIUnit> list)
    {
        List<string> parts = new List<string>();
        foreach (AIUnit u in list) parts.Add(u.UnitName);
        return string.Join(",", parts);
    }

    private static string PathFor(string file)
    {
        return Path.Combine(Application.persistentDataPath, file);
    }

    private IEnumerator Start()
    {
        var manager = AIManager.Instance;

        // ---------------------------------------------------------------
        Debug.Log("=== ТЕСТ 1: Поліморфізм стратегій: один виклик ChooseTarget, різні цілі ===");
        List<TargetCandidate> candidates = new List<TargetCandidate>
        {
            new TargetCandidate(null, "Archer", 5, 10, true),
            new TargetCandidate(null, "Knight", 2, 50, false),
            new TargetCandidate(null, "Peasant", 4, 3, false)
        };

        // Масив базового типу, всередині різні нащадки
        AIStrategy[] strategies =
        {
            new ClosestEnemyStrategy(), new AggressiveStrategy(),
            new DefensiveStrategy(), new FocusFireStrategy()
        };
        string[] expectedTargets = { "Knight", "Peasant", "Knight", "Archer" };

        for (int i = 0; i < strategies.Length; i++)
        {
            Check(strategies[i].ChooseTarget(candidates).Name == expectedTargets[i],
                  $"{strategies[i].Name}: ціль {expectedTargets[i]}");
        }

        bool allNull = true;
        foreach (AIStrategy s in strategies)
        {
            if (s.ChooseTarget(new List<TargetCandidate>()) != null) allNull = false;
        }
        Check(allNull, "Порожній список кандидатів: усі стратегії повертають null");

        List<TargetCandidate> noRanged = new List<TargetCandidate>
        {
            new TargetCandidate(null, "Knight", 2, 50, false),
            new TargetCandidate(null, "Peasant", 4, 3, false)
        };
        Check(new FocusFireStrategy().ChooseTarget(noRanged).Name == "Knight",
              "FocusFire без стрільців: найближчий (успадкована логіка)");

        // ---------------------------------------------------------------
        Debug.Log("=== ТЕСТ 2: Перевизначення ShouldRetreat (базовий поріг 0.5) ===");
        const float baseThreshold = 0.5f;
        Check(strategies[0].ShouldRetreat(0.4f, baseThreshold), "Closest: 0.4 < 0.5, відступає");
        Check(!strategies[1].ShouldRetreat(0.4f, baseThreshold), "Aggressive: поріг 0.25, не відступає");
        Check(strategies[2].ShouldRetreat(0.4f, baseThreshold), "Defensive: поріг 0.75, відступає");
        Check(strategies[3].ShouldRetreat(0.4f, baseThreshold), "FocusFire успадкував поведінку Closest");
        Check(strategies[2].ShouldRetreat(0.6f, baseThreshold)
              && !strategies[0].ShouldRetreat(0.6f, baseThreshold)
              && !strategies[1].ShouldRetreat(0.6f, baseThreshold),
              "Те саме співвідношення 0.6: відступає лише Defensive");

        // ---------------------------------------------------------------
        Debug.Log("=== ТЕСТ 3: Структура успадкування ===");
        Check(strategies[2] is ClosestEnemyStrategy && strategies[3] is ClosestEnemyStrategy,
              "Defensive та FocusFire є нащадками ClosestEnemyStrategy");
        Check(!(strategies[1] is ClosestEnemyStrategy), "Aggressive успадковує напряму AIStrategy");
        HashSet<string> names = new HashSet<string>();
        foreach (AIStrategy s in strategies) names.Add(s.Name);
        Check(names.Count == 4, "Кожен нащадок має власну назву");

        // ---------------------------------------------------------------
        Debug.Log("=== ТЕСТ 4: Юніти діють за стратегією свого гравця ===");
        AIPlayer red = manager.CreatePlayer("Red", AIManager.AIDifficulty.Hard, new AggressiveStrategy());
        AIPlayer blue = manager.CreatePlayer("Blue", AIManager.AIDifficulty.Easy, new DefensiveStrategy());
        AIUnit knight = red.CreateUnit("Knight", new Vector3(0, 0, 0));
        red.CreateUnit("Archer", new Vector3(1, 0, 0));
        red.CreateUnit("Mage", new Vector3(2, 0, 0));
        AIUnit scout = blue.CreateUnit("Scout", new Vector3(5, 0, 0));
        blue.CreateUnit("Guard", new Vector3(6, 0, 0));

        Check(red.Strategy.Name == "Aggressive" && blue.Strategy.Name == "Defensive",
              "Гравці мають задані стратегії");
        Check(ReferenceEquals(knight.Strategy, red.Strategy),
              "Юніт отримує той самий об'єкт стратегії, що й гравець");
        Check(!knight.ShouldRetreat(40, 100),
              "Hard + Aggressive (поріг 0.375): співвідношення 0.4 не відступає");
        Check(scout.ShouldRetreat(30, 100),
              "Easy + Defensive (поріг 0.375): співвідношення 0.3 відступає");
        Check(knight.ChooseTarget(candidates).Name == "Peasant", "Юніт Red (Aggressive) обирає найслабшого");
        Check(scout.ChooseTarget(candidates).Name == "Knight", "Юніт Blue (Defensive) обирає найближчого");

        // ---------------------------------------------------------------
        Debug.Log("=== ТЕСТ 5: Поліморфні фільтри пошуку ===");
        AIUnitFilter[] filters =
        {
            new DifficultyFilter(AIManager.AIDifficulty.Hard),
            new NameContainsFilter("kn"),
            new StrategyFilter("Defensive")
        };
        int[] expectedCounts = { 3, 1, 2 };
        for (int i = 0; i < filters.Length; i++)
        {
            Check(manager.FindAI(filters[i]).Count == expectedCounts[i],
                  $"Фільтр «{filters[i].Description}»: {expectedCounts[i]}");
        }
        Check(manager.FindAI(new StrategyFilter("FocusFire")).Count == 0, "Фільтр без збігів: порожній список");
        Check(manager.FindAI(null).Count == 0, "Фільтр null: порожній список");

        // ---------------------------------------------------------------
        Debug.Log("=== ТЕСТ 6: Поліморфне сортування ===");
        AIUnitSorter[] sorters = { new ByNameSorter(), new ByDifficultySorter(), new ByStrategySorter() };
        string[] expectedOrders =
        {
            "Archer,Guard,Knight,Mage,Scout",
            "Guard,Scout,Archer,Knight,Mage",
            "Archer,Knight,Mage,Guard,Scout"
        };
        for (int i = 0; i < sorters.Length; i++)
        {
            Check(Names(manager.GetAISorted(sorters[i])) == expectedOrders[i],
                  $"Сортування {sorters[i].Description}: {expectedOrders[i]}");
        }
        Check(Names(manager.GetAISortedByName()) == expectedOrders[0], "GetAISortedByName (API ЛР1) працює як і раніше");

        // ---------------------------------------------------------------
        Debug.Log("=== ТЕСТ 7: Поліморфне збереження та відновлення стану ===");
        red.RecordDecision(1, "Knight", "Attack");
        red.RecordDecision(2, "Archer", "Shoot");
        blue.RecordDecision(1, "Guard", "Hold");
        string firstTimestamp = red.GetHistory()[0].Timestamp;

        AIStorage[] storages = { new JsonAIStorage(), new TextAIStorage() };
        foreach (AIStorage st in storages)
        {
            Check(manager.SaveState(st, PathFor("ai_state_test" + st.FileExtension)),
                  $"{st.Name}: стан збережено");
        }

        foreach (AIStorage st in storages)
        {
            manager.DestroyPlayer("Red");
            manager.DestroyPlayer("Blue");
            yield return null; // Destroy відкладений до кінця кадру
            Check(manager.GetPlayerCount() == 0 && manager.GetActiveAICount() == 0,
                  $"{st.Name}: стан очищено перед відновленням");

            Check(manager.LoadState(st, PathFor("ai_state_test" + st.FileExtension)),
                  $"{st.Name}: LoadState = true");

            AIPlayer r = manager.GetPlayer("Red");
            AIPlayer b = manager.GetPlayer("Blue");
            if (r == null || b == null)
            {
                Check(false, $"{st.Name}: гравці Red та Blue відновлені");
                continue;
            }

            Check(manager.GetPlayerCount() == 2, $"{st.Name}: відновлено 2 гравці");
            Check(r.UnitCount == 3 && b.UnitCount == 2, $"{st.Name}: юніти відновлені (3 та 2)");
            Check(r.Strategy.Name == "Aggressive" && b.Strategy.Name == "Defensive"
                  && r.Profile.Difficulty == AIManager.AIDifficulty.Hard,
                  $"{st.Name}: стратегії та складність відновлені");
            Check(r.HistoryCount == 2 && b.HistoryCount == 1 && r.GetHistory()[0].Timestamp == firstTimestamp,
                  $"{st.Name}: журнал відновлений разом із часом запису");
            Check(r.GetUnit("Archer").transform.position.x == 1f, $"{st.Name}: позиція юніта збережена");
            Check(r.GetUnit("Knight").Strategy.Name == "Aggressive",
                  $"{st.Name}: відновлений юніт діє за стратегією гравця");
        }

        Debug.Log("=== ТЕСТ 8: Пошкоджені та відсутні файли (очікуються LogError та LogWarning) ===");
        string badJson = PathFor("ai_bad.json");
        string badText = PathFor("ai_bad.txt");
        File.WriteAllText(badJson, "{ це не json");
        File.WriteAllText(badText, "це не збереження\n");
        Check(!manager.LoadState(new JsonAIStorage(), badJson), "Пошкоджений JSON: LoadState = false");
        Check(!manager.LoadState(new TextAIStorage(), badText), "Пошкоджений текст: LoadState = false");
        Check(manager.GetPlayerCount() == 2, "Після невдалого завантаження стан не змінився");
        Check(!manager.LoadState(new JsonAIStorage(), PathFor("ai_missing.json")), "Файлу немає: LoadState = false");

        // Прибирання
        manager.DestroyPlayer("Red");
        manager.DestroyPlayer("Blue");
        foreach (string f in new[] { "ai_state_test.json", "ai_state_test.txt", "ai_bad.json", "ai_bad.txt" })
        {
            if (File.Exists(PathFor(f))) File.Delete(PathFor(f));
        }
        manager.LogAIStatus();

        Debug.Log($"=== ПІДСУМОК: пройдено {passed}, провалено {failed} ===");
    }
}
