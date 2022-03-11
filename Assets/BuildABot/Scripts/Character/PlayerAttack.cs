using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildABot
{
    public class PlayerAttack : MonoBehaviour
    {
        /** Size of the attack box */
        [SerializeField] private Vector2 attackSize;

        /** Distance the attack travels */
        [SerializeField] private float attackDist;

        /** Duration of attack in milliseconds */
        [SerializeField] private int attackDuration;

        /** Distance between the character and the created attack */
        private Vector2 _offset;

        /** Player's movement component */
        private PlayerMovement _playerMovement;

        /** Effect applied when an attack hits */
        [SerializeField] Effect damageEffect;

        /** Magnitude of damage done */
        [Min(0.0f)]
        [SerializeField] float damageValue = 1.0f;

        // Start is called before the first frame update
        void Start()
        {
            _offset = new Vector2(GetComponent<Collider2D>().bounds.extents.x, 0);
            _playerMovement = GetComponent<PlayerMovement>();
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        /** Coroutine to perform an attack which lasts for a number of milliseconds determined by attackDuration */
        public IEnumerator Attack()
        {
            for (int i = 0; i < attackDuration; i++)
            {
                Vector2 position = transform.position;
                RaycastHit2D hitInfo = Physics2D.BoxCast(position + (_offset * _playerMovement.Facing), attackSize, 0, _playerMovement.Facing, attackDist, LayerMask.GetMask("Enemy"));
                Debug.DrawRay(position + (_offset * _playerMovement.Facing), _playerMovement.Facing * attackDist, Color.red, 1.0f);
                if (hitInfo) {
                    GameObject hitObj = hitInfo.collider.gameObject;

                    Enemy enemy = hitObj.GetComponent<Enemy>();
                    if (enemy != null && damageEffect != null)
                    {
                        enemy.Attributes.ApplyEffect(damageEffect, enemy, damageValue);
                    }

                    yield break;
                }
                yield return new WaitForSeconds(.001f);
            }
        }
    }
}
