using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroUI : MonoBehaviour
{
    public TMP_Text heroNameText;
    public TMP_Text movementText;
    public Image heroPortrait;

    private HeroController currentHero;

    public void ShowHero(HeroController hero)
    {
        currentHero = hero;

        if (hero == null)
        {
            heroNameText.text = "No hero";
            movementText.text = "";
            return;
        }

        heroNameText.text = hero.name;

        UpdateUI();
    }

    private void Update()
    {
        if (currentHero != null)
            UpdateUI();
    }

    private void UpdateUI()
    {
        movementText.text = "Movement: " + currentHero.movementPoints + " / " + currentHero.maxMovementPoints; 
    }
}