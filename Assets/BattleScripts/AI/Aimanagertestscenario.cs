using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ЛР1: тестовий сценарій. Повісити на порожній GameObject у порожній сцені, натиснути Play.
/// Очікувані повідомлення: LogError у тесті 4 та LogWarning у тестах 5, 8 і 10 — це нормально.
/// </summary>
public class AIManagerTestScenario : MonoBehaviour
{
	private int passed, failed;

	private void Check(bool condition, string name)
	{
		if (condition) { passed++; Debug.Log($"PASS: {name}"); }
		else { failed++; Debug.LogError($"FAIL: {name}"); }
	}

	private IEnumerator Start()
	{
		Debug.Log("=== ТЕСТ 1: Singleton ===");
		var manager = AIManager.Instance;
		manager.ClearBattleHistory();
		Check(manager != null, "Instance створено");
		Check(manager == AIManager.Instance, "Instance завжди повертає один об'єкт");

		Debug.Log("=== ТЕСТ 2: Складність (UC-25) ===");
		manager.SetDifficulty(AIManager.AIDifficulty.Hard);
		Check(manager.GetDifficulty() == AIManager.AIDifficulty.Hard, "SetDifficulty/GetDifficulty");

		Debug.Log("=== ТЕСТ 3: Створення (UC-24) ===");
		var ai1 = manager.CreateAI("Zeta", new Vector3(1, 0, 0));
		manager.SetDifficulty(AIManager.AIDifficulty.Easy);
		var ai2 = manager.CreateAI("Alpha", new Vector3(2, 0, 0));
		Check(ai1 != null && ai2 != null, "Два ШІ створено");
		Check(manager.GetActiveAICount() == 2, "Лічильник = 2");
		Check(ai1.UnitName == "Zeta" && ai1.Difficulty == AIManager.AIDifficulty.Hard, "Initialize: ім'я та складність Zeta");

		Debug.Log("=== ТЕСТ 4: Дублікат імені (очікується LogError) ===");
		Check(manager.CreateAI("Zeta", Vector3.zero) == null, "Дублікат відхилено");
		Check(manager.GetActiveAICount() == 2, "Лічильник не змінився");

		Debug.Log("=== ТЕСТ 5: Пошук (UC-5) та сортування (UC-6) ===");
		Check(manager.GetAI("Zeta") == ai1, "GetAI повертає потрібний об'єкт");
		Check(manager.GetAI("Ghost") == null, "GetAI неіснуючого = null");
		Check(manager.GetAISortedByName()[0].UnitName == "Alpha", "Сортування за ім'ям");
		Check(manager.FindAIByDifficulty(AIManager.AIDifficulty.Hard).Count == 1, "Пошук за складністю");

		Debug.Log("=== ТЕСТ 6: Автономна поведінка AIUnit ===");
		Check(!ai1.IsAlive(), "Без BattleUnit юніт не вважається живим");
		bool turnCompleted = false;
		ai1.MakeAutonomousDecision(new List<BattleUnit>(), () => turnCompleted = true);
		Check(turnCompleted, "Callback завершення ходу викликано");

		Debug.Log("=== ТЕСТ 7: UC-22 / UC-23 ===");
		Check(ai1.EvaluateForceRatio(50, 100) == 0.5f, "Співвідношення 50/100 = 0.5");
		Check(ai1.EvaluateForceRatio(10, 0) == float.PositiveInfinity, "Ворога немає: нескінченність");
		Check(ai1.ShouldRetreat(10, 100), "Слабка армія відступає");
		Check(!ai1.ShouldRetreat(100, 50), "Сильна армія не відступає");
		Check(ai1.ShouldRetreat(70, 100) && !ai2.ShouldRetreat(70, 100), "Hard відступає раніше, ніж Easy");

		Debug.Log("=== ТЕСТ 8: UC-11 (Фасад) ===");
		bool done = false;
		Check(manager.ComputeTurn("Zeta", new List<BattleUnit>(), () => done = true), "ComputeTurn знайшов юніт");
		Check(done, "Хід завершено через callback");
		Check(!manager.ComputeTurn("Ghost", new List<BattleUnit>(), null), "Невідомий ШІ: false");

		Debug.Log("=== ТЕСТ 9: Збереження / завантаження (UC-3 / UC-4) ===");
		manager.SaveBattleResult("Victory", "Zeta");
		manager.SaveBattleResult("Defeat", "Alpha");
		string history = manager.LoadBattleHistory();
		Check(history.Contains("Zeta") && history.Contains("Victory"), "Історія містить перший запис");
		Check(history.Contains("Alpha") && history.Contains("Defeat"), "Історія містить другий запис");

		Debug.Log("=== ТЕСТ 10: Знищення (UC-24) ===");
		manager.DestroyAI("Zeta");
		Check(manager.GetActiveAICount() == 1, "Лічильник = 1 після знищення");
		yield return null; // Destroy відкладений до кінця кадру
		Check(ai1 == null, "GameObject справді знищено");
		manager.DestroyAI("Zeta"); // очікується Warning
		manager.LogAIStatus();

		Debug.Log($"=== ПІДСУМОК: пройдено {passed}, провалено {failed} ===");
	}
}