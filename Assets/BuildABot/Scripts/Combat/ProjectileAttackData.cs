using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildABot
{
    [CreateAssetMenu(fileName = "NewProjectileAttack", menuName = "Build-A-Bot/Combat/Projectile Attack", order = 3)]
    public class ProjectileAttackData : AttackData
    {
        [Header("Projectile Information")]
        
        [Tooltip("Amount of time before the bullet will be destroyed")]
        [SerializeField] private float bulletLifetime;

        [Tooltip("Speed at which bullets move")]
        [SerializeField] private float bulletSpeed;

        [Tooltip("The number of bullets fired in total.")] 
        [SerializeField] private int bulletCount;

        [Tooltip("The interval between bullets being shot.")] 
        [SerializeField] private float fireRate;

        [Tooltip("Prefab of the projectile that should be fired")]
        [SerializeField] private Projectile bulletPrefab;

        public override IEnumerator Execute(CombatController instigator, List<Character> hits, Action<float> onProgress = null, Action onComplete = null)
        {
            if (!AllowMovement) instigator.Character.CharacterMovement.CanMove = false;
            
            HashSet<Character> hitLookup = new HashSet<Character>();
            float progress = 0f;
            float interval = fireRate;
            float duration = bulletCount * fireRate;
            float progressInterval = duration == 0f ? 0f : interval / duration;

            void HandleHit(CombatController other)
            {
                if (null != other && null != other.Character &&
                    (CanHitSelf || other.Character != instigator.Character) &&
                    (AllowMultiHit || !hitLookup.Contains(other.Character)))
                {
                    if (other.TryReceiveAttack(this, instigator))
                    {
                        hits.Add(other.Character);
                        hitLookup.Add(other.Character);
                    }
                }
            }

            void SpawnProjectile(Vector3 position)
            {
                //Spawn prefab
                Projectile bullet = Instantiate(bulletPrefab, position, Quaternion.identity);

                bullet.OnHitCombatant += HandleHit;
                
                //Set destruction timer on projectile
                Utility.DelayedFunction(bullet, bulletLifetime, () =>
                {
                    bullet.OnHitCombatant -= HandleHit;
                    Destroy(bullet.gameObject);
                });
                
                //Give the projectile velocity
                if (instigator != null)
                {
                    bullet.GetComponent<Rigidbody2D>().AddForce(instigator.AttackDirection * bulletSpeed, ForceMode2D.Impulse);
                }
                else
                {
                    Destroy(bullet.gameObject);
                }
            }
            
            return Utility.RepeatFunction(instigator, () =>
            {
                Vector3 localPosition = instigator.transform.position;

                SpawnProjectile(localPosition);
                
                //Update progress
                progress += progressInterval;
                onProgress?.Invoke(progress);

            }, interval, bulletCount, () =>
            {
                if (!AllowMovement) instigator.Character.CharacterMovement.CanMove = true;
                onComplete?.Invoke();
            });
        }
    }
}