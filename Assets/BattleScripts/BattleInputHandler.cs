using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class BattleInputHandler : MonoBehaviour
{
    public TurnManager turnManager;
    public HexGridManager gridManager;

    private bool isProcessingAction = false;
    private BattleUnit currentActiveUnit;
    private Dictionary<Vector2Int, Vector2Int> currentPathMap;
    private Dictionary<SpriteRenderer, Color> originalColors = new Dictionary<SpriteRenderer, Color>();

    [Header("Кольори підсвічування")]
    public Color moveHighlightColor = new Color(0.3f, 1f, 0.3f, 0.6f);
    public Color activePlayerColor = new Color(0.2f, 1f, 0.2f, 0.8f);
    public Color enemyTargetColor = new Color(1f, 0.2f, 0.2f, 0.7f);

    private void Awake()
    {
        if (turnManager == null) turnManager = FindFirstObjectByType<TurnManager>();
        if (gridManager == null) gridManager = FindFirstObjectByType<HexGridManager>();
    }

    void Update()
    {
        if (turnManager == null || gridManager == null) return;

        // Якщо немає активного юніта (бій закінчено) — вимикаємо підсвітку
        if (turnManager.activeUnit == null || turnManager.allUnits.Count == 0)
        {
            if (currentPathMap != null || currentActiveUnit != null)
            {
                currentPathMap = null;
                currentActiveUnit = null;
                ClearHighlights();
            }
            return;
        }

        // При зміні активного юніта оновлюємо область ходу та підсвічування
        if (turnManager.activeUnit != currentActiveUnit && !isProcessingAction)
        {
            currentActiveUnit = turnManager.activeUnit;
            HighlightMoveArea();
        }

        if (isProcessingAction) return;

        // --- ОБРОБКА КЛАВІАТУРИ (Тільки для юнітів гравця teamId == 0) ---
        if (Keyboard.current != null && turnManager.activeUnit != null && turnManager.activeUnit.teamId == 0)
        {
            // 1. Клавіша W — Перенести в кінець черги (Wait)
            if (Keyboard.current.wKey.wasPressedThisFrame)
            {
                if (!turnManager.activeUnit.hasWaited)
                {
                    ClearHighlights();
                    turnManager.WaitCurrentUnit();
                    return;
                }
                else
                {
                    if (BattleLogUI.Instance != null)
                    {
                        BattleLogUI.Instance.LogCustomMessage("Цей юніт вже відкладав хід у цьому раунді!");
                    }
                }
            }

            // 2. Клавіша Space — Пропустити хід / Захист
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                ClearHighlights();
                turnManager.SkipCurrentUnitTurn();
                return;
            }
        }

        // Обробка кліку миші
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            // ЗАХИСТ ВІД КЛІКІВ КРІЗЬ UI: якщо курсор над кнопкою чи вікном — ігноруємо клік по гексу!
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(screenPos);

            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

            BattleUnit clickedUnit = null;
            BattleHex clickedHex = null;

            foreach (var hit in hits)
            {
                if (clickedUnit == null) clickedUnit = hit.collider.GetComponentInParent<BattleUnit>();
                if (clickedHex == null) clickedHex = hit.collider.GetComponent<BattleHex>();
            }

            // Якщо клікнули по гексу, де стоїть юніт
            if (clickedUnit == null && clickedHex != null)
            {
                Vector2Int hexCoords = new Vector2Int(clickedHex.x, clickedHex.y);
                clickedUnit = GetUnitAtHex(hexCoords);
            }

            // --- КЛІК ПО ВОРОГУ ---
            if (clickedUnit != null && clickedUnit.teamId != turnManager.activeUnit.teamId && clickedUnit.stackSize > 0)
            {
                BattleUnit attacker = turnManager.activeUnit;
                int distance = HexUtils.GetHexDistance(attacker.hexCoords, clickedUnit.hexCoords);
                bool isEnemyAdjacent = CheckIfEnemyIsAdjacent(attacker);

                // 1. Дальній бій (лише якщо є постріли, НЕМАЄ ворога упритул і відстань > 1)
                if (attacker.data.isRanged && attacker.currentShots > 0 && !isEnemyAdjacent && distance > 1)
                {
                    ExecuteAction(() => attacker.RangedAttack(clickedUnit, OnActionComplete));
                }
                // 2. Ближній бій упритул (дистанція = 1)
                else if (distance == 1)
                {
                    ExecuteAction(() => attacker.MeleeAttack(clickedUnit, OnActionComplete));
                }
                // 3. Ближній бій з підходом (переміщення по сітці + удар)
                else
                {
                    Vector2Int? attackHexCoords = GetBestAttackHex(attacker, clickedUnit);

                    if (attackHexCoords.HasValue)
                    {
                        Vector2Int targetCoords = attackHexCoords.Value;

                        List<Vector2Int> pathCoords = HexPathfinding.ReconstructPath(
                            attacker.hexCoords,
                            targetCoords,
                            currentPathMap,
                            attacker.data.isFlyer
                        );

                        List<GameObject> pathHexes = ConvertCoordsToGameObjects(pathCoords);

                        ExecuteAction(() => {
                            StartCoroutine(attacker.MoveAlongPath(pathHexes, targetCoords, () => {
                                attacker.MeleeAttack(clickedUnit, OnActionComplete);
                            }));
                        });
                    }
                }
            }
            // --- ПЕРЕМІЩЕННЯ НА ПОРОЖНІЙ ГЕКС ---
            else if (clickedHex != null && clickedHex.walkable)
            {
                Vector2Int targetCoords = new Vector2Int(clickedHex.x, clickedHex.y);

                if (!IsHexOccupied(targetCoords) && currentPathMap != null && currentPathMap.ContainsKey(targetCoords) && targetCoords != turnManager.activeUnit.hexCoords)
                {
                    List<Vector2Int> pathCoords = HexPathfinding.ReconstructPath(
                        turnManager.activeUnit.hexCoords,
                        targetCoords,
                        currentPathMap,
                        turnManager.activeUnit.data.isFlyer
                    );

                    List<GameObject> pathHexes = ConvertCoordsToGameObjects(pathCoords);

                    ExecuteAction(() => {
                        StartCoroutine(turnManager.activeUnit.MoveAlongPath(pathHexes, targetCoords, OnActionComplete));
                    });
                }
            }
        }
    }

    public BattleUnit GetUnitAtHex(Vector2Int coords)
    {
        foreach (var unit in turnManager.allUnits)
        {
            if (unit != null && unit.stackSize > 0 && unit.hexCoords == coords)
                return unit;
        }
        return null;
    }

    private List<GameObject> ConvertCoordsToGameObjects(List<Vector2Int> coordsList)
    {
        List<GameObject> hexes = new List<GameObject>();
        foreach (var coord in coordsList)
        {
            if (coord.x >= 0 && coord.x < gridManager.columns && coord.y >= 0 && coord.y < gridManager.rows)
            {
                hexes.Add(gridManager.gridArray[coord.x, coord.y]);
            }
        }
        return hexes;
    }

    private Vector2Int? GetBestAttackHex(BattleUnit attacker, BattleUnit target)
    {
        List<Vector2Int> neighbors = HexPathfinding.GetNeighbors(target.hexCoords);
        Vector2Int? bestHex = null;
        int minDistance = int.MaxValue;

        foreach (var hexCoords in neighbors)
        {
            if (hexCoords.x < 0 || hexCoords.x >= gridManager.columns ||
                hexCoords.y < 0 || hexCoords.y >= gridManager.rows) continue;

            if (currentPathMap == null || !currentPathMap.ContainsKey(hexCoords)) continue;

            BattleHex stat = gridManager.gridArray[hexCoords.x, hexCoords.y].GetComponent<BattleHex>();
            if (stat == null || !stat.walkable || IsHexOccupied(hexCoords)) continue;

            int distToAttacker = HexUtils.GetHexDistance(attacker.hexCoords, hexCoords);
            if (distToAttacker < minDistance)
            {
                minDistance = distToAttacker;
                bestHex = hexCoords;
            }
        }

        return bestHex;
    }

    private void HighlightMoveArea()
    {
        ClearHighlights();
        if (currentActiveUnit == null || currentActiveUnit.stackSize <= 0) return;

        HashSet<Vector2Int> blockedHexes = new HashSet<Vector2Int>();

        BattleHex[] allHexes = FindObjectsByType<BattleHex>(FindObjectsSortMode.None);
        foreach (var hex in allHexes)
        {
            if (!hex.walkable) blockedHexes.Add(new Vector2Int(hex.x, hex.y));
        }

        foreach (var unit in turnManager.allUnits)
        {
            if (unit != null && unit.stackSize > 0)
            {
                blockedHexes.Add(unit.hexCoords);
            }
        }

        Vector2Int start = currentActiveUnit.hexCoords;
        bool isFlyer = currentActiveUnit.data.isFlyer;
        int speed = currentActiveUnit.data.speed;

        currentPathMap = HexPathfinding.FindReachableArea(start, speed, isFlyer, blockedHexes);

        // Сірі гекси під доступними порожніми клітинками для переміщення
        foreach (var hex in allHexes)
        {
            Vector2Int hexCoords = new Vector2Int(hex.x, hex.y);

            if (!hex.walkable || IsHexOccupied(hexCoords)) continue;

            if (currentPathMap.ContainsKey(hexCoords) && hexCoords != start)
            {
                SetHexColor(hex.gameObject, moveHighlightColor);
            }
        }

        // ПІДСВІЧУЄМО ТІЛЬКИ ЯКЩО ХОДИТЬ ЮНІТ ГРАВЦЯ (teamId == 0)
        if (currentActiveUnit.teamId == 0)
        {
            // 1. Зелений гекс під самісіньким activeUnit
            GameObject activeHex = gridManager.gridArray[start.x, start.y];
            SetHexColor(activeHex, activePlayerColor);

            // 2. Червоні гекси під ворогами, яких можна атакувати
            List<BattleUnit> attackableTargets = GetAttackableTargets(currentActiveUnit);
            foreach (var target in attackableTargets)
            {
                GameObject targetHex = gridManager.gridArray[target.hexCoords.x, target.hexCoords.y];
                SetHexColor(targetHex, enemyTargetColor);
            }
        }
    }

    private List<BattleUnit> GetAttackableTargets(BattleUnit attacker)
    {
        List<BattleUnit> targets = new List<BattleUnit>();
        if (attacker == null || attacker.stackSize <= 0) return targets;

        bool isBlockedByEnemy = CheckIfEnemyIsAdjacent(attacker);

        // Дальнобійник може стріляти, лише якщо немає ворога упритул і є набої
        bool canShoot = attacker.data.isRanged && attacker.currentShots > 0 && !isBlockedByEnemy;

        foreach (var enemy in turnManager.allUnits)
        {
            if (enemy == null || enemy.stackSize <= 0 || enemy.teamId == attacker.teamId)
                continue;

            if (canShoot)
            {
                // Якщо не заблокований — бачить і може обстріляти будь-кого
                targets.Add(enemy);
            }
            else
            {
                // Якщо заблокований або це мілішник:
                int dist = HexUtils.GetHexDistance(attacker.hexCoords, enemy.hexCoords);
                // 1. Ворог стоїть упритул
                if (dist == 1)
                {
                    targets.Add(enemy);
                }
                // 2. Або ми можемо дойти до нього за цей хід для удару
                else if (GetBestAttackHex(attacker, enemy) != null)
                {
                    targets.Add(enemy);
                }
            }
        }

        return targets;
    }

    private void SetHexColor(GameObject hexObj, Color color)
    {
        if (hexObj == null) return;

        SpriteRenderer sr = null;
        Transform innerTile = hexObj.transform.Find("InnerTile");
        if (innerTile != null)
        {
            sr = innerTile.GetComponent<SpriteRenderer>();
        }
        if (sr == null)
        {
            sr = hexObj.GetComponent<SpriteRenderer>();
        }

        if (sr != null)
        {
            if (!originalColors.ContainsKey(sr))
            {
                originalColors[sr] = sr.color;
            }
            sr.color = color;
        }
    }

    public void ClearHighlights()
    {
        foreach (var pair in originalColors)
        {
            if (pair.Key != null)
            {
                pair.Key.color = pair.Value;
            }
        }
        originalColors.Clear();
    }

    private bool IsHexOccupied(Vector2Int coords)
    {
        return GetUnitAtHex(coords) != null;
    }

    private void ExecuteAction(System.Action action)
    {
        isProcessingAction = true;
        ClearHighlights();
        action.Invoke();
    }

    private void OnActionComplete()
    {
        isProcessingAction = false;
        turnManager.EndTurn();
    }

    private bool CheckIfEnemyIsAdjacent(BattleUnit unit)
    {
        foreach (var other in turnManager.allUnits)
        {
            if (other != null && other.teamId != unit.teamId && other.stackSize > 0)
            {
                if (HexUtils.GetHexDistance(unit.hexCoords, other.hexCoords) == 1) return true;
            }
        }
        return false;
    }
}