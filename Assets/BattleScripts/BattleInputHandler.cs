using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BattleInputHandler : MonoBehaviour
{
    public TurnManager turnManager;
    public Color moveHighlightColor = new Color(0.5f, 0.5f, 0.5f, 0.6f); // Зелений напівпрозорий колір

    private bool isProcessingAction = false;
    private BattleUnit currentActiveUnit;
    private Dictionary<Vector2Int, Vector2Int> currentPathMap;

    // Словник для збереження оригінальних кольорів текстур
    private Dictionary<SpriteRenderer, Color> originalColors = new Dictionary<SpriteRenderer, Color>();

    private void Awake()
    {
        if (turnManager == null) turnManager = FindFirstObjectByType<TurnManager>();
    }

    void Update()
    {
        if (turnManager == null) return;

        // Коли змінюється активний юніт — оновлюємо підсвітку гексів
        if (turnManager.activeUnit != currentActiveUnit && !isProcessingAction)
        {
            currentActiveUnit = turnManager.activeUnit;
            HighlightMoveArea();
        }

        if (isProcessingAction || turnManager.activeUnit == null) return;

        // Перевіряємо клік ЛКМ через новий Input System
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(screenPos);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                BattleUnit clickedUnit = hit.collider.GetComponent<BattleUnit>();
                GridStat clickedHex = hit.collider.GetComponent<GridStat>();

                // Атака ворога
                if (clickedUnit != null && clickedUnit.teamId != turnManager.activeUnit.teamId)
                {
                    BattleUnit attacker = turnManager.activeUnit;
                    int distance = GetHexDistance(attacker.hexCoords, clickedUnit.hexCoords);
                    bool isEnemyAdjacent = CheckIfEnemyIsAdjacent(attacker);

                    if (attacker.data.isRanged && attacker.currentShots > 0 && !isEnemyAdjacent && distance > 1)
                    {
                        ExecuteAction(() => attacker.RangedAttack(clickedUnit, OnActionComplete));
                    }
                    else if (distance == 1)
                    {
                        ExecuteAction(() => attacker.MeleeAttack(clickedUnit, OnActionComplete));
                    }
                }
                // Переміщення на підсвічений гекс
                else if (clickedHex != null)
                {
                    Vector2Int targetCoords = new Vector2Int(clickedHex.x, clickedHex.y);
                    int moveDistance = GetHexDistance(turnManager.activeUnit.hexCoords, targetCoords);

                    if (moveDistance <= turnManager.activeUnit.data.speed && moveDistance > 0)
                    {
                        ExecuteAction(() => {
                            StartCoroutine(turnManager.activeUnit.MoveToHex(clickedHex.gameObject, targetCoords, OnActionComplete));
                        });
                    }
                }
            }
        }
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

            bool shouldHighlight = false;

            if (isFlyer)
            {
                if (GetHexDistance(start, hexCoords) <= speed && GetHexDistance(start, hexCoords) > 0)
                {
                    shouldHighlight = true;
                }
            }
            else
            {
                if (currentPathMap.ContainsKey(hexCoords) && hexCoords != start)
                {
                    shouldHighlight = true;
                }
            }

            if (shouldHighlight)
            {
                // Шукаємо дочірній об'єкт з назвою "InnerTile"
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
        foreach (var unit in turnManager.allUnits)
        {
            if (unit != null && unit.stackSize > 0 && unit.hexCoords == coords)
                return true;
        }
        return false;
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

    private int GetHexDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
    }

    private bool CheckIfEnemyIsAdjacent(BattleUnit unit)
    {
        foreach (var other in turnManager.allUnits)
        {
            if (other != null && other.teamId != unit.teamId && other.stackSize > 0)
            {
                if (GetHexDistance(unit.hexCoords, other.hexCoords) == 1) return true;
            }
        }
        return false;
    }
}