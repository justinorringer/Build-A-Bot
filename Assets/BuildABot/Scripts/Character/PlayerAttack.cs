using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildABot
{
    public class PlayerAttack : MonoBehaviour
    {
        /** Size of the attack */
        [SerializeField] private Vector2 attackSize;

        /** Duration of attack in milliseconds */
        [SerializeField] private int attackDuration;

        /** Distance between the character and the created attack */
        private Vector2 _offset;

        /** Player's movement component */
        private PlayerMovement _playerMovement;

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
                RaycastHit2D hitInfo = Physics2D.BoxCast(position + (_offset * _playerMovement.Facing), attackSize, 0, _playerMovement.Facing, 10.0f);
                Debug.DrawRay(position + (_offset * _playerMovement.Facing), _playerMovement.Facing, Color.red, 10.0f);
                if (hitInfo) {
                    GameObject hitObj = hitInfo.collider.gameObject;

                    if(hitObj.tag.Equals("Enemy"))
                    {
                        // TODO do damage to the enemy using attribute system
                    }
                }
                yield return new WaitForSeconds(.001f);
            }
        }
    }
}
