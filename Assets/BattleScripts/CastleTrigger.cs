using UnityEngine;
using UnityEngine.SceneManagement; // Позволяет менять сцены

public class CastleTrigger : MonoBehaviour
{
    // Имя сцены замка, в которую переходим
    [SerializeField] private string castleSceneName = "CastleScene";

    private bool isPlayerInside = false;

    private void Update()
    {
        // Проверяем: если игрок находится в триггере И нажал клавишу E
        if (isPlayerInside && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Загрузка сцены замка...");
            SceneManager.LoadScene(castleSceneName);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Регистрируем вхождение только если у объекта тег "Player"
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            Debug.Log("Игрок подошел к дверям! Нажми E.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            Debug.Log("Игрок отошел от дверей.");
        }
    }
}