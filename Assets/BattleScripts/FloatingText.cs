using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 1.5f; // Швидкість підняття вгору
    public float fadeSpeed = 1.5f; // Швидкість зникнення
    public float lifetime = 1.0f;  // Час життя об'єкта в секундах

    private TextMeshPro textMesh;
    private Color textColor;

    public void Setup(int damageAmount)
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh != null)
        {
            textMesh.text = $"-{damageAmount}";
            textColor = textMesh.color;
        }

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Повільний рух вгору
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);

        // Плавне зменшення прозорості (Alpha)
        if (textMesh != null)
        {
            textColor.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = textColor;
        }
    }
}