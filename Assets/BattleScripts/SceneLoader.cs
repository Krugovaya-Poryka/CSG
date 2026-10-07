using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Метод для выхода на карту
    public void GoToMap()
    {
        SceneManager.LoadScene("Map");
    }

    // Метод для входа в замок
    public void GoToCastle()
    {
        SceneManager.LoadScene("CastleScene");
    }
}