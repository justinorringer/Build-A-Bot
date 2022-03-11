using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
namespace BuildABot {
    public class Temperature : MonoBehaviour
    {
        [SerializeField] private Player bipy;
        [SerializeField] private TextMeshProUGUI temperature;

        void Awake()
        {
            UpdateTemperature();
        }
        public void UpdateTemperature()
        {
            temperature.text = $"Temp: {bipy.Attributes.Temperature.CurrentValue}";
        }
    }
}