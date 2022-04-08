﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BuildABot
{
    public class Projectile : MonoBehaviour
    {

        [Tooltip("Layers to target")]
        [SerializeField] private LayerMask targetLayers;

        [Tooltip("Layers to ignore")]
        [SerializeField] private LayerMask ignoreLayers;
        
        [Tooltip("The event triggered when this projectile hits a target.")]
        [SerializeField] private UnityEvent<CombatController> onHitCombatant;
        
        /** An event triggered when this projectile hits a target. */
        public event UnityAction<CombatController> OnHitCombatant
        {
            add => onHitCombatant.AddListener(value);
            remove => onHitCombatant.RemoveListener(value);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null) return;
            if ((targetLayers.value & (1 << other.gameObject.layer)) > 0)
            {
                CombatController target = other.gameObject.GetComponent<CombatController>();
                if (target == null) return;
                
                onHitCombatant.Invoke(target);
                Destroy(gameObject);
            }
            else if ((ignoreLayers.value & (1 << other.gameObject.layer)) == 0)
            {
                Destroy(gameObject);
            }
        }
    }
}