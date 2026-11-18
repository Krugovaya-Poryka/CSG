using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Actual map overview; clicking it moves the existing adventure camera.
public class StrategyMinimap : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private GridBehavior grid;
    [SerializeField] private Texture2D map;
    private float refreshAt;

    private void Update()
    {
        if (Time.unscaledTime < refreshAt) return;
        refreshAt = Time.unscaledTime + 0.5f;
        if (grid == null) grid = FindAnyObjectByType<GridBehavior>();
        if (grid == null || grid.gridArray == null) return;
        if (map == null || map.width != grid.columns || map.height != grid.rows)
        {
            if (map != null) Destroy(map);
            map = new Texture2D(grid.columns, grid.rows, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            GetComponent<RawImage>().texture = map;
        }
        for (int y = 0; y < grid.rows; y++)
            for (int x = 0; x < grid.columns; x++)
            {
                GridStat tile = grid.GetTile(x, y);
                Color32 color = new Color32(32, 32, 25, 255);
                if (tile != null)
                {
                    switch (tile.tileType)
                    {
                        case GridStat.TileType.Water: color = new Color32(46, 70, 91, 255); break;
                        case GridStat.TileType.Forest: color = new Color32(43, 69, 35, 255); break;
                        case GridStat.TileType.Road: color = new Color32(179, 144, 92, 255); break;
                        case GridStat.TileType.Mountain: color = new Color32(99, 94, 82, 255); break;
                        default: color = new Color32(100, 119, 60, 255); break;
                    }
                    if (tile.castle != null) color = new Color32(233, 194, 99, 255);
                }
                map.SetPixel(x, y, color);
            }
        if (TeamManager.Instance != null)
        {
            int id = HeroSelectionManager.Instance != null ? HeroSelectionManager.Instance.currentTeamId : 0;
            Team team = TeamManager.Instance.GetTeam(id);
            if (team != null && team.heroes != null)
                foreach (HeroController hero in team.heroes)
                    if (hero != null && hero.gridX >= 0 && hero.gridX < map.width && hero.gridY >= 0 && hero.gridY < map.height)
                        map.SetPixel(hero.gridX, hero.gridY, Color.white);
        }
        map.Apply(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Camera cam = Camera.main;
        if (grid == null || cam == null) return;
        RectTransform rect = (RectTransform)transform;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out Vector2 point)) return;
        float u = Mathf.Clamp01((point.x - rect.rect.xMin) / rect.rect.width);
        float v = Mathf.Clamp01((point.y - rect.rect.yMin) / rect.rect.height);
        cam.transform.position = new Vector3(grid.leftBottomLocation.x + u * (grid.columns - 1) * grid.scale,
            grid.leftBottomLocation.y + v * (grid.rows - 1) * grid.scale, cam.transform.position.z);
    }
    private void OnDestroy() { if (map != null) Destroy(map); }
}
