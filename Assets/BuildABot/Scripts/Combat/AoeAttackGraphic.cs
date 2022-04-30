using UnityEngine;

namespace BuildABot
{
    public class AoeAttackGraphic : AttackGraphic<AoeAttackData>
    {

        /** The sprite renderer used by this graphic. */
        [SerializeField] private SpriteRenderer spriteRenderer;

        public override void Initialize(AoeAttackData attack)
        {
            base.Initialize(attack);
            if (spriteRenderer != null) spriteRenderer.color = attack.GraphicColorGradient.Evaluate(0);
        }

        public override void OnAttackProgress(float progress)
        {
            float radius = AttackData.AreaOverTime.Evaluate(progress) * AttackData.Radius;
            transform.localScale = new Vector3(radius, radius, 1);
            if (spriteRenderer != null) spriteRenderer.color = AttackData.GraphicColorGradient.Evaluate(progress);
        }

        public override void OnAttackFinish()
        {
            Destroy(gameObject);
        }
    }
}