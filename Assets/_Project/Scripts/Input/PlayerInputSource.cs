// Location: Assets/_Project/Scripts/Input/PlayerInputSource.cs
// Assembly: EC.Controls
// Reads the "Combat" map of ControlsActions.inputactions (keyboard, gamepad, and later on-screen touch controls)
// and turns it into fighter commands. Movement is camera-relative.
//
// Inspector:
//   Actions          -> Assets/_Project/Settings/Input/ControlsActions.inputactions
//   Camera Transform -> leave empty to use Camera.main
using System;
using ElementalClash.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ElementalClash.Controls
{
    public sealed class PlayerInputSource : MonoBehaviour, IFighterCommandSource
    {
        // Must match the FighterAction enum order and the action names in the .inputactions file.
        private static readonly string[] ButtonActionNames =
        {
            "Light", "Heavy", "Special1", "Special2", "Guard", "Dodge", "Ultimate",
        };

        [SerializeField] private InputActionAsset _actions;
        [SerializeField] private string _mapName = "Combat";
        [SerializeField] private Transform _cameraTransform;
        [SerializeField, Range(0.05f, 0.3f)] private float _bufferSeconds = 0.15f;

        private readonly InputAction[] _buttons = new InputAction[FighterActions.Count];
        private InputActionMap _map;
        private InputAction _move;
        private InputBuffer _buffer;

        private void Awake()
        {
            _buffer = new InputBuffer(_bufferSeconds);

            if (_actions == null)
            {
                Debug.LogError("[Elemental Clash] PlayerInputSource needs the ControlsActions asset.", this);
                enabled = false;
                return;
            }

            _map = _actions.FindActionMap(_mapName, true);
            _move = _map.FindAction("Move", true);
            for (int i = 0; i < _buttons.Length; i++)
            {
                _buttons[i] = _map.FindAction(ButtonActionNames[i], true);
            }
        }

        private void OnEnable()
        {
            if (_map == null) return;
            foreach (InputAction button in _buttons) button.performed += OnButtonPerformed;
            _map.Enable();
        }

        private void OnDisable()
        {
            if (_map == null) return;
            foreach (InputAction button in _buttons) button.performed -= OnButtonPerformed;
            _map.Disable();
            _buffer.Clear();
        }

        private void OnButtonPerformed(InputAction.CallbackContext context)
        {
            int index = Array.IndexOf(_buttons, context.action);
            if (index >= 0) _buffer.Register((FighterAction)index, Time.realtimeSinceStartup);
        }

        public Vector3 GetMoveDirection(Fighter self)
        {
            if (_move == null) return Vector3.zero;

            Vector2 stick = _move.ReadValue<Vector2>();
            Transform cam = _cameraTransform;
            if (cam == null && Camera.main != null) cam = Camera.main.transform;
            if (cam == null) return Vector3.ClampMagnitude(new Vector3(stick.x, 0f, stick.y), 1f);

            Vector3 forward = cam.forward;
            forward.y = 0f;
            forward.Normalize();
            Vector3 right = cam.right;
            right.y = 0f;
            right.Normalize();
            return Vector3.ClampMagnitude(forward * stick.y + right * stick.x, 1f);
        }

        public bool ConsumePress(FighterAction action) =>
            _buffer != null && _buffer.Consume(action, Time.realtimeSinceStartup);

        public bool IsHeld(FighterAction action)
        {
            InputAction button = _buttons[(int)action];
            return button != null && button.IsPressed();
        }
    }
}
