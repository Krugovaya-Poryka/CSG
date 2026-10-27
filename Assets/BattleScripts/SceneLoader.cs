using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void GoToMap()
    {
        SceneManager.LoadScene("Map");
    }

    public void GoToCastle()
    {
        SceneManager.LoadScene("CastleScene");
    }
}