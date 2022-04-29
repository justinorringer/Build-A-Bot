using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildABot
{

    /**
     * The type of a melee attack, either light or heavy.
     */
    public enum EMeleeAttackType
    {
        Light,
        Heavy
    }
    
    /**
     * A melee attack that uses box-casts extending from the attacker.
     */
    [CreateAssetMenu(fileName = "NewMeleeAttack", menuName = "Build-A-Bot/Combat/Melee Attack", order = 1)]
    public class MeleeAttackData : AttackData
    {

        [Header("Melee")]
        
        [Tooltip("The type of this melee attack.")]
        [SerializeField] private EMeleeAttackType attackType = EMeleeAttackType.Light;

        [Tooltip("The distance the attack travels.")]
        [Min(0f)]
        [SerializeField] private float distance = 1f;

        [Tooltip("Is the distance of this attack relative to the bounds of the attacking character?")]
        [SerializeField] private bool useRelativeDistance = true;
        
        [Tooltip("Duration of attack in seconds.")]
        [Min(0f)]
        [SerializeField] private float duration;

        [Tooltip("The number of times to raycast the attack per second.")]
        [Min(1)]
        [SerializeField] private int raycastRate = 10;

        /** The type of this melee attack. */
        public EMeleeAttackType AttackType => attackType;

        /** The distance the attack travels. */
        public float Distance => distance;

        /** Is the distance of this attack relative to the bounds of the attacking character? */
        public bool UseRelativeDistance => useRelativeDistance;

        /** Duration of attack in seconds. */
        public float Duration => duration;

        /** The number of times to raycast the attack per second. */
        public int RaycastRate => raycastRate;

        public override IEnumerator Execute(CombatController instigator, List<Character> hits, Action<float> onProgress = null, Action onComplete = null, Action onHit = null)
        {
            if (!AllowMovement) instigator.Character.CharacterMovement.CanMove = false;
            
            HashSet<Character> hitLookup = new HashSet<Character>();
            float progress = 0f;
            float interval = 1f / raycastRate;
            float progressInterval = duration == 0f ? 0f : interval / duration;

            void ProcessHit(CombatController other)
            {
                if (null != other && null != other.Character &&
                    (CanHitSelf || other.Character != instigator.Character) &&
                    (AllowMultiHit || !hitLookup.Contains(other.Character)))
                {
                    if (other.TryReceiveAttack(this, instigator))
                    {
                        hits.Add(other.Character);
                        hitLookup.Add(other.Character);
                        onHit?.Invoke();
                    }
                }
            }

            bool useCollider = false;

            if (instigator.MeleeCollider != null)
            {
                useCollider = true;
                instigator.MeleeCollider.OnHit += ProcessHit;
            }
            
            return Utility.RepeatFunction(instigator, () =>
            {
                Vector2 position = instigator.Character.transform.position;
                position += (OffsetInLookDirection ? instigator.Character.CharacterMovement.Facing : Vector2.one) *
                            Offset * new Vector2(OffsetXCurve.Evaluate(progress), OffsetYCurve.Evaluate(progress));
                
                Vector2 trueSize = UseRelativeSize ? instigator.Character.Bounds * Size : Size;
                float dist = useRelativeDistance ? distance * instigator.Character.Bounds.x : distance;

                if (!useCollider)
                {
                    RaycastHit2D[] hitInfoAll = Physics2D.BoxCastAll(
                        position,
                        trueSize,
                        0,
                        instigator.Character.CharacterMovement.Facing,
                        dist,
                        instigator.TargetLayers);
                
                    DebugUtility.DrawBoxCast2D(position, trueSize, 0.0f, 
                        instigator.Character.CharacterMovement.Facing,
                        dist, Color.red, duration * (1 - progress));

                    foreach (RaycastHit2D hitInfo in hitInfoAll)
                    {
                        GameObject hitObj = hitInfo.collider.gameObject;

                        // Get the other character hit
                        CombatController other = hitObj.GetComponent<CombatController>();
                        ProcessHit(other);
                    }
                }

                progress += progressInterval;
                onProgress?.Invoke(progress);
                
            }, interval, (int) (raycastRate * duration), () =>
            {
                if (useCollider)
                {
                    instigator.MeleeCollider.OnHit -= ProcessHit;
                }
                if (!AllowMovement) instigator.Character.CharacterMovement.CanMove = true;
                onComplete?.Invoke();
            });
        }
    }
}