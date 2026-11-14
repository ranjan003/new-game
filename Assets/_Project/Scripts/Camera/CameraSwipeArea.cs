// Location: Assets/_Project/Scripts/Camera/CameraSwipeArea.cs
// Assembly: EC.Camera
// Put on a transparent UI Image covering the right side of the screen, *behind* the attack buttons.
// Dragging on it orbits the combat camera. Works with touch and with the mouse in the editor.
using UnityEngine;
using UnityEngine.EventSystems;

namespace ElementalClash.CameraSystem
{
    public sealed class CameraSwipeArea : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        [SerializeField] private SimpleCombatCamera _camera;
        [Tooltip("Degrees of orbit per 1000 px of horizontal swipe on a 1920 px wide screen.")]
        [SerializeField, Range(20f, 400f)] private float _sensitivity = 150f;

        public float Sensitivity
        {
            get => _sensitivity;
            set => _sensitivity = value;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Needed so this object receives the drag.
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_camera == null) return;
            float normalized = eventData.delta.x * (1920f / Mathf.Max(1, Screen.width));
            _camera.AddYaw(normalized * _sensitivity / 1000f);
        }
    }
}
