using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 100f; // Швидкість руху вгору у пікселях Canvas
    public float fadeSpeed = 1.5f;  // Швидкість зникання
    public float lifetime = 1.0f;   // Час життя у секундах

    private TMP_Text textMesh;
    private Color textColor;
    private bool isInitialized = false;

    private void FindTextComponent()
    {
        if (textMesh == null)
        {
            textMesh = GetComponent<TMP_Text>();
            if (textMesh == null) textMesh = GetComponentInChildren<TMP_Text>();
        }
    }

    public void Setup(int damageAmount)
    {
        FindTextComponent();

        if (textMesh != null)
        {
            textMesh.text = $"-{damageAmount}";
            textColor = textMesh.color;
            textColor.a = 1f; // Гарантуємо 100% видимість на початку
            textMesh.color = textColor;

            // ГАРАНТІЯ ВИДИМОСТІ: примусово виносимо текст поверх усіх спрайтів та сітки
            Renderer textRenderer = textMesh.GetComponent<Renderer>();
            if (textRenderer != null)
            {
                textRenderer.sortingOrder = 100;
            }
        }

        isInitialized = true;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Рух вгору у пікселях UI
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);

        // Поступове зменшення альфа-каналу (прозорість)
        if (isInitialized && textMesh != null)
        {
            textColor.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = textColor;
        }
    }
}