using UnityEngine;
using Cinemachine;

namespace BuildABot
{
    public class CameraController : MonoBehaviour
    {
        /** Game object containing main camera and cinemachine cameras being moved by this script */
        [SerializeField] private GameObject cameraObj;
        /** Camera being controlled by this script */
        [SerializeField] private Camera camera;
        /** Virtual camera to the left of the player */
        [SerializeField] private CinemachineVirtualCamera cameraLeft;
        /** Virtual camera to the right of the player */
        [SerializeField] private CinemachineVirtualCamera cameraRight;
        
        /** Maximum distance the character can look up and down */
        [SerializeField] private float maxLookDist;
        /** Speed the camera moves when looking up and down */
        [SerializeField] private float lookSpeed;
        /** Current offset of camera position caused by player looking */
        private Vector3 _lookOffset = Vector3.zero;

        /** Movement script for this game object */
        private CharacterMovement _mov;

        /** Rigidbody of the object */
        private Rigidbody2D _rigidbody;

        /** Initial zoom of the camera */
        private float _defaultZoom;

        /** Player script */
        private Player _player;

        private void Start()
        {
            _mov = GetComponent<CharacterMovement>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _player = GetComponent<Player>();

            _defaultZoom = camera.orthographicSize;
        }

        // Update is called once per frame
        void Update()
        {
            if(_mov.Facing.x > 0 && cameraLeft.Priority > cameraRight.Priority)
            {
                cameraLeft.Priority--;
                cameraRight.Priority++;
            }
            else if (_mov.Facing.x < 0 && cameraLeft.Priority < cameraRight.Priority)
            {
                cameraRight.Priority--;
                cameraLeft.Priority++;
            }
        }

        // Move the camera based on mouse movement if the player is stationary. Input vector is assumed to be normalized.
        public void CameraLook(Vector2 mouseDir)
        {
            if (_mov.IsGrounded && _rigidbody.velocity == Vector2.zero && mouseDir != Vector2.zero)
            {
                Vector3 offset = mouseDir * lookSpeed * Time.deltaTime;
                Vector3 look = _lookOffset + offset;
                if (look.magnitude > maxLookDist)
                {
                    offset = mouseDir * (maxLookDist - _lookOffset.magnitude);
                    look = Vector3.ClampMagnitude(look, maxLookDist);
                }
                cameraObj.transform.position += offset;
                _lookOffset = look;
            }
            else
            {
                Vector3 offset = _lookOffset.normalized * lookSpeed * Time.deltaTime;
                if (offset.magnitude > _lookOffset.magnitude)
                {
                    offset = _lookOffset;
                }
                cameraObj.transform.position -= offset;
                _lookOffset -= offset;
            }
        }

        // When the view distance attribute is changed, this function will be called to update the camera's size
        public void UpdateCameraZoom()
        {
            camera.orthographicSize = _defaultZoom * _player.Attributes.ViewDistance.BaseValue;
        }

    }
}