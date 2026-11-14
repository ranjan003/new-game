// Location: Assets/_Project/Scripts/Camera/SimpleCombatCamera.cs
// Assembly: EC.Camera
// Batch B placeholder camera: sits behind the player, looking past them at the opponent,
// and pulls back as the fighters separate. Replaced by a Cinemachine rig with swipe orbit in Batch C.
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

        public void SetTargets(Transform player, Transform opponent)
        {
            _player = player;
            _opponent = opponent;
        }

        private void LateUpdate()
        {
            if (_player == null || _opponent == null) return;

            Vector3 player = _player.position;
            Vector3 opponent = _opponent.position;
            Vector3 toOpponent = opponent - player;
            toOpponent.y = 0f;
            float separation = toOpponent.magnitude;
            Vector3 direction = separation > 0.01f ? toOpponent / separation : _player.forward;

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
