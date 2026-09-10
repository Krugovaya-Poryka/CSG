using UnityEngine;

public class BattleSetup : MonoBehaviour
{
    public HexGridManager gridManager;
    public TurnManager turnManager;
    public GameObject unitPrefab;

    [Header("Герої бою")]
    public HeroArmy playerHero;
    public HeroArmy enemyHero;

    void Start()
    {
        Debug.Log("--- BattleSetup Start ---");

        if (playerHero == null || enemyHero == null)
        {
            Debug.LogError("ПОМИЛКА: Не заповнено PlayerHero або EnemyHero в Inspector!");
            return;
        }

        if (gridManager == null)
        {
            Debug.LogError("ПОМИЛКА: Не заповнено GridManager в Inspector!");
            return;
        }

        if (unitPrefab == null)
        {
            Debug.LogError("ПОМИЛКА: Не перетягнуто UnitPrefab в Inspector!");
            return;
        }

        InitializeBattle(playerHero, enemyHero);
    }

    public void InitializeBattle(HeroArmy attacker, HeroArmy defender)
    {
        Debug.Log("Починаємо спавн армій...");
        SpawnArmy(attacker, teamId: 0, targetColumn: 0);
        SpawnArmy(defender, teamId: 1, targetColumn: gridManager.columns - 1);

        if (turnManager != null)
            turnManager.StartBattle();
    }

    private void SpawnArmy(HeroArmy army, int teamId, int targetColumn)
    {
        int[] startRows = new int[] { 10, 8, 6, 5, 4, 2, 0 };

        Debug.Log($"Спавн для {army.heroName} (Team {teamId}). Слотів: {army.slots.Length}");

        for (int i = 0; i < army.slots.Length; i++)
        {
            if (i >= startRows.Length) break;

            var slot = army.slots[i];

            if (slot.unitData == null)
            {
                Debug.LogWarning($"Слот {i}: порожній (UnitData == null)");
                continue;
            }

            if (slot.count <= 0)
            {
                Debug.LogWarning($"Слот {i}: Кількість ({slot.count}) <= 0");
                continue;
            }

            int row = startRows[i];

            if (gridManager.gridArray == null)
            {
                Debug.LogError("ПОМИЛКА: Сітка gridArray досі NULL! Змініть Start() на Awake() у HexGridManager.");
                return;
            }

            GameObject hex = gridManager.gridArray[targetColumn, row];
            Vector3 spawnPos = hex.transform.position;

            Debug.Log($"Створюємо {slot.unitData.name} ({slot.count} шт) на гексі [{targetColumn}, {row}]");

            GameObject unitObj = Instantiate(unitPrefab, spawnPos, Quaternion.identity);
            BattleUnit bUnit = unitObj.GetComponent<BattleUnit>();

            if (bUnit != null)
            {
                bUnit.Init(slot.unitData, slot.count, teamId);
                bUnit.hexCoords = new Vector2Int(targetColumn, row);
            }

            if (teamId == 1)
            {
                Vector3 scale = unitObj.transform.localScale;
                scale.x *= -1;
                unitObj.transform.localScale = scale;
            }

            if (turnManager != null)
                turnManager.allUnits.Add(bUnit);
        }
    }
}