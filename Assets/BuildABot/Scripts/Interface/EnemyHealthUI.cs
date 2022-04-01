using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BuildABot
{
    public class EnemyHealthUI : MonoBehaviour
    {

        [Tooltip("Reference to the bar that will grow with the enemy's temperature")]
        [SerializeField] private GameObject tempBar;

        /** Rect transform of the temperature bar*/
        private RectTransform _tempBarTransform;
        /** Reference to the enemy class using this UI */
        private Enemy _enemy;
        /** Attributes of the associated enemy */
        private CharacterAttributeSet _enemyAttributes;
        /** Cached width value of temperature bar rect transform */
        private float _tempBarWidth = 0.5f;
        /** Cached height value of temperature bar rect transform */
        private float _tempBarHeight = 0.1f;
        /** Cached Image component of temperature bar rect transform */
        private Image _tempBarImage;
        /** Cached base health value of enemy */
        private float _baseHealth;

        void Awake()
        {
            //Cache initial values
            _tempBarTransform = tempBar.GetComponent<RectTransform>();
            _tempBarWidth = _tempBarTransform.sizeDelta.x;
            _tempBarHeight = _tempBarTransform.sizeDelta.y;
            _tempBarImage = tempBar.GetComponent<Image>();
            _enemy = GetComponentInParent<Enemy>();
            _enemyAttributes = _enemy.Attributes;
        }

        void Start()
        {
            //Cache these values after attribute set has been initialized
            _baseHealth = _enemyAttributes.Temperature.CurrentValue;
            UpdateUI(_baseHealth);
        }

        void OnEnable()
        {
            //Subscribe to health change event
            _enemyAttributes.Temperature.AddPostValueChangeListener(UpdateUI);
        }

        void OnDisable()
        {
            //Unsubscribe from health change event
            _enemyAttributes.Temperature.RemovePostValueChangeListener(UpdateUI);
        }

        private void UpdateUI(float newValue)
        {
            //Calculate the percentage of an enemy's heat from its base and maximum values
            float healthPercent = (newValue - _baseHealth) / (_enemyAttributes.MaxTemperature.CurrentValue - _baseHealth);

            //Edge case: if the percentage goes negative, turn the bar blue
            _tempBarImage.color = healthPercent < 0 ? Color.blue : Color.red;

            //Adjust the size of the temperature bar based on the percentage of enemy's heat
            _tempBarTransform.sizeDelta = new Vector2(healthPercent * _tempBarWidth, _tempBarHeight);
        }

    }
}