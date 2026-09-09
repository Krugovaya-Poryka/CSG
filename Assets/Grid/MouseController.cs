using UnityEngine;
using UnityEngine.InputSystem;

public class MouseController : MonoBehaviour
{
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
            Debug.LogError("Main Camera not found!");
    }

    private void Update()
    {
        if (Mouse.current == null || mainCamera == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(mousePosition);

        Vector2 point = new Vector2(
            worldPosition.x,
            worldPosition.y
        );

        Collider2D[] hits = Physics2D.OverlapPointAll(point);

        if (hits.Length == 0)
        {
            Debug.Log("Nothing clicked");
            return;
        }

        foreach (Collider2D hit in hits)
        {
            HeroController hero =
                hit.GetComponent<HeroController>();

            if (hero != null)
            {
                Debug.Log("HERO SELECTED: " + hero.name);

                HeroSelectionManager.Instance.SelectHero(hero);
                return;
            }
        }

        foreach (Collider2D hit in hits)
        {
            GridStat tile =
                hit.GetComponent<GridStat>();

            if (tile == null)
                continue;

            Debug.Log("TILE CLICKED: " + tile.x + ", " + tile.y);

            if (!tile.walkable)
            {
                Debug.Log("Tile is not walkable");
                return;
            }

            if (HeroSelectionManager.Instance == null)
            {
                Debug.LogError(
                    "HeroSelectionManager not found!"
                );
                return;
            }

            if (HeroSelectionManager.Instance.selectedHero == null)
            {
                Debug.Log("No hero selected");
                return;
            }

            HeroSelectionManager.Instance.MoveSelectedHero(tile);
            return;
        }
    }
}