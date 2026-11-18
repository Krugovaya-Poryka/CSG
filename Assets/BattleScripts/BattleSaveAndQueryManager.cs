using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine;
using System.IO;

public class BattleSaveAndQueryManager : MonoBehaviour
{
    public TurnManager turnManager;
    public HexGridManager gridManager;
    public BattleInputHandler inputHandler;

    private void Awake()
    {
        if (turnManager == null) turnManager = FindAnyObjectByType<TurnManager>();
        if (gridManager == null) gridManager = FindAnyObjectByType<HexGridManager>();
        if (inputHandler == null) inputHandler = FindAnyObjectByType<BattleInputHandler>();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.f5Key.wasPressedThisFrame)
        {
            SaveGameToJson();
        }

        if (Keyboard.current.f9Key.wasPressedThisFrame)
        {
            LoadGameFromJson();
        }
    }

    public BattleUnit FindStrongestUnit(int teamId)
    {
        return turnManager.allUnits
            .Where(u => u != null && u.teamId == teamId && u.stackSize > 0)
            .OrderByDescending(u => u.stackSize * u.currentHealth)
            .FirstOrDefault();
    }

    public List<BattleUnit> SearchUnitsByName(string keyword)
    {
        return turnManager.allUnits
            .Where(u => u != null && u.data != null && u.data.unitName.Contains(keyword))
            .ToList();
    }

    public List<BattleUnit> GetUnitsSortedByHealthAscending()
    {
        return turnManager.allUnits
            .Where(u => u != null && u.stackSize > 0)
            .OrderBy(u => u.currentHealth)
            .ToList();
    }

    [System.Serializable]
    public class UnitSaveData
    {
        public string unitName;
        public int stackSize;
        public int currentHealth;
        public int teamId;
        public int x, y;
    }

    [System.Serializable]
    public class BattleStateContainer
    {
        public string heroName;
        public string activeUnitName;
        public int activeUnitTeamId = -1;
        public int activeUnitX;
        public int activeUnitY;
        public List<UnitSaveData> units = new List<UnitSaveData>();
    }

    public void SaveGameToJson()
    {
        if (turnManager == null)
        {
            Debug.LogWarning("[Збереження] TurnManager не знайдено!");
            return;
        }

        BattleUnit current = turnManager.activeUnit;

        BattleStateContainer state = new BattleStateContainer
        {
            heroName = turnManager.playerHeroName,
            activeUnitName = (current != null && current.data != null) ? current.data.unitName : string.Empty,
            activeUnitTeamId = current != null ? current.teamId : -1,
            activeUnitX = current != null ? current.hexCoords.x : 0,
            activeUnitY = current != null ? current.hexCoords.y : 0
        };

        // Беремо всіх зареєстрованих юнітів бою, щоб зберегти і живих, і тих, хто вже загинув на момент збереження
        IEnumerable<BattleUnit> sourceUnits = (turnManager.registeredUnits != null && turnManager.registeredUnits.Count > 0)
            ? turnManager.registeredUnits
            : turnManager.allUnits;

        foreach (var unit in sourceUnits)
        {
            if (unit != null && unit.data != null)
            {
                state.units.Add(new UnitSaveData
                {
                    unitName = unit.data.unitName,
                    stackSize = Mathf.Max(0, unit.stackSize),
                    currentHealth = Mathf.Max(0, unit.currentHealth),
                    teamId = unit.teamId,
                    x = unit.hexCoords.x,
                    y = unit.hexCoords.y
                });
            }
        }

        string json = JsonUtility.ToJson(state, true);
        string path = Path.Combine(Application.persistentDataPath, "battle_save.json");
        File.WriteAllText(path, json);
        Debug.Log($"[Бій збережено] Активний хід: {state.activeUnitName} (Team {state.activeUnitTeamId}). Записів: {state.units.Count}");
    }

    public void LoadGameFromJson()
    {
        string path = Path.Combine(Application.persistentDataPath, "battle_save.json");
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[Відновлення] Файл збереження не знайдено за шляхом: {path}");
            return;
        }

        string json = File.ReadAllText(path);
        BattleStateContainer state = JsonUtility.FromJson<BattleStateContainer>(json);

        if (state == null)
        {
            Debug.LogWarning("[Відновлення] Не вдалося прочитати дані з JSON.");
            return;
        }

        if (turnManager == null) turnManager = FindAnyObjectByType<TurnManager>();
        if (gridManager == null) gridManager = FindAnyObjectByType<HexGridManager>();
        if (inputHandler == null) inputHandler = FindAnyObjectByType<BattleInputHandler>();

        if (turnManager != null && !string.IsNullOrEmpty(state.heroName))
        {
            turnManager.playerHeroName = state.heroName;
        }

        // Збираємо всіх наявних юнітів (і з allUnits, і з registeredUnits, і на сцені на випадок видалення зі списків)
        List<BattleUnit> pool = new List<BattleUnit>();
        if (turnManager != null)
        {
            if (turnManager.registeredUnits != null) pool.AddRange(turnManager.registeredUnits.Where(u => u != null));
            if (turnManager.allUnits != null) pool.AddRange(turnManager.allUnits.Where(u => u != null));
        }
        pool.AddRange(FindObjectsByType<BattleUnit>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        pool = pool.Distinct().ToList();

        BattleUnit restoredActiveUnit = null;

        foreach (var saved in state.units)
        {
            BattleUnit matchedUnit = FindMatchingUnit(pool, saved);

            if (matchedUnit != null)
            {
                pool.Remove(matchedUnit);
                ApplySavedUnitState(matchedUnit, saved);

                if (matchedUnit.stackSize > 0 &&
                    state.activeUnitTeamId >= 0 &&
                    matchedUnit.teamId == state.activeUnitTeamId &&
                    matchedUnit.data != null &&
                    matchedUnit.data.unitName == state.activeUnitName &&
                    restoredActiveUnit == null)
                {
                    restoredActiveUnit = matchedUnit;
                }
            }
        }

        if (turnManager != null)
        {
            if (restoredActiveUnit == null && state.activeUnitTeamId >= 0)
            {
                restoredActiveUnit = turnManager.allUnits.FirstOrDefault(u =>
                    u != null &&
                    u.stackSize > 0 &&
                    u.teamId == state.activeUnitTeamId &&
                    u.hexCoords == new Vector2Int(state.activeUnitX, state.activeUnitY));
            }

            turnManager.RestoreActiveUnit(restoredActiveUnit);
        }

        if (inputHandler != null)
        {
            inputHandler.RefreshAfterLoad();
        }

        Debug.Log($"[Бій відновлено] Поточний хід: {(turnManager != null && turnManager.activeUnit != null && turnManager.activeUnit.data != null ? turnManager.activeUnit.data.unitName : "невизначено")}");
    }

    private BattleUnit FindMatchingUnit(List<BattleUnit> pool, UnitSaveData saved)
    {
        BattleUnit exact = pool.FirstOrDefault(u =>
            u.teamId == saved.teamId &&
            u.data != null &&
            u.data.unitName == saved.unitName);

        if (exact != null) return exact;

        return pool.FirstOrDefault(u => u.teamId == saved.teamId);
    }

    private void ApplySavedUnitState(BattleUnit unit, UnitSaveData saved)
    {
        Vector2Int targetCoords = new Vector2Int(saved.x, saved.y);
        int hp = saved.currentHealth > 0
            ? saved.currentHealth
            : (unit.data != null ? unit.data.maxHealth : 1);

        unit.RestoreAliveState(saved.stackSize, hp, targetCoords);

        if (gridManager != null &&
            gridManager.gridArray != null &&
            targetCoords.x >= 0 && targetCoords.x < gridManager.columns &&
            targetCoords.y >= 0 && targetCoords.y < gridManager.rows)
        {
            GameObject hexObj = gridManager.gridArray[targetCoords.x, targetCoords.y];
            if (hexObj != null)
            {
                Vector3 pos = hexObj.transform.position;
                pos.z = unit.transform.position.z;
                unit.transform.position = pos;
            }
        }
    }
}