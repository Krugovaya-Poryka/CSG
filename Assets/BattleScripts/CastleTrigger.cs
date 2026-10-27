using UnityEngine;
using UnityEngine.SceneManagement;

public class CastleTrigger : MonoBehaviour
{
   
    [SerializeField] private string castleSceneName = "CastleScene";

    private bool isPlayerInside = false;

    private void Update()
    {
        
        if (isPlayerInside && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Загрузка сцены замка...");
            SceneManager.LoadScene(castleSceneName);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        
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