using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildABot
{
    [CreateAssetMenu(fileName = "NewProjectileAttack", menuName = "Build-A-Bot/Combat/Projectile Attack", order = 3)]
    public class ProjectileAttackData : AttackData
    {

        public override IEnumerator Execute(CombatController instigator, List<Character> hits, Action<float> onProgress = null, Action onComplete = null)
        {
            if (!allowMovement) instigator.Character.CharacterMovement.CanMove = false;
            return null;
        }
    }
}