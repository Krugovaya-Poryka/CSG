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
        {
            // Передаємо ім'я героя гравця у TurnManager
            turnManager.playerHeroName = attacker.heroName;

            turnManager.StartBattle();
        }
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

            // Забираємо всі посилання зі старого BattleUnit на префабі, щоб нічого не загубилось
            BattleUnit templateUnit = unitObj.GetComponent<BattleUnit>();
            var savedDamageText = templateUnit != null ? templateUnit.damageTextPrefab : null;
            var savedSprite = templateUnit != null ? templateUnit.unitSprite : null;
            var savedTexture = templateUnit != null ? templateUnit.textureChild : null;
            var savedStackText = templateUnit != null ? templateUnit.stackText : null;
            var savedStackBg = templateUnit != null ? templateUnit.stackBgImage : null;
            var savedPlayerColor = templateUnit != null ? templateUnit.playerColor : new Color(24f / 255f, 113f / 255f, 202f / 255f, 1f);
            var savedEnemyColor = templateUnit != null ? templateUnit.enemyColor : new Color(195f / 255f, 19f / 255f, 19f / 255f, 1f);

            if (templateUnit != null)
            {
                Destroy(templateUnit);
            }

            BattleUnit bUnit;
            if (slot.unitData.isRanged)
                bUnit = unitObj.AddComponent<RangedBattleUnit>();
            else
                bUnit = unitObj.AddComponent<MeleeBattleUnit>();

            if (bUnit != null)
            {
                bUnit.damageTextPrefab = savedDamageText;
                bUnit.unitSprite = savedSprite;
                bUnit.textureChild = savedTexture;
                bUnit.stackText = savedStackText;
                bUnit.stackBgImage = savedStackBg;
                bUnit.playerColor = savedPlayerColor;
                bUnit.enemyColor = savedEnemyColor;

                bUnit.Init(slot.unitData, slot.count, teamId);
                bUnit.hexCoords = new Vector2Int(targetColumn, row);
            }

            if (teamId == 1 && bUnit != null)
            {
                bUnit.FaceTarget(bUnit.transform.position + Vector3.left);

                AIUnit ai = unitObj.GetComponent<AIUnit>();
                if (ai == null)
                {
                    ai = unitObj.AddComponent<AIUnit>();
                }
                ai.BindBattleUnit(bUnit);
            }

            if (turnManager != null)
                turnManager.allUnits.Add(bUnit);
        }
    }
}