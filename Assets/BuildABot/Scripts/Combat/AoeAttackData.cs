using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildABot
{

    public enum EAoeAttackShape
    {
        Box,
        Circle,
        Cone
    }
    
    [CreateAssetMenu(fileName = "NewAreaAttack", menuName = "Build-A-Bot/Combat/AOE Attack", order = 2)]
    public class AoeAttackData : AttackData
    {
        [Header("AOE")]
        
        [Tooltip("The shape of the attack box.")]
        [SerializeField] private EAoeAttackShape shape = EAoeAttackShape.Circle;

        [Tooltip("Is the size of this attack relative to the bounds of the attacking character?")]
        [SerializeField] private bool useRelativeRadius;
        
        [Tooltip("The maximum radius of the attack area.")]
        [Min(0f)]
        [SerializeField] private float radius = 1f;

        [Tooltip("The curve that the attack area will follow over time.")]
        [SerializeField] private AnimationCurve areaOverTime = AnimationCurve.Constant(0, 1, 1);
        
        [Tooltip("The maximum angle used if this attack uses a cone area.")]
        [Range(0f, 180f)]
        [SerializeField] private float angle = 30f;

        [Tooltip("The curve that the attack area will follow over time.")]
        [SerializeField] private AnimationCurve angleOverTime = AnimationCurve.Constant(0, 1, 1);
        
        [Tooltip("The duration of this attack in seconds.")]
        [Min(0f)]
        [SerializeField] private float duration;

        [Tooltip("The number of times to raycast the attack per second.")]
        [Min(1)]
        [SerializeField] private int raycastRate = 10;

        public override IEnumerator Execute(CombatController instigator, List<Character> hits, Action<float> onProgress = null, Action onComplete = null)
        {
            if (!allowMovement) instigator.Character.CharacterMovement.CanMove = false;
            
            HashSet<Character> hitLookup = new HashSet<Character>();
            float progress = 0f;
            float interval = 1f / raycastRate;
            float progressInterval = duration == 0f ? 1f : interval / duration;
            
            return Utility.RepeatFunction(instigator, () =>
            {
                Vector2 position = instigator.Character.transform.position;
                position += (offsetInLookDirection ? instigator.Character.CharacterMovement.Facing : Vector2.one) *
                            offset * new Vector2(offsetXCurve.Evaluate(progress), offsetYCurve.Evaluate(progress));

                float trueRadius = radius;
                
                Collider2D[] hitColliders;
                switch (shape)
                {
                    case EAoeAttackShape.Box:
                
                        Vector2 trueSize = useRelativeSize ? instigator.Character.Bounds * size : size;
                        trueSize *= areaOverTime.Evaluate(progress);
                        
                        hitColliders = Physics2D.OverlapBoxAll(
                            position,
                            trueSize,
                            0,
                            instigator.TargetLayers);
                        DebugUtility.DrawBox2D(position, trueSize, 0, 0, Color.red, duration * (1 - progress));
                        break;
                    case EAoeAttackShape.Circle:
                
                        if (useRelativeRadius) trueRadius *= instigator.Character.Bounds.magnitude * 0.5f;
                        trueRadius *= areaOverTime.Evaluate(progress);
                        
                        hitColliders = Physics2D.OverlapCircleAll(
                            position ,
                            trueRadius, instigator.TargetLayers);
                        
                        DebugUtility.DrawCircle2D(position, trueRadius, 0, 16, Color.red, duration * (1 - progress));
                        break;
                    case EAoeAttackShape.Cone:
                
                        if (useRelativeRadius) trueRadius *= instigator.Character.Bounds.magnitude * 0.5f;
                        trueRadius *= areaOverTime.Evaluate(progress);

                        float trueAngle = angle * angleOverTime.Evaluate(progress);
                        
                        hitColliders = Physics2D.OverlapCircleAll(
                            position,
                            trueRadius, instigator.TargetLayers);
                        
                        DebugUtility.DrawCircle2D(position, trueRadius, 0, 16, Color.red, duration * (1 - progress));
                        
                        break;
                    default:
                        hitColliders = new Collider2D[0];
                        break;
                }

                foreach (Collider2D hit in hitColliders)
                {
                    GameObject hitObj = hit.gameObject;

                    // Get the other character hit
                    Character other = hitObj.GetComponent<Character>();
                    if ((canHitSelf || other != instigator.Character) &&
                        null != other &&
                        (allowMultiHit || !hitLookup.Contains(other)))
                    {
                        foreach (EffectInstance instance in effects)
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
                if (!allowMovement) instigator.Character.CharacterMovement.CanMove = true;
                onComplete?.Invoke();
            });
        }
    }
}