using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace BuildABot {
    public class TemperatureDisplay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HUD hud;
        [SerializeField] private Slider thermostat;
        [SerializeField] private TextMeshProUGUI temperature;
        
        [Header("Properties")]
        [SerializeField] private Gradient colorGradient;

        [SerializeField] private float flashingInterval = 0.5f;
        [SerializeField] private float flashingThresholdMin = 0.1f;
        [SerializeField] private float flashingThresholdMax = 0.9f;

        private IEnumerator _flashingTempTask;

        private Color _latestColor;
        private float _latestValue;
        private bool _isFlashed;

        void Awake()
        {
            if (!hud.Player.Attributes.Initialized)
            {
                hud.Player.Attributes.OnInitialize += UpdateTemperature;
            }
            else
            {
                UpdateTemperature();
            }
        }
        
        public void UpdateTemperature()
        {
            if (hud.Player.Attributes.Initialized)
            {
                thermostat.minValue = hud.Player.Attributes.MinTemperature.CurrentValue;
                thermostat.maxValue = hud.Player.Attributes.MaxTemperature.CurrentValue;
                float temp = hud.Player.Attributes.Temperature.CurrentValue;
                thermostat.value = temp;
                _latestValue = (thermostat.value - thermostat.minValue) / (thermostat.maxValue - thermostat.minValue);
                _latestColor = colorGradient.Evaluate(_latestValue);
                thermostat.image.color = _latestColor;
                temperature.text = $"{temp:0.0} C";

                // Make the temperature flash if in a dangerous position
                if (ShouldFlash() && _flashingTempTask == null)
                {
                    _flashingTempTask = Utility.RepeatFunctionUntil(this, FlashTemperature, flashingInterval,
                        ShouldFlash,
                        () =>
                        {
                            temperature.color = Color.white;
                            _isFlashed = false;
                            _flashingTempTask = null;
                        });
                }
            }
        }

        private bool ShouldFlash()
        {
            return _latestValue >= flashingThresholdMax || _latestValue <= flashingThresholdMin;
        }

        private void FlashTemperature()
        {
            _isFlashed = !_isFlashed;
            if (_isFlashed)
            {
                temperature.color = _latestColor;
            }
            else
            {
                temperature.color = Color.white;
            }
        }
    }
}