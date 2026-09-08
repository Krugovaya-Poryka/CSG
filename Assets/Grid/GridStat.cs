using TMPro;
using UnityEngine;

public class GridStat : MonoBehaviour
{
    public int visited = -1;
    public int x;
    public int y;
    public bool walkable = true;
    public TMP_Text coordinateText;

    public void UpdateText()
    {
        coordinateText.text = x + ", " + y + "\n " + walkable.ToString();
    }
}