using UnityEngine;

public class PathMaker : MonoBehaviour
{
    public Sprite arrowsRED;
    public Sprite arrowsGREEN;
    public Sprite endRED;
    public Sprite endGREEN;

    private SpriteRenderer arrow;
    private Sprite currentSprite;

    private void Awake()
    {
        arrow = GetComponent<SpriteRenderer>();
    }

    public void SetGreen()
    {
        currentSprite = arrowsGREEN;
        arrow.sprite = currentSprite;
    }

    public void SetGreenEnd()
    {
        currentSprite = endGREEN;
        arrow.sprite = currentSprite;
    }
    
    public void SetRed()
    {
        currentSprite = arrowsRED;
        arrow.sprite = currentSprite;
    }
    
    public void SetRedEnd()
    {
        currentSprite = endRED;
        arrow.sprite = currentSprite;
    }
    
    public void SetDirection(float angle)
    {
        
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

}
