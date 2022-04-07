using System;
using System.Collections.Generic;
using UnityEngine;

namespace BuildABot
{
    public class Projectile : MonoBehaviour
    {
        [Tooltip("Effect to apply on hitting a character")]
        [SerializeField] private List<EffectInstance> hitEffects;

        [Tooltip("Layer to target")]
        [SerializeField] private LayerMask targetLayer;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((targetLayer.value & (1 << other.gameObject.layer)) > 0)
            {
                Character otherChar = other.gameObject.GetComponent<Character>();
                
                foreach (EffectInstance instance in hitEffects)
                {
                    otherChar.Attributes.ApplyEffect(instance, otherChar);
                }
                
                Destroy(gameObject);
            }
        }
    }
}