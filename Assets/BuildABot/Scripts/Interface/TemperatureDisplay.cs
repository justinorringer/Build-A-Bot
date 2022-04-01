using UnityEngine;
using TMPro;

namespace BuildABot {
    public class TemperatureDisplay : MonoBehaviour
    {
        [SerializeField] private HUD hud;
        [SerializeField] private TextMeshProUGUI temperature;

        void Start()
        {
            UpdateTemperature();
        }
        
        public void UpdateTemperature()
        {
            temperature.text = $"Temp: {hud.Player.Attributes.Temperature.CurrentValue}";
        }
    }
}