using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BuildABot
{
    public class AutoSelectOnHover : MonoBehaviour, IPointerEnterHandler
    {
        [Tooltip("The target to select when this component is hovered.")]
        [SerializeField] private Selectable target;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (target != null) target.Select();
        }
    }
}