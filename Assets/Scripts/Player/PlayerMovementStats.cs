using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName="Player Movement")]
public class PlayerMovementStats : ScriptableObject
{
    [Header("Move")] [Range(1f, 100f)] public float maxMoveSpeed = 12.5f;
    [Header("Move")] [Range(0.25f, 50f)] public float groundAcceleration = 5f;
    [Header("Move")] [Range(0.25f, 50f)] public float groundDeceleration = 20;
    [Header("Move")] [Range(0.25f, 50f)] public float AirAcceleration = 5f;
    [Header("Move")] [Range(0.25f, 50f)] public float AirDeceleration = 5f;
    
    [Header("Run")]
    [Range(1f, 100f)] public float maxRunSpeed = 20f;
    
    [Header("Grounded/Collision Checks")]
    public LayerMask groundLayer;

    public float groundDetectionRayLength = 0.02f;
    public float hadDetectionRayLength = 0.02f;
    [Range(0f, 1f)] public float HeadWidth = 0.75f;

    [Header("Jump")] public float jumpHeight = 6.5f;
    [Range(1f, 1.1f)] public float jumpHeightCompensationFactor = 1.054f;
    public float timetilljumpApex = 0.35f;
    [Range(0.01f, 5f)] public float gravityonreleaseMultiplier = 2f;
    public float maxFallSpeed = 26f;
    [Range(1f, 5f)] public int numberofJumpsAllowed = 1;
    
    [Header("Jump Cut")]
    [Range(0.02f, 0.3f)] public float timeforUpwardsCancel = 0.027f;

    [Header("Jump Apex")] 
    [Range(0.05f, 1f)] public float apexThreshold = 0.97f;
    [Range(0.01f, 1f)] public float apexHangTime = 0.075f;

    [Header("Jump Buffer")] 
    [Range(0f, 1f)] public float jumpBufferTime = 0.125f;
    
    [Header("Jump Coyote Time")] 
    [Range(0f, 1f)] public float jumpCoyoteTime = 0.1f;

    [Header("Jump vis Tool")] public bool ShowWalkJumpArc = false;
    public bool stoponcollision = true;
    public bool DrawRight = true;
    [Range(5, 100)] public int arcResolution = 20;
    [Range(0, 500)] public int Visualizationsteps = 90;
    
    public float Gravity { get; private set; }
    public float initialJumpVelocity { get; private set; }
    
    public float AdjustedJumpHeight { get; private set; }

    private void OnValidate()
    {
        CalculateValues();
    }

    private void OnEnable()
    {
        CalculateValues();
    }


    private void CalculateValues()
    {
        AdjustedJumpHeight = jumpHeight*jumpHeightCompensationFactor;
        Gravity = -(2f*AdjustedJumpHeight) / Mathf.Pow(timetilljumpApex, 2f);
        initialJumpVelocity = Mathf.Abs(Gravity)*timetilljumpApex;
    }







}
