// Location: Assets/_Project/Scripts/Camera/SimpleCombatCamera.cs
// Assembly: EC.Camera
// Combat camera: sits behind the player looking past them at the opponent, pulls back as the fighters
// separate, and can be orbited sideways (swipe on phone, mouse drag in editor) up to ±60°.
// It drifts back to centre after a short pause. A Cinemachine version may replace it after the prototype.
using UnityEngine;

namespace ElementalClash.CameraSystem
{
    public sealed class SimpleCombatCamera : MonoBehaviour
    {
        [SerializeField] private Transform _player;
        [SerializeField] private Transform _opponent;
        [SerializeField] private float _distance = 5.5f;
        [SerializeField] private float _height = 3f;
        [SerializeField] private float _lookHeight = 1.1f;
        [SerializeField] private float _extraDistancePerMeter = 0.3f;
        [SerializeField] private float _sharpness = 6f;

        [Header("Orbit")]
        [SerializeField, Range(0f, 90f)] private float _maxYaw = 60f;
        [SerializeField, Min(0f)] private float _recenterDelay = 1.5f;
        [SerializeField, Min(0f)] private float _recenterSpeed = 60f;

        private float _yaw;
        private float _timeSinceYawInput = float.MaxValue;

        public float Yaw => _yaw;

        public void SetTargets(Transform player, Transform opponent)
        {
            _player = player;
            _opponent = opponent;
        }

        /// <summary>Orbit sideways by this many degrees (positive = to the right).</summary>
        public void AddYaw(float degrees)
        {
            _yaw = Mathf.Clamp(_yaw + degrees, -_maxYaw, _maxYaw);
            _timeSinceYawInput = 0f;
        }

        private void LateUpdate()
        {
            if (_player == null || _opponent == null) return;

            _timeSinceYawInput += Time.deltaTime;
            if (_timeSinceYawInput > _recenterDelay)
            {
                _yaw = Mathf.MoveTowards(_yaw, 0f, _recenterSpeed * Time.deltaTime);
            }

            Vector3 player = _player.position;
            Vector3 opponent = _opponent.position;
            Vector3 toOpponent = opponent - player;
            toOpponent.y = 0f;
            float separation = toOpponent.magnitude;
            Vector3 direction = separation > 0.01f ? toOpponent / separation : _player.forward;
            direction = Quaternion.AngleAxis(-_yaw, Vector3.up) * direction;

            float distance = _distance + separation * _extraDistancePerMeter;
            Vector3 desiredPosition = player - direction * distance + Vector3.up * _height;
            Vector3 lookTarget = (player + opponent) * 0.5f + Vector3.up * _lookHeight;

            float t = 1f - Mathf.Exp(-_sharpness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, t);
            Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, t);
        }
    }
}
