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

        /** The direction that this controller is attacking in. */
        public Vector2 AttackDirection { get; set; }

        [Tooltip("The light melee attack equipped by the character.")]
        [SerializeField] private MeleeAttackData lightAttack;
        [Tooltip("The heavy melee attack equipped by the character.")]
        [SerializeField] private MeleeAttackData heavyAttack;
        [Tooltip("The AoE attack equipped by the character.")]
        [SerializeField] private AoeAttackData aoeAttack;
        [Tooltip("The ranged projectile attack equipped by the character.")]
        [SerializeField] private ProjectileAttackData projectileAttack;
        
        [Tooltip("The layers that can be hit by attacks from this character.")]
        [SerializeField] private LayerMask targetLayers;

        [Tooltip("The collider to use when performing melee attacks. If not specified, a raycast will be used instead.")]
        [SerializeField] private MeleeCollider meleeCollider;

        [Tooltip("The audio source component used by this object.")]
        [SerializeField] private AudioSource audioSource;

        [Tooltip("An event triggered before this combat controller is hit with an attack.")]
        [SerializeField] private UnityEvent<AttackData, CombatController> onPreHit;

        [Tooltip("An event triggered after this combat controller is hit with an attack.")]
        [SerializeField] private UnityEvent<AttackData, CombatController> onPostHit;

        [Tooltip("An event triggered when this combat controller kills another.")]
        [SerializeField] private UnityEvent<AttackData, CombatController> onKill;

        [Tooltip("Can this combat controller receive attacks?")]
        [SerializeField] private bool canReceiveAttacks = true;

        /** Gets the layers targeted by this controller. */
        public LayerMask TargetLayers => targetLayers;

        /** The collider to use when performing melee attacks. If not specified, a raycast will be used instead. */
        public MeleeCollider MeleeCollider => meleeCollider;

        /** The light melee attack equipped by the character. */
        public MeleeAttackData LightAttack
        {
            get => lightAttack;
            set => lightAttack = value;
        }
        /** The heavy melee attack equipped by the character. */
        public MeleeAttackData HeavyAttack
        {
            get => heavyAttack;
            set => heavyAttack = value;
        }
        /** The AoE attack equipped by the character. */
        public AoeAttackData AoeAttack
        {
            get => aoeAttack;
            set => aoeAttack = value;
        }
        /** The ranged projectile attack equipped by the character. */
        public ProjectileAttackData ProjectileAttack
        {
            get => projectileAttack;
            set => projectileAttack = value;
        }

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

        /** the combat controller currently attacking this controller. */
        public CombatController CurrentAttacker { get; private set; }

        /** The animator used by this object. */
        private Animator _anim;

        /** The set of valid parameters for the associated animator. */
        private HashSet<string> _animValidParameters;

        /** Is the damage animation playing? */
        private bool _isAnimatingDamage;
        
        /** An event triggered before this combat controller is hit with an attack. */
        public event UnityAction<AttackData, CombatController> OnPreHit
        {
            add => onPreHit.AddListener(value);
            remove => onPreHit.RemoveListener(value);
        }
        
        /** An event triggered after this combat controller is hit with an attack. */
        public event UnityAction<AttackData, CombatController> OnPostHit
        {
            add => onPostHit.AddListener(value);
            remove => onPostHit.RemoveListener(value);
        }
        
        /** An event triggered when this combat controller kills another. Subscribers receive the killing attack and the killed controller. */
        public event UnityAction<AttackData, CombatController> OnKill
        {
            add => onKill.AddListener(value);
            remove => onKill.RemoveListener(value);
        }

        // Start is called before the first frame update
        protected void Start()
        {
            Character = GetComponent<Character>();

            _animValidParameters = new HashSet<string>();
            if (TryGetComponent(out _anim) && _anim.runtimeAnimatorController != null)
            {
                // Cache the parameters that the animator has for later checks
                foreach (AnimatorControllerParameter param in _anim.parameters)
                {
                    _animValidParameters.Add(param.name);
                }
            }
        }

        /**
         * Checks whether the animator parameter cache contains the provided parameter.
         * <param name="parameter">The parameter name to check for.</param>
         * <returns>True if the parameter exists.</returns>
         */
        protected bool AnimatorHasParameter(string parameter)
        {
            return _animValidParameters?.Contains(parameter) ?? false;
        }

        /**
         * Attempts to perform the stored light attack for this character.
         * <returns>True if the attack is available and could be performed.</returns>
         */
        public bool DoLightMeleeAttack()
        {
            return lightAttack != null && TryPerformAttack(lightAttack);
        }

        /**
         * Attempts to perform the stored heavy attack for this character.
         * <returns>True if the attack is available and could be performed.</returns>
         */
        public bool DoHeavyMeleeAttack()
        {
            return heavyAttack != null && TryPerformAttack(heavyAttack);
        }

        /**
         * Attempts to perform the stored AoE attack for this character.
         * <returns>True if the attack is available and could be performed.</returns>
         */
        public bool DoAreaOfEffectAttack()
        {
            return aoeAttack != null && TryPerformAttack(aoeAttack);
        }

        /**
         * Attempts to perform the stored ranged projectile attack for this character.
         * <returns>True if the attack is available and could be performed.</returns>
         */
        public bool DoProjectileAttack()
        {
            return projectileAttack != null && TryPerformAttack(projectileAttack);
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
            if (null == attack) return false;
            _currentAttack = attack;
            _currentHits = new List<Character>();
            _currentOnFinish = onFinish;
            _currentOnCancel = onCancel;
            _currentAttackCoroutine = attack.Execute(this, _currentHits,
                progress =>
                {
                    // Play the progress attack sound
                    if (audioSource != null && attack.ProgressSound != null)
                        audioSource.PlayOneShot(attack.ProgressSound);
                    onProgress?.Invoke(progress);
                },
                OnFinishAttack, PlayHitSound);
            
            // Play the animation if there is an animator attached that supports the trigger
            if (_anim != null && _anim.runtimeAnimatorController != null && AnimatorHasParameter(attack.AnimationTriggerName))
                _anim.SetTrigger(attack.AnimationTriggerName);
            // Play the start attack sound
            if (audioSource != null && attack.StartSound != null)
            {
                audioSource.PlayOneShot(attack.StartSound);
            }

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
            if (attack == null || instigator == null || !canReceiveAttacks || Character == null) return false;
            
            CurrentAttacker = instigator;
            onPreHit.Invoke(attack, instigator);

            void OnDeath()
            {
                instigator.onKill.Invoke(attack, this);
                if (instigator.Character == GameManager.GetPlayer())
                {
                    GameManager.GameState.KillCount++;
                }
            }
            
            // TODO: Make knockback work
            //Character.CharacterMovement.ApplyKnockback(instigator.AttackDirection);

            PlayDamageAnimation();

            Character.OnDeath += OnDeath;
            
            // Apply the effects from the attack
            foreach (EffectInstance instance in attack.Effects)
            {
                EffectInstance amplifiedByStatsInst = instance;
                // Apply attack power buff
                amplifiedByStatsInst.magnitude *= instigator.Character.Attributes.AttackPower.CurrentValue;
                // TODO: Apply receiver resistance
                Character.Attributes.ApplyEffect(instance, Character);
            }

            Character.OnDeath -= OnDeath;
            
            onPostHit.Invoke(attack, instigator);
            CurrentAttacker = null;

            return true;
        }

        /** The gradient applied to characters when they receive an attack. */
        private static readonly Gradient DamageGradient = new Gradient()
        {
            colorKeys = new []
            {
                new GradientColorKey(new Color(1, 1, 1), 0.0f),
                new GradientColorKey(new Color(1f, 0.5f, 0.4f), 0.1f),
                new GradientColorKey(new Color(1, 1, 1), 0.2f),
                new GradientColorKey(new Color(1f, 0.5f, 0.4f), 0.3f),
                new GradientColorKey(new Color(1, 1, 1), 0.5f),
                new GradientColorKey(new Color(1f, 0.5f, 0.4f), 0.7f),
                new GradientColorKey(new Color(1, 1, 1), 1.0f),
            },
            alphaKeys = new []
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        };
        
        /** Plays the damage coloration animation. */
        private void PlayDamageAnimation()
        {
            if (_isAnimatingDamage) return;
            _isAnimatingDamage = true;
            
            const float animationTime = 1.0f;
            const float animationInterval = 0.1f;
            const float progressInterval = animationInterval / animationTime;
            
            float progress = 0;
            
            Utility.DelayedFunction(this, animationTime, () => _isAnimatingDamage = false);
            Utility.RepeatFunctionUntil(this, () =>
            {
                progress += progressInterval;
                Character.SpriteRenderer.color = DamageGradient.Evaluate(progress);
            }, animationInterval, () => _isAnimatingDamage);
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

        /**
         * Plays the frame sound of the current attck if it exists. Meant to be called by an animation event.
         */
        public void PlayFrameSound()
        {
            if (audioSource != null && _currentAttack.FrameSound != null)
                audioSource.PlayOneShot(_currentAttack.FrameSound);
        }

        /**
         * Plays the hit sound of the current attck if it exists.
         */
        private void PlayHitSound()
        {
            if (audioSource != null && _currentAttack.HitSound != null)
                audioSource.PlayOneShot(_currentAttack.HitSound);
        }
    }
}
