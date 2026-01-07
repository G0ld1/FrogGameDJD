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
  
    
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _grappleAction;
    private InputAction _bashAction;
    private InputAction _auraAction;
   

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        _moveAction = playerInput.actions["Move"];
        _jumpAction = playerInput.actions["Jump"];
        _grappleAction = playerInput.actions["Grapple"];
        _bashAction = playerInput.actions["Bash"];
        _auraAction = playerInput.actions["Aura"];
    }
    

    // Update is called once per frame
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
        
    }
}
