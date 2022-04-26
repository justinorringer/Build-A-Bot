using System;
using UnityEngine;
using UnityEngine.Events;

namespace BuildABot
{
    public class MeleeCollider : MonoBehaviour
    {
        [Tooltip("An event fired any time this collider overlaps a combat controller.")]
        [SerializeField] private UnityEvent<CombatController> onHit;

        [Tooltip("The combat controller in charge of this collider.")]
        [SerializeField] private CombatController controller;

        /**
         * An event called whenever this object hits another object with a combat controller.
         */
        public event UnityAction<CombatController> OnHit
        {
            add => onHit.AddListener(value);
            remove => onHit.RemoveListener(value);
        }

        protected void Awake()
        {
            gameObject.SetActive(false);
        }

        protected void Update()
        {
            Vector2 facing = controller.Character.CharacterMovement.Facing;
            transform.localScale = new Vector3(facing.x, 1, 1);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other != null && other.TryGetComponent(out CombatController hit) && 
                controller.TargetLayers == (controller.TargetLayers | (1 << other.gameObject.layer)))
            {
                onHit.Invoke(hit);
            }
        }
    }
}