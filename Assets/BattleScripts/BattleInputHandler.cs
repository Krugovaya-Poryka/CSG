using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BattleInputHandler : MonoBehaviour
{
    public TurnManager turnManager;
    public HexGridManager gridManager;
    public Color moveHighlightColor = new Color(0.3f, 1f, 0.3f, 0.6f);

    private bool isProcessingAction = false;
    private BattleUnit currentActiveUnit;
    private Dictionary<Vector2Int, Vector2Int> currentPathMap;

    private Dictionary<SpriteRenderer, Color> originalColors = new Dictionary<SpriteRenderer, Color>();

    private void Awake()
    {
        if (turnManager == null) turnManager = FindFirstObjectByType<TurnManager>();
        if (gridManager == null) gridManager = FindFirstObjectByType<HexGridManager>();
    }

    void Update()
    {
        if (turnManager == null || gridManager == null) return;

        // ЗАХИСТ: Якщо активного юніта немає (бій закінчено) — вимикаємо підсвітку і блокуємо кліки
        if (turnManager.activeUnit == null || turnManager.allUnits.Count == 0)
        {
            if (currentPathMap != null)
            {
                currentPathMap = null;
                currentActiveUnit = null;
                ClearHighlights();
            }
            return;
        }

        if (turnManager.activeUnit != currentActiveUnit && !isProcessingAction)
        {
            currentActiveUnit = turnManager.activeUnit;
            HighlightMoveArea();
        }

        if (isProcessingAction) return;

        if (turnManager.activeUnit != currentActiveUnit && !isProcessingAction)
        {
            currentActiveUnit = turnManager.activeUnit;
            HighlightMoveArea();
        }

        if (isProcessingAction || turnManager.activeUnit == null) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(screenPos);

            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

            BattleUnit clickedUnit = null;
            GridStat clickedHex = null;

            foreach (var hit in hits)
            {
                if (clickedUnit == null) clickedUnit = hit.collider.GetComponentInParent<BattleUnit>();
                if (clickedHex == null) clickedHex = hit.collider.GetComponent<GridStat>();
            }

            // ЯКЩО КЛІКНУЛИ ПО ТАЙЛУ: Шукаємо, чи стоїть на цьому тайлі юніт
            if (clickedUnit == null && clickedHex != null)
            {
                Vector2Int hexCoords = new Vector2Int(clickedHex.x, clickedHex.y);
                clickedUnit = GetUnitAtHex(hexCoords);
            }

            // --- КЛІК ПО ВОРОГУ (по спрайту АБО по його тайлу) ---
            if (clickedUnit != null && clickedUnit.teamId != turnManager.activeUnit.teamId && clickedUnit.stackSize > 0)
            {
                BattleUnit attacker = turnManager.activeUnit;
                int distance = HexUtils.GetHexDistance(attacker.hexCoords, clickedUnit.hexCoords);
                bool isEnemyAdjacent = CheckIfEnemyIsAdjacent(attacker);

                // 1. Дальній бій
                if (attacker.data.isRanged && attacker.currentShots > 0 && !isEnemyAdjacent && distance > 1)
                {
                    ExecuteAction(() => attacker.RangedAttack(clickedUnit, OnActionComplete));
                }
                // 2. Ближній бій упритул
                else if (distance == 1)
                {
                    ExecuteAction(() => attacker.MeleeAttack(clickedUnit, OnActionComplete));
                }
                // 3. Ближній бій з підходом (рух по покроковому шляху + удар)
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

                // ПЕРЕВІРКА: не дозволяємо рухатися на клітинку, де вже хтось стоїть
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

            GridStat stat = gridManager.gridArray[hexCoords.x, hexCoords.y].GetComponent<GridStat>();
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
        if (currentActiveUnit == null) return;

        HashSet<Vector2Int> blockedHexes = new HashSet<Vector2Int>();

        GridStat[] allHexes = FindObjectsByType<GridStat>(FindObjectsSortMode.None);
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

        foreach (var hex in allHexes)
        {
            Vector2Int hexCoords = new Vector2Int(hex.x, hex.y);

            if (!hex.walkable || IsHexOccupied(hexCoords)) continue;

            if (currentPathMap.ContainsKey(hexCoords) && hexCoords != start)
            {
                Transform innerTile = hex.transform.Find("InnerTile");
                if (innerTile != null)
                {
                    SpriteRenderer sr = innerTile.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        if (!originalColors.ContainsKey(sr))
                        {
                            originalColors[sr] = sr.color;
                        }
                        sr.color = moveHighlightColor;
                    }
                }
            }
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