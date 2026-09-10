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
        
        Debug.Log("<color=green>[+] AIManager initialized (Singleton)</color>");
    }

    public void SetDifficulty(AIDifficulty difficulty)
    {
        currentDifficulty = difficulty;
        Debug.Log($"<color=yellow>[CONFIG] AI Difficulty set to: {difficulty}</color>");
    }

    public AIDifficulty GetDifficulty()
    {
        return currentDifficulty;
    }

    public AIUnit CreateAI(string unitName, Vector3 startPosition)
    {
        if (activeAIUnits.ContainsKey(unitName))
        {
            Debug.LogError($"<color=red>[-] AI with name '{unitName}' already exists!</color>");
            return null;
        }
        
        GameObject aiGameObject = new GameObject($"AI_{unitName}");
        aiGameObject.transform.position = startPosition;

        AIUnit newAI = aiGameObject.AddComponent<AIUnit>();
        newAI.Initialize(unitName, currentDifficulty, startPosition);

        activeAIUnits[unitName] = newAI;
        aiUnitCounter++;
        
        Debug.Log($"<color=cyan>[+] New AI unit created: '{unitName}' at position: {startPosition}</color>");
        return newAI;
    }

    public void DestroyAI(string unitName)
    {
        if (!activeAIUnits.ContainsKey(unitName))
        {
            Debug.LogWarning($"<color=orange>[!] AI '{unitName}' not found!</color>");
            return;
        }
        
        AIUnit aiUnit = activeAIUnits[unitName];
        activeAIUnits.Remove(unitName);

        Destroy(aiUnit.gameObject);
        Debug.Log($"<color=magenta>[-] AI unit '{unitName}' destroyed</color>");
    }

    public AIUnit GetAI(string unitName)
    {
        if (activeAIUnits.ContainsKey(unitName))
        {
            return activeAIUnits[unitName];
        }
        
        Debug.LogWarning($"<color=orange>[!] AI '{unitName}' not found!</color>");
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

        Debug.Log($"<color=green>[SAVE] Result saved: {saveData}</color>");
    }
    
    public string LoadBattleHistory()
    {
        Debug.Log("<color=blue>[LOAD] Loading battle history...</color>");
        return "History loaded";
    }

    private void LogAIStatus()
    {
        Debug.Log($"<color=cyan>[STATUS] AIManager: {GetActiveAICount()} active AI</color>");
    }
}
