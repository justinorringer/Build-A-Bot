using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildABot
{
    
    /**
     * A melee attack that uses box-casts extending from the attacker.
     */
    [CreateAssetMenu(fileName = "NewMeleeAttack", menuName = "Build-A-Bot/Combat/Melee Attack", order = 1)]
    public class MeleeAttackData : AttackData
    {

        [Header("Melee")]
        
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

        /** The distance the attack travels. */
        public float Distance => distance;

        /** Is the distance of this attack relative to the bounds of the attacking character? */
        public bool UseRelativeDistance => useRelativeDistance;

        /** Duration of attack in seconds. */
        public float Duration => duration;

        /** The number of times to raycast the attack per second. */
        public int RaycastRate => raycastRate;

        public override IEnumerator Execute(CombatController instigator, List<Character> hits, Action<float> onProgress = null, Action onComplete = null)
        {
            if (!AllowMovement) instigator.Character.CharacterMovement.CanMove = false;
            
            HashSet<Character> hitLookup = new HashSet<Character>();
            float progress = 0f;
            float interval = 1f / raycastRate;
            float progressInterval = duration == 0f ? 0f : interval / duration;
            
            return Utility.RepeatFunction(instigator, () =>
            {
                Vector2 position = instigator.Character.transform.position;
                position += (OffsetInLookDirection ? instigator.Character.CharacterMovement.Facing : Vector2.one) *
                            Offset * new Vector2(OffsetXCurve.Evaluate(progress), OffsetYCurve.Evaluate(progress));
                
                Vector2 trueSize = UseRelativeSize ? instigator.Character.Bounds * Size : Size;
                float dist = useRelativeDistance ? distance * instigator.Character.Bounds.x : distance;
                
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
                    Character other = hitObj.GetComponent<Character>();
                    if ((CanHitSelf || other != instigator.Character) &&
                        null != other &&
                        (AllowMultiHit || !hitLookup.Contains(other)))
                    {
                        foreach (EffectInstance instance in Effects)
                        {
                            other.Attributes.ApplyEffect(instance, other);
                        }
                        hits.Add(other);
                        hitLookup.Add(other);
                    }
                }

                progress += progressInterval;
                onProgress?.Invoke(progress);
                
            }, interval, (int) (raycastRate * duration), () =>
            {
                if (!AllowMovement) instigator.Character.CharacterMovement.CanMove = true;
                onComplete?.Invoke();
            });
        }
    }
}