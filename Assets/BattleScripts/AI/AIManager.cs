using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Менеджер ШІ. Патерни: Одинак (Singleton) + Фасад (Facade).
/// ЛР1: UC-11, UC-24, UC-25, UC-3/UC-4 (історія боїв), UC-5, UC-6 (API збережено).
/// ЛР2: композиція AIManager *-- AIPlayer *-- AIUnit, пресети профілів AIManager *-- AIProfile.
/// ЛР3: стратегії гравців, пошук через фільтри, сортування через AIUnitSorter,
///      збереження/відновлення стану через AIStorage (JSON або текст).
/// Юніти тепер належать гравцям; методи ЛР1 працюють над юнітами всіх гравців,
/// а CreateAI створює юніт у гравця за замовчуванням ("Default").
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
    // Dictionary: унікальні імена гравців і швидкий пошук за ім'ям
    private readonly Dictionary<string, AIPlayer> players = new Dictionary<string, AIPlayer>();
    // Dictionary з ключем-enum: один спільний профіль на кожну складність
    private readonly Dictionary<AIDifficulty, AIProfile> profiles = new Dictionary<AIDifficulty, AIProfile>();

    private const string DefaultPlayerName = "Default";

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

    // ---------- ЛР2: профілі (менеджер їх створює й володіє ними) ----------
    public AIProfile GetProfile(AIDifficulty difficulty)
    {
        if (!profiles.TryGetValue(difficulty, out AIProfile profile))
        {
            profile = new AIProfile(difficulty);
            profiles[difficulty] = profile;
        }
        return profile;
    }

    // ---------- ЛР2: гравці (композиція) ----------
    public AIPlayer CreatePlayer(string playerName, AIDifficulty difficulty)
    {
        return CreatePlayer(playerName, difficulty, null);
    }

    // ЛР3: гравець зі своєю стратегією (null = ClosestEnemyStrategy)
    public AIPlayer CreatePlayer(string playerName, AIDifficulty difficulty, AIStrategy strategy)
    {
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogError("[-] Player name must not be empty!");
            return null;
        }

        if (players.ContainsKey(playerName))
        {
            Debug.LogError($"[-] Player '{playerName}' already exists!");
            return null;
        }

        AIPlayer player = new AIPlayer(playerName, GetProfile(difficulty), strategy);
        players[playerName] = player;

        Debug.Log($"[+] Player '{playerName}' created ({difficulty}, strategy {player.Strategy.Name})");
        return player;
    }

    public bool DestroyPlayer(string playerName)
    {
        if (!players.TryGetValue(playerName, out AIPlayer player))
        {
            Debug.LogWarning($"[!] Player '{playerName}' not found!");
            return false;
        }

        player.DestroyAll();          // ціле знищує свої частини
        players.Remove(playerName);
        return true;
    }

    public AIPlayer GetPlayer(string playerName)
    {
        if (players.TryGetValue(playerName, out AIPlayer player)) return player;

        Debug.LogWarning($"[!] Player '{playerName}' not found!");
        return null;
    }

    public int GetPlayerCount() => players.Count;

    public List<AIPlayer> GetAllPlayers() => new List<AIPlayer>(players.Values);

    private AIPlayer GetOrCreateDefaultPlayer()
    {
        if (!players.TryGetValue(DefaultPlayerName, out AIPlayer player))
        {
            player = CreatePlayer(DefaultPlayerName, AIDifficulty.Normal);
        }
        return player;
    }

    // ---------- Допоміжні: юніти всіх гравців ----------
    private IEnumerable<AIUnit> AllUnits()
    {
        return players.Values.SelectMany(p => p.GetUnits());
    }

    private AIUnit FindUnit(string unitName)
    {
        foreach (AIPlayer p in players.Values)
        {
            AIUnit unit = p.GetUnit(unitName);
            if (unit != null) return unit;
        }
        return null;
    }

    private AIPlayer FindOwner(string unitName)
    {
        foreach (AIPlayer p in players.Values)
        {
            if (p.GetUnit(unitName) != null) return p;
        }
        return null;
    }

    // ---------- UC-24: створення / знищення (API ЛР1) ----------
    public AIUnit CreateAI(string unitName, Vector3 startPosition)
    {
        if (string.IsNullOrEmpty(unitName))
        {
            Debug.LogError("[-] AI name must not be empty!");
            return null;
        }

        if (FindUnit(unitName) != null)
        {
            Debug.LogError($"[-] AI with name '{unitName}' already exists!");
            return null;
        }

        AIUnit newAI = GetOrCreateDefaultPlayer().CreateUnit(unitName, startPosition, currentDifficulty);
        if (newAI != null) totalCreated++;
        return newAI;
    }

    public void DestroyAI(string unitName)
    {
        AIPlayer owner = FindOwner(unitName);
        if (owner == null)
        {
            Debug.LogWarning($"[!] AI '{unitName}' not found!");
            return;
        }

        owner.RemoveUnit(unitName);   // реально знищується наприкінці кадру
    }

    // ---------- UC-5: пошук ----------
    public AIUnit GetAI(string unitName)
    {
        AIUnit unit = FindUnit(unitName);
        if (unit != null) return unit;

        Debug.LogWarning($"[!] AI '{unitName}' not found!");
        return null;
    }

    // ЛР3: різні фільтри (нащадки AIUnitFilter) через один метод
    public List<AIUnit> FindAI(AIUnitFilter filter)
    {
        if (filter == null) return new List<AIUnit>();
        return AllUnits().Where(u => filter.Matches(u)).ToList();
    }

    public List<AIUnit> FindAIByDifficulty(AIDifficulty difficulty)
    {
        return FindAI(new DifficultyFilter(difficulty));
    }

    public int GetActiveAICount() => players.Values.Sum(p => p.UnitCount);

    // Повертаємо копію, щоб зовнішній код не міг змінити внутрішні колекції
    public List<AIUnit> GetAllActiveAI() => AllUnits().ToList();

    // ---------- UC-6: сортування ----------
    // ЛР3: різні критерії (нащадки AIUnitSorter) через один метод; OrderBy стабільний
    public List<AIUnit> GetAISorted(AIUnitSorter sorter)
    {
        if (sorter == null) sorter = new ByNameSorter();
        return AllUnits().OrderBy(u => u, sorter).ToList();
    }

    public List<AIUnit> GetAISortedByName()
    {
        return GetAISorted(new ByNameSorter());
    }

    // ---------- UC-11: обчислити хід ШІ (фасад над AIUnit) ----------
    public bool ComputeTurn(string unitName, List<BattleUnit> allUnits, System.Action onTurnComplete)
    {
        AIUnit unit = FindUnit(unitName);
        if (unit == null)
        {
            Debug.LogWarning($"[!] ComputeTurn: AI '{unitName}' not found!");
            return false;
        }

        unit.MakeAutonomousDecision(allUnits, onTurnComplete);
        return true;
    }

    // ---------- ЛР3: збереження та відновлення стану ШІ ----------
    public AISaveData CreateSnapshot()
    {
        AISaveData data = new AISaveData();

        foreach (AIPlayer player in players.Values)
        {
            PlayerSaveData pd = new PlayerSaveData
            {
                name = player.Name,
                difficulty = (int)player.Profile.Difficulty,
                strategy = player.Strategy.Name
            };

            foreach (AIUnit unit in player.GetUnits())
            {
                Vector3 pos = unit.transform.position;
                pd.units.Add(new UnitSaveData
                {
                    name = unit.UnitName,
                    x = pos.x,
                    y = pos.y,
                    z = pos.z,
                    difficulty = (int)unit.Difficulty
                });
            }

            foreach (AIDecisionRecord record in player.GetHistory())
            {
                pd.history.Add(new RecordSaveData
                {
                    turn = record.Turn,
                    unitName = record.UnitName,
                    action = record.Action,
                    timestamp = record.Timestamp
                });
            }

            data.players.Add(pd);
        }
        return data;
    }

    // Замінює поточних гравців тими, що в збереженні
    public void RestoreSnapshot(AISaveData data)
    {
        foreach (string playerName in players.Keys.ToList())
        {
            DestroyPlayer(playerName);
        }

        foreach (PlayerSaveData pd in data.players)
        {
            AIPlayer player = CreatePlayer(pd.name, (AIDifficulty)pd.difficulty, AIStrategy.FromName(pd.strategy));
            if (player == null) continue;

            foreach (UnitSaveData ud in pd.units)
            {
                player.CreateUnit(ud.name, new Vector3(ud.x, ud.y, ud.z), (AIDifficulty)ud.difficulty);
            }

            foreach (RecordSaveData rd in pd.history)
            {
                player.RecordDecision(rd.turn, rd.unitName, rd.action, rd.timestamp);
            }
        }
    }

    // storage: JsonAIStorage або TextAIStorage (поліморфізм); при помилці поточний стан не змінюється
    public bool SaveState(AIStorage storage, string path)
    {
        try
        {
            storage.Save(CreateSnapshot(), path);
            Debug.Log($"[SAVE] State saved ({storage.Name}): {path}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[-] SaveState failed ({storage.Name}): {e.Message}");
            return false;
        }
    }

    public bool LoadState(AIStorage storage, string path)
    {
        AISaveData data;
        try
        {
            data = storage.Load(path);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[-] LoadState failed ({storage.Name}): {e.Message}");
            return false;
        }

        if (data == null)
        {
            Debug.LogWarning($"[!] LoadState: file not found: {path}");
            return false;
        }

        RestoreSnapshot(data);
        Debug.Log($"[LOAD] State restored ({storage.Name}): {data.players.Count} players");
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
        Debug.Log($"[STATUS] AIManager: {players.Count} players, {GetActiveAICount()} active AI, created via manager: {totalCreated}");
    }
}