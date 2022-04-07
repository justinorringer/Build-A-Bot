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

        [Tooltip("Seconds between firing a bullet")] 
        [SerializeField] private float fireRate;

        [Tooltip("Number of seconds during which bullets should be fired")] 
        [SerializeField] private float duration;

        [Tooltip("Prefab of the projectile that should be fired")]
        [SerializeField] private GameObject bulletPrefab;

        public Transform Target { get; set; }

        public override IEnumerator Execute(CombatController instigator, List<Character> hits, Action<float> onProgress = null, Action onComplete = null)
        {
            if (!AllowMovement) instigator.Character.CharacterMovement.CanMove = false;
            
            float progress = 0f;
            float interval = 1f / fireRate;
            float progressInterval = duration == 0f ? 0f : interval / duration;
            
            return Utility.RepeatFunction(instigator, () =>
            {
                Vector3 localPosition = instigator.transform.position;
                
                //Spawn prefab
                GameObject bullet = Instantiate(bulletPrefab, localPosition, Quaternion.identity);

                //Give the projectile velocity
                Vector3 direction = (Target.position - localPosition).normalized;
                bullet.GetComponent<Rigidbody2D>().AddForce(direction * bulletSpeed, ForceMode2D.Impulse);
                
                //Set destruction timer on projectile
                Destroy(bullet, bulletLifetime);
                
                //Update progress
                progress += progressInterval;
                onProgress?.Invoke(progress);

            }, interval, (int) (fireRate * duration), () =>
            {
                if (!AllowMovement) instigator.Character.CharacterMovement.CanMove = true;
                onComplete?.Invoke();
            });

            return null;
        }
    }
}