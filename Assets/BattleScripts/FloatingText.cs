using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float fadeSpeed = 1.5f;
    public float lifetime = 1.0f;

    private TMP_Text textMesh;
    private Color textColor;

    public void Setup(int damageAmount)
    {
        // Шукаємо компонент будь-якого типу TextMeshPro
        textMesh = GetComponent<TMP_Text>();
        if (textMesh == null) textMesh = GetComponentInChildren<TMP_Text>();

        if (textMesh != null)
        {
            textMesh.text = $"-{damageAmount}";
            textColor = textMesh.color;
        }

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);

        if (textMesh != null)
        {
            textColor.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = textColor;
        }
    }
}