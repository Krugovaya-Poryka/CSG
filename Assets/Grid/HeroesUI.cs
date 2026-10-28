using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroUI : MonoBehaviour
{
    public TMP_Text heroNameText;
    public TMP_Text movementText;
    [Tooltip("Reserved portrait hook; the current HUD displays text only.")]
    public Image heroPortrait;

    private HeroController currentHero;

    private void Start()
    {
        ShowHero(HeroSelectionManager.Instance != null
            ? HeroSelectionManager.Instance.selectedHero : null);
    }

    public void ShowHero(HeroController hero)
    {
        currentHero = hero;

        if (hero == null)
        {
            if (heroNameText != null)
                heroNameText.text = "No hero";
            if (movementText != null)
                movementText.text = "";
            return;
        }

        if (heroNameText != null)
            heroNameText.text = hero.name;

        UpdateUI();
    }

    private void Update()
    {
        HeroController selected = HeroSelectionManager.Instance != null
            ? HeroSelectionManager.Instance.selectedHero : null;
        if (currentHero != selected)
            ShowHero(selected);
        if (currentHero != null)
            UpdateUI();
    }

    private void UpdateUI()
    {
        if (movementText == null)
            return;

        movementText.text = "Movement: " + currentHero.movementPoints + " / " + currentHero.maxMovementPoints; 
    }
}
