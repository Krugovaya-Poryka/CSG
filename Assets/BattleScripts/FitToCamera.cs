using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FitToCamera : MonoBehaviour
{
    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Camera cam = Camera.main;

        if (sr == null || sr.sprite == null || cam == null) return;

        // 1. Центруємо фон за позицією камери (зберігаємо Z фону)
        transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, transform.position.z);

        // 2. Вираховуємо розмір видимої області камери у світових одиницях
        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        // 3. Вираховуємо початковий розмір спрайту
        Vector2 spriteSize = sr.sprite.bounds.size;

        // 4. Масштабуємо спрайт точно під розміри камери
        transform.localScale = new Vector3(camWidth / spriteSize.x, camHeight / spriteSize.y, 1f);
    }
}