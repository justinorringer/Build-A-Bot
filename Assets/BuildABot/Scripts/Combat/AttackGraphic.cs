using System.Collections.Generic;
using UnityEngine;

namespace BuildABot
{
    public abstract class AttackGraphic<TAttackData> : MonoBehaviour where TAttackData : AttackData
    {

        /** The attack data bound to this graphic. */
        private TAttackData _attackData;

        /** The attack data bound to this graphic. */
        protected TAttackData AttackData => _attackData;

        /**
         * Initializes this attack graphic using the provided attack data.
         * <param name="attack">The attack to bind to this graphic.</param>
         */
        public virtual void Initialize(TAttackData attack)
        {
            if (attack == null || _attackData != null) return;
            _attackData = attack;
        }

        /**
         * A handler function called any time the attack progress changes.
         * <param name="progress">The new progress value.</param>
         */
        public abstract void OnAttackProgress(float progress);

        /**
         * A handler function called when the bound attack finishes.
         */
        public abstract void OnAttackFinish();
    }
}