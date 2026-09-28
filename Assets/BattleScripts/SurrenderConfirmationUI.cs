using UnityEngine;
using TMPro;

public class SurrenderConfirmationUI : MonoBehaviour
{
    [Header("Посилання")]
    public TurnManager turnManager;
    public GameObject confirmationPanel; // Панель-вікно підтвердження

    private void Awake()
    {
        if (turnManager == null) turnManager = FindAnyObjectByType<TurnManager>();

        // Переконуємось, що вікно закрите на початку гри
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
    }

    public void OpenConfirmationWindow()
    {
        // Не відкриваємо вікно, якщо бій уже закінчено
        if (turnManager == null || turnManager.activeUnit == null) return;

        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
        }
    }

    public void ConfirmSurrender()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }

        if (turnManager != null)
        {
            turnManager.Surrender();
        }
    }

    public void CancelSurrender()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
    }
}