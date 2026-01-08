
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static PlayerInput playerInput;

    public static Vector3 movement;
    public static bool jumpWasPressed;
    public static bool jumpIsHeld;
    public static bool jumpWasReleased;

    public static bool grappleWasPressed;
    public static bool grappleWasReleased;

    public static bool bashwasPressed;
    public static bool bashwasReleased;

    public static bool aurawasPressed;

    // new UI input flags
    public static bool menuWasPressed;
    public static bool submitWasPressed;

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _grappleAction;
    private InputAction _bashAction;
    private InputAction _auraAction;

    // new actions
    private InputAction _menuAction;
    private InputAction _submitAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        _moveAction = playerInput.actions["Move"];
        _jumpAction = playerInput.actions["Jump"];
        _grappleAction = playerInput.actions["Grapple"];
        _bash_action: _bashAction = playerInput.actions["Bash"];
        _auraAction = playerInput.actions["Aura"];

        _menuAction = playerInput.actions["Menu"];
       _submitAction = playerInput.actions["Submit"];
    }

    void Update()
    {
        movement = _moveAction.ReadValue<Vector2>();

        jumpWasPressed = _jumpAction.WasPressedThisFrame();
        jumpIsHeld = _jumpAction.IsPressed();
        jumpWasReleased = _jumpAction.WasReleasedThisFrame();

        grappleWasPressed = _grappleAction.WasPressedThisFrame();
        grappleWasReleased = _grappleAction.WasReleasedThisFrame();

        bashwasPressed = _bashAction.WasPressedThisFrame();
        bashwasReleased = _bashAction.WasReleasedThisFrame();

        aurawasPressed = _auraAction.WasPressedThisFrame();

        menuWasPressed = (_menuAction != null && _menuAction.WasPressedThisFrame()) || Input.GetKeyDown(KeyCode.Escape);
        submitWasPressed = (_submitAction != null && _submitAction.WasPressedThisFrame()) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space);
    }
}
