using UnityEngine;
using Cinemachine;

namespace BuildABot
{
    public class CameraController : MonoBehaviour
    {
        /** Game object containing main camera and cinemachine cameras being moved by this script */
        [SerializeField] private GameObject cameraObj;
        /** Camera being controlled by this script */
        [SerializeField] private Camera mainCamera;
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

        /** Maximum distance the character can zoom out, as a multiplier of current base camera zoom */
        [SerializeField] private float maxZoom;
        /** Speed the camera zooms out */
        [SerializeField] private float zoomSpeed;
        /** Difference between base zoom and current zoom */
        private float _zoomDiff = 0;
        /** Current base zoom of camera */
        private float _baseZoom;

        /** Movement script for this game object */
        private CharacterMovement _mov;

        /** Initial zoom of the camera */
        private float _defaultZoom;

        /** Player script */
        private Player _player;

        /** Time the camera waits after there is no more input for camera look before returning */
        [SerializeField] private float lookTime = .1f;

        /** Time since the camera has stopped recieving input */
        private float _lookTimer = 0;

        private void Start()
        {
            _mov = GetComponent<CharacterMovement>();
            _player = GetComponent<Player>();

            _defaultZoom = cameraLeft.m_Lens.OrthographicSize;
            _baseZoom = _defaultZoom;
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
            if (_mov.IsGrounded && !_mov.InMotion && mouseDir != Vector2.zero)
            {
                _lookTimer = 0;
                
                Vector3 offset = mouseDir * lookSpeed * Time.deltaTime;
                Vector3 look = _lookOffset + offset;
                if (look.magnitude > maxLookDist)
                {
                    offset = mouseDir * (maxLookDist - _lookOffset.magnitude);
                    look = _lookOffset + offset;
                }
                cameraObj.transform.position += offset;
                _lookOffset = look;
            }
            else
            {
                if (_lookTimer >= lookTime || _mov.InMotion || !_mov.IsGrounded)
                {
                    Vector3 offset = _lookOffset.normalized * lookSpeed * 1.5f * Time.deltaTime;
                    if (offset.magnitude > _lookOffset.magnitude)
                    {
                        offset = _lookOffset;
                    }
                    cameraObj.transform.position -= offset;
                    _lookOffset -= offset;
                }
                else
                {
                    _lookTimer += Time.deltaTime;
                }
            }
        }

        public void ZoomOut(bool zooming)
        {
            if (_mov.IsGrounded && !_mov.InMotion && zooming)
            {
                float zoom = Mathf.Min(zoomSpeed * Time.deltaTime, (maxZoom * _baseZoom) - (_baseZoom + _zoomDiff));
                AddCameraSize(zoom);
                _zoomDiff += zoom;
            }
            else if (_zoomDiff != 0)
            {
                float zoom = Mathf.Min(zoomSpeed * 2 * Time.deltaTime, _zoomDiff);
                AddCameraSize(-zoom);
                _zoomDiff -= zoom;
            }
        }

        // When the view distance attribute is changed, this function will be called to update the camera's size
        public void UpdateCameraZoom()
        {
            float newZoom = _defaultZoom * _player.Attributes.ViewDistance.CurrentValue;
            SetCameraSize(newZoom);
            _baseZoom = newZoom;
        }

        private void SetCameraSize(float newSize)
        {
            cameraLeft.m_Lens.OrthographicSize = newSize;
            cameraRight.m_Lens.OrthographicSize = newSize;
        }

        private void AddCameraSize(float sizeChange)
        {
            cameraLeft.m_Lens.OrthographicSize += sizeChange;
            cameraRight.m_Lens.OrthographicSize += sizeChange;
        }
    }
}