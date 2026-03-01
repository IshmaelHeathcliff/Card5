using Gameplay.Character.Player;
using UnityEngine;

namespace Core
{
    public class CameraFollow : MonoBehaviour, IController
    {
        [SerializeField] float _speed = 5;

        [Header("目标")]
        [SerializeField] Transform _target;

        [Header("边界")]
        [SerializeField] Transform _leftBoundary;
        [SerializeField] Transform _rightBoundary;
        [SerializeField] Transform _upBoundary;
        [SerializeField] Transform _downBoundary;


        Camera _camera;
        Vector2 _cameraHalfSize;


        void MoveCamera()
        {
            Vector2 targetPosition = _target == null ? this.SendQuery(new PlayerPositionQuery()) : _target.position;

            var targetDeltaPosition = Vector3.Lerp(_camera.transform.position, targetPosition, Time.fixedDeltaTime * _speed);


            if (_leftBoundary != null)
            {
                targetDeltaPosition.x = Mathf.Max(targetDeltaPosition.x,
                    _leftBoundary.position.x + _cameraHalfSize.x);
            }

            if (_rightBoundary != null)
            {
                targetDeltaPosition.x = Mathf.Min(targetDeltaPosition.x,
                    _rightBoundary.position.x - _cameraHalfSize.x);
            }

            if (_downBoundary != null)
            {
                targetDeltaPosition.y = Mathf.Max(targetDeltaPosition.y,
                    _downBoundary.position.y + _cameraHalfSize.y);
            }

            if (_upBoundary != null)
            {
                targetDeltaPosition.y = Mathf.Min(targetDeltaPosition.y,
                    _upBoundary.position.y - _cameraHalfSize.y);
            }

            targetDeltaPosition.z = _camera.transform.position.z;
            _camera.transform.position = targetDeltaPosition;
        }

        void Awake()
        {
            _camera = GetComponent<Camera>();
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            _cameraHalfSize = new Vector2(_camera.orthographicSize * _camera.aspect, _camera.orthographicSize);
        }


        void FixedUpdate()
        {
            MoveCamera();
        }

        public IArchitecture GetArchitecture()
        {
            return GameFrame.Interface;
        }
    }
}
