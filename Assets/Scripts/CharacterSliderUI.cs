using UnityEngine;
using UnityEngine.UI;

public class CharacterSliderUI : MonoBehaviour
{
    [SerializeField] private float sliderSpeed = 5f;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider exhaustionSlider;
    [SerializeField] private Canvas playerCanvas;
    private float displayedHealth;
    private float displayedExhaustion;
    private Character player;

    
    private void Update()
    {
        if (player == null) return;

        displayedHealth = Mathf.Lerp(displayedHealth, player.GetHealth(), Time.deltaTime * sliderSpeed);
        displayedExhaustion = Mathf.Lerp(displayedExhaustion, player.GetExhaustion(), Time.deltaTime * sliderSpeed);

        healthSlider.value = displayedHealth;
        exhaustionSlider.value = displayedExhaustion;
    }
    public void Setup(Character newPlayer)
    {
        player = newPlayer;
        healthSlider.maxValue = player.GetMaxHealth() * 2;
        healthSlider.value = player.GetMaxHealth();
        exhaustionSlider.maxValue = 2;
        exhaustionSlider.value = 0;
        playerCanvas.worldCamera = Camera.main;
    }
}