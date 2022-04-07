using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BuildABot
{
    /**
     * The component used to drive player attacks.
     */
    public class CombatController : MonoBehaviour
    {

        /** A reference to the player instance using this component. */
        public Character Character { get; private set; }

        [SerializeField] private AttackData storedAttack; // TODO: Remove, only use TryPerformAttack or have a labelled list
        
        [Tooltip("The layers that can be hit by attacks from this character.")]
        [SerializeField] private LayerMask targetLayers;

        [Tooltip("An event triggered when this combat controller is hit with an attack.")]
        [SerializeField] private UnityEvent<AttackData, CombatController> onHit;

        [Tooltip("Can this combat controller receive attacks?")]
        [SerializeField] private bool canReceiveAttacks = true;

        /** Gets the layers targeted by this controller. */
        public LayerMask TargetLayers => targetLayers;

        /** The IEnumerator representing the current attack's coroutine. */
        private IEnumerator _currentAttackCoroutine;
        /** The attack currently being executed. */
        private AttackData _currentAttack;
        /** The hits generated by the current attack. */
        private List<Character> _currentHits;
        /** The current on finish action. */
        private Action<List<Character>> _currentOnFinish;
        /** The current on cancel action. */
        private Action<List<Character>> _currentOnCancel;

        /** The animator used by this object. */
        private Animator _anim;
        /** Hash of "attack" parameter in the animator, stored for optimization */
        private int _attackTriggerHash;
        
        /** An event triggered when this combat controller is hit with an attack. */
        public event UnityAction<AttackData, CombatController> OnHit
        {
            add => onHit.AddListener(value);
            remove => onHit.RemoveListener(value);
        }

        // Start is called before the first frame update
        protected void Start()
        {
            Character = GetComponent<Character>();

            _anim = GetComponent<Animator>();
            // TODO: Handle different attacks by specifying a trigger string in the attack data
            _attackTriggerHash = Animator.StringToHash("Attack");
        }

        public void DoStoredAttack()
        {
            TryPerformAttack(storedAttack);
        }

        /**
         * Performs the specified attack.
         * <param name="attack">The attack to execute.</param>
         * <param name="onProgress">An optional function to call as the attack progresses.</param>
         * <param name="onFinish">An optional function to call when this attack finishes.</param>
         * <param name="onCancel">An optional function to call if this attack is cancelled.</param>
         * <returns>True if the attack could be started.</returns>
         */
        public bool TryPerformAttack(AttackData attack, Action<float> onProgress = null, Action<List<Character>> onFinish = null, Action<List<Character>> onCancel = null)
        {
            if (null != _currentAttack) return false;
            _currentAttack = attack;
            _currentHits = new List<Character>();
            _currentOnFinish = onFinish;
            _currentOnCancel = onCancel;
            _currentAttackCoroutine = attack.Execute(this, _currentHits, onProgress, OnFinishAttack);
            if (_anim != null && _anim.runtimeAnimatorController != null) _anim.SetTrigger(_attackTriggerHash);
            return true;
        }

        /**
         * Attempts to cancel the currently executing attack if it can be interrupted.
         * <returns>True if the attack could be cancelled.</returns>
         */
        public bool TryCancelAttack()
        {
            bool canCancel = null != _currentAttack && _currentAttack.TryCancel(this, _currentAttackCoroutine);
            if (canCancel)
            {
                _currentOnCancel?.Invoke(_currentHits);
                CleanUpCurrentAttack();
            }

            return canCancel;
        }

        /**
         * Attempts to apply the provided attack to this character as if sent by the given instigator.
         * <param name="attack">The attack to apply.</param>
         * <param name="instigator">The combat controller that instigated the attack.</param>
         * <returns>True if the attack was successfully applied.</returns>
         */
        public bool TryReceiveAttack(AttackData attack, CombatController instigator)
        {
            if (attack == null || instigator == null || !canReceiveAttacks) return false;
            
            // Apply the effects from the attack
            foreach (EffectInstance instance in attack.Effects)
            {
                Character.Attributes.ApplyEffect(instance, Character);
            }
            
            onHit.Invoke(attack, instigator);

            return true;
        }
        
        

        /**
         * Called whenever the current attack is finished.
         */
        private void OnFinishAttack()
        {
            _currentOnFinish?.Invoke(_currentHits);
            CleanUpCurrentAttack();
        }

        /**
         * Cleans up the cache values used for the current attack.
         */
        private void CleanUpCurrentAttack()
        {
            _currentAttackCoroutine = null;
            _currentAttack = null;
            _currentHits = null;
            _currentOnFinish = null;
            _currentOnCancel = null;
        }
    }
}
