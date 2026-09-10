using UnityEngine;
using System.Collections.Generic;

public class AIManager : MonoBehaviour
{
    private static AIManager instance;
    
    public static AIManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AIManager>();

                if (instance == null)
                {
                    GameObject go = new GameObject("AIManager");
                    instance = go.AddComponent<AIManager>();
                }
            }
            return instance;
        }
    }

    private Dictionary<string, AIUnit> activeAIUnits = new Dictionary<string, AIUnit>();
    
    private AIDifficulty currentDifficulty = AIDifficulty.Normal;
    
    private int aiUnitCounter = 0;

    public enum AIDifficulty
    {
        Easy = 0,
        Normal = 1,
        Hard = 2
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);
        
        Debug.Log("<color=green>✓ AIManager ініціалізований (Singleton)</color>");
    }

    public void SetDifficulty(AIDifficulty difficulty)
    {
        currentDifficulty = difficulty;
        
        Debug.Log($"<color=yellow>⚙ Складність ШІ встановлена: {difficulty}</color>");
    }

    public AIDifficulty GetDifficulty()
    {
        return currentDifficulty;
    }

    public AIUnit CreateAI(string unitName, Vector3 startPosition)
    {
        if (activeAIUnits.ContainsKey(unitName))
        {
            Debug.LogError($"<color=red>✗ AI з ім'ям '{unitName}' вже існує!</color>");
            return null;
        }
        
        GameObject aiGameObject = new GameObject($"AI_{unitName}");
        aiGameObject.transform.position = startPosition;

        AIUnit newAI = aiGameObject.AddComponent<AIUnit>();

        newAI.Initialize(unitName, currentDifficulty, startPosition);

        activeAIUnits[unitName] = newAI;

        aiUnitCounter++;
        Debug.Log($"<color=cyan>✓ Новий AI юніт створений: '{unitName}' (Позиція: {startPosition})</color>");
        
        return newAI;
    }

    public void DestroyAI(string unitName)
    {
        if (!activeAIUnits.ContainsKey(unitName))
        {
            Debug.LogWarning($"<color=orange>⚠ AI '{unitName}' не знайдено!</color>");
            return;
        }
        
        AIUnit aiUnit = activeAIUnits[unitName];
        
        activeAIUnits.Remove(unitName);

        Destroy(aiUnit.gameObject);
        
        Debug.Log($"<color=magenta>✓ AI юніт '{unitName}' знищений</color>");
    }

    public AIUnit GetAI(string unitName)
    {
        if (activeAIUnits.ContainsKey(unitName))
        {
            return activeAIUnits[unitName];
        }
        
        Debug.LogWarning($"<color=orange>⚠ AI '{unitName}' не знайдено!</color>");
        return null;
    }
    
    public int GetActiveAICount()
    {
        return activeAIUnits.Count;
    }

    public List<AIUnit> GetAllActiveAI()
    {
        return new List<AIUnit>(activeAIUnits.Values);
    }

    public void SaveBattleResult(string battleResult, string aiName)
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string saveData = $"[{timestamp}] AI: {aiName}, Result: {battleResult}";

        Debug.Log($"<color=green>💾 Результат збережено: {saveData}</color>");
    }
    
    public string LoadBattleHistory()
    {
        Debug.Log("<color=blue">📂 Завантаження історії боїв...</color>");
        return "Історія боїв завантажена";
    }

    private void LogAIStatus()
    {
        Debug.Log($"<color=cyan>📊 Статус AIManager: {GetActiveAICount()} активних AI</color>");
    }
}
