using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// ЛР1: Менеджер ШІ. Патерни: Одинак (Singleton) + Фасад (Facade).
/// Реалізує прецеденти: UC-11, UC-24, UC-25, UC-3/UC-4 (історія боїв), UC-5, UC-6.
/// </summary>
public class AIManager : MonoBehaviour
{
    // ---------- Singleton ----------
    private static AIManager instance;

    public static AIManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<AIManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("AIManager");
                    instance = go.AddComponent<AIManager>();
                }
            }
            return instance;
        }
    }

    public enum AIDifficulty
    {
        Easy = 0,
        Normal = 1,
        Hard = 2
    }

    // ---------- Поля ----------
    private readonly Dictionary<string, AIUnit> activeAIUnits = new Dictionary<string, AIUnit>();
    private AIDifficulty currentDifficulty = AIDifficulty.Normal;
    private int totalCreated = 0;

    private string SavePath => Path.Combine(Application.persistentDataPath, "battle_history.txt");

    public int TotalCreated => totalCreated;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[+] AIManager initialized (Singleton)");
    }

    // ---------- UC-25: складність ----------
    public void SetDifficulty(AIDifficulty difficulty)
    {
        currentDifficulty = difficulty;
        Debug.Log($"[CONFIG] AI Difficulty set to: {difficulty}");
    }

    public AIDifficulty GetDifficulty() => currentDifficulty;

    // ---------- UC-24: створення / знищення ----------
    public AIUnit CreateAI(string unitName, Vector3 startPosition)
    {
        if (string.IsNullOrEmpty(unitName))
        {
            Debug.LogError("[-] AI name must not be empty!");
            return null;
        }

        if (activeAIUnits.ContainsKey(unitName))
        {
            Debug.LogError($"[-] AI with name '{unitName}' already exists!");
            return null;
        }

        GameObject aiGameObject = new GameObject($"AI_{unitName}");
        aiGameObject.transform.position = startPosition;

        AIUnit newAI = aiGameObject.AddComponent<AIUnit>();
        newAI.Initialize(unitName, currentDifficulty);

        activeAIUnits[unitName] = newAI;
        totalCreated++;

        Debug.Log($"[+] New AI unit created: '{unitName}' ({currentDifficulty}) at {startPosition}");
        return newAI;
    }

    public void DestroyAI(string unitName)
    {
        if (!activeAIUnits.TryGetValue(unitName, out AIUnit aiUnit))
        {
            Debug.LogWarning($"[!] AI '{unitName}' not found!");
            return;
        }

        activeAIUnits.Remove(unitName);
        Destroy(aiUnit.gameObject); // реально знищується наприкінці кадру
        Debug.Log($"[-] AI unit '{unitName}' destroyed");
    }

    // ---------- UC-5: пошук ----------
    public AIUnit GetAI(string unitName)
    {
        if (activeAIUnits.TryGetValue(unitName, out AIUnit unit)) return unit;

        Debug.LogWarning($"[!] AI '{unitName}' not found!");
        return null;
    }

    public List<AIUnit> FindAIByDifficulty(AIDifficulty difficulty)
    {
        return activeAIUnits.Values.Where(u => u.Difficulty == difficulty).ToList();
    }

    public int GetActiveAICount() => activeAIUnits.Count;

    // Повертаємо копію, щоб зовнішній код не міг змінити внутрішню колекцію
    public List<AIUnit> GetAllActiveAI() => new List<AIUnit>(activeAIUnits.Values);

    // ---------- UC-6: сортування ----------
    public List<AIUnit> GetAISortedByName()
    {
        return activeAIUnits.Values.OrderBy(u => u.UnitName).ToList();
    }

    // ---------- UC-11: обчислити хід ШІ (фасад над AIUnit) ----------
    public bool ComputeTurn(string unitName, List<BattleUnit> allUnits, System.Action onTurnComplete)
    {
        if (!activeAIUnits.TryGetValue(unitName, out AIUnit unit))
        {
            Debug.LogWarning($"[!] ComputeTurn: AI '{unitName}' not found!");
            return false;
        }

        unit.MakeAutonomousDecision(allUnits, onTurnComplete);
        return true;
    }

    // ---------- UC-3 / UC-4: збереження та завантаження історії боїв ----------
    public void SaveBattleResult(string battleResult, string aiName)
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string line = $"[{timestamp}] AI: {aiName}, Result: {battleResult}";

        File.AppendAllText(SavePath, line + "\n");
        Debug.Log($"[SAVE] {line}");
    }

    public string LoadBattleHistory()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("[LOAD] No history file yet");
            return string.Empty;
        }

        Debug.Log("[LOAD] Loading battle history...");
        return File.ReadAllText(SavePath);
    }

    public void ClearBattleHistory()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
    }

    public void LogAIStatus()
    {
        Debug.Log($"[STATUS] AIManager: {GetActiveAICount()} active AI, created total: {totalCreated}");
    }
}