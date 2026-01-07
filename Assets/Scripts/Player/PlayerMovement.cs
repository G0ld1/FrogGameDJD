using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
   [Header("References")]
   public PlayerMovementStats MoveStats;

   [SerializeField] private BoxCollider _feetColl;
   [SerializeField] private Collider _bodyColl;

   private Rigidbody _rb;

   [Header("Animation")]
   public Animator playerAnimator;

   [Header("Audio")]
   [SerializeField] private AudioSource footstepSource;

   //movement vars
   private Vector3 moveVelocity;
   private bool _isFacingRight;
   private float Dir;


   [Header("Bash")]
   [SerializeField] private float Raduis;
   [SerializeField] GameObject BashAbleObj;
   private bool NearToBashAbleObj;
   public bool IsChosingDir;
   private bool IsBashing;
   [SerializeField] private float BashPower;

   [Header("Glide Settings")]
   public float glideDuration = 2.0f;     // Tempo máximo de voo
   public float glideFallSpeed = -2.0f;   // Velocidade constante de descida (ex: -2)
   private float glideTimer;
   private bool isGliding;
   private bool canGlide = true;

   [SerializeField] private GameObject Arrow;
   private ArrowMovement _arrowMovement;
   Vector3 BashDir;
   private float BashTimeReset;

   [Header("Bash Refinamento")]
   private bool justBashed;
   public float bashGravityMultiplier = 0.5f;
   public float bashFloatTime = 0.2f;
   private float bashFloatTimer;

   [Header("Grapple Refinamento")]
   private bool isFloatingFromGrapple;

   //collision check vars
   private RaycastHit _groundhit;
   private RaycastHit _headhit;
   private bool isGrounded;
   private bool bumpedHead;


   //jump vars
   public float VerticalVelocity {get; private set;}
   private bool isJumping;
   private bool isFastFalling;
   private bool isFalling;
   private float fastFallTime;
   private float fastFallReleaseSpeed;
   private int numberofJumpsUsed;


   //apex vars
   private float apexPoint;
   private float timepastApexThreshold;
   private bool isPastApexThreshold;

   //jump buffer vars
   private float jumpBufferTimer;
   private bool jumpReleasedDuringBuffer;

   //coyote time vars
   private float coyoteTimer;

   public GrapplingHook grapplingHook;

   private void Awake()
   {
      grapplingHook = GetComponent<GrapplingHook>();

      _isFacingRight = true;

      _rb = GetComponent<Rigidbody>();
      _arrowMovement = Arrow.GetComponent<ArrowMovement>();
   }

   private void Update()
   {
      CountTimers();
      JumpChecks();
      Bash();

      // Resetar o Glide quando toca no chão
      if (isGrounded)
      {
         canGlide = true;
         isGliding = false;
         glideTimer = 0;
      }

      // Determine horizontal input: prefer controller (InputManager) when non-zero, else keyboard
      float inputX = Mathf.Abs(InputManager.movement.x) > 0.01f ? InputManager.movement.x : Input.GetAxis("Horizontal");
      Dir = inputX * MoveStats.maxRunSpeed;

      // Ativação do Glide - support both InputManager and keyboard
      float currentVertical = _rb.linearVelocity.y;
      bool jumpPressed = InputManager.jumpWasPressed || Input.GetKeyDown(KeyCode.Space);
      bool jumpReleased = InputManager.jumpWasReleased || Input.GetKeyUp(KeyCode.Space);

      if (!isGrounded && canGlide && jumpPressed && currentVertical < 0)
      {
         isGliding = true;
         canGlide = false; // Só pode ativar uma vez por salto
         glideTimer = glideDuration;

         // RESET DA QUEDA: Para o momento de queda abrupta instantaneamente
         VerticalVelocity = glideFallSpeed;
      }

      // Se soltar o botão ou o tempo acabar, para o Glide
      if (isGliding && (jumpReleased || glideTimer <= 0))
      {
         isGliding = false;
      }
   }



   private void FixedUpdate()
   {
      CollisionChecks();

      if (IsChosingDir )
         return;


      if (grapplingHook != null && grapplingHook.isGrapplingActive)
      {
         ApplyGrapplePropulsion();
         return;
      }

      Jump();

      if (!justBashed)
      {
         // keep same behaviour but use Dir (from Update)
         _rb.linearVelocity = new Vector2(Dir * Time.deltaTime, _rb.linearVelocity.y);

         // Build moveInput: prefer InputManager if non-zero, else keyboard axis
         Vector3 moveInput = InputManager.movement;
         if (Mathf.Abs(moveInput.x) < 0.01f)
         {
            moveInput = new Vector3(Input.GetAxis("Horizontal"), 0f, 0f);
         }

         if (isGrounded)
         {
            Move(MoveStats.groundAcceleration, MoveStats.groundDeceleration, moveInput);
         }
         else
         {
            Move(MoveStats.AirAcceleration, MoveStats.AirDeceleration, moveInput);
         }
      }

      AnimateMovement();
   }

   private void AnimateMovement()
   {
      if (playerAnimator == null) return;

      float currentSpeed = Mathf.Abs(_rb.linearVelocity.x);

      float maxSpeed = MoveStats.maxRunSpeed;


      float normalizedSpeed = currentSpeed / maxSpeed;

      playerAnimator.SetFloat("Speed", normalizedSpeed);
   }

   public void ResetStateForRespawn()
   {
      // Zera o estado Vertical
      VerticalVelocity = 0f;
      isJumping = false;
      isFalling = false;
      isFastFalling = false;
      numberofJumpsUsed = 0;
      isGliding = false;
      canGlide = true;

      // Zera o estado de buffs/debuffs
      justBashed = false;
      isFloatingFromGrapple = false;
      IsChosingDir = false;

      // Zera as velocidades internas (horizontal)
      moveVelocity = Vector3.zero;
      Dir = 0f;

      // Zera os timers
      jumpBufferTimer = 0f;
      coyoteTimer = 0f;

   }


   #region Movement

   private void Move(float acceleration, float decceleration, Vector3 moveInput)
   {
      if (moveInput != Vector3.zero)
      {
         TurnCheck(moveInput);
         Vector3 targetVelocity = Vector3.zero;

         targetVelocity = new Vector3(moveInput.x, 0f, 0f) * MoveStats.maxRunSpeed;

         moveVelocity = Vector3.Lerp(moveVelocity, targetVelocity , acceleration * Time.fixedDeltaTime);
         _rb.linearVelocity = new Vector3(moveVelocity.x, _rb.linearVelocity.y, 0);
      }

      else if (moveInput == Vector3.zero)
      {
         moveVelocity = Vector3.Lerp(moveVelocity, Vector3.zero, decceleration * Time.fixedDeltaTime);
         _rb.linearVelocity = new Vector3(moveVelocity.x, _rb.linearVelocity.y, 0);
      }


   }

   private void TurnCheck(Vector3 moveInput)
   {
      if(_isFacingRight && moveInput.x <0)
      {
         Turn(false);
      }
      else if(!_isFacingRight && moveInput.x >0)
      {
         Turn(true);
      }
   }

   private void Turn(bool turnRight)
   {
      if (turnRight)
      {
         _isFacingRight = true;
         transform.Rotate(0f, 180f, 0f);
      }
      else
      {
         _isFacingRight = false;
         transform.Rotate(0f, -180f, 0f);
      }
   }
   #endregion

   #region Bash

   void Bash()
   {
       float bashRadius = Raduis;

       Collider[] targets = Physics.OverlapSphere(transform.position, bashRadius, MoveStats.bashLayer);

       NearToBashAbleObj = false;

       foreach (Collider target in targets)
       {
           if (target.CompareTag("BashAble"))
           {
               NearToBashAbleObj = true;
               BashAbleObj = target.gameObject;
               Debug.Log("Estas perto do objeto bashable");
               break;
           }
       }

       if (NearToBashAbleObj)
       {
           BashAbleObj.GetComponent<Renderer>().material.color = Color.yellow;

           // Start aiming: support both controller and mouse button
           bool startAim = InputManager.bashwasPressed || Input.GetMouseButtonDown(1);
           bool releaseAim = InputManager.bashwasReleased || Input.GetMouseButtonUp(1);

           if (startAim)
           {
               Time.timeScale = 0;
               Arrow.SetActive(true);
               _arrowMovement.Initialize(BashAbleObj.transform);
               IsChosingDir = true;
           }
           // Release aim: either controller or mouse
           else if (IsChosingDir && releaseAim)
           {
               Time.timeScale = 1f;

               SfxManager.instance.PlaySFX(
                  SfxManager.instance.bash,
                  0.2f
               );

               IsChosingDir = false;
               Arrow.SetActive(false);

               Vector3 aimWorldPos = GetAimWorldPosition();
               BashDir = aimWorldPos - BashAbleObj.transform.position;
               BashDir.z = 0;
               BashDir = BashDir.normalized;

               transform.position = BashAbleObj.transform.position;

               Rigidbody targetRb = BashAbleObj.GetComponent<Rigidbody>();
               if (targetRb != null)
               {
                  targetRb.AddForce(BashDir * 50f, ForceMode.Impulse);
               }

               Vector3 playerLaunchDir = -BashDir;

               float targetLaunchSpeed = BashPower;

               _rb.linearVelocity = playerLaunchDir * targetLaunchSpeed;

               _rb.useGravity = false;
               justBashed = true;
               bashFloatTimer = bashFloatTime;

               VerticalVelocity = _rb.linearVelocity.y;
           }
       }
       else if (BashAbleObj != null)
       {
           BashAbleObj.GetComponent<Renderer>().material.color = Color.white;
       }
   }

   // Aim position: prefer gamepad right-stick, fallback to mouse pointer
   // Add or replace this method in `PlayerMovement.cs`
   private Vector3 GetAimWorldPosition()
   {
      // Priority: gamepad right stick -> mouse
      Vector2 right = Vector2.zero;
      if (InputManager.playerInput != null && Gamepad.current != null)
      {
         right = Gamepad.current.rightStick.ReadValue();
      }

      float deadzone = 0.15f;
      if (right.sqrMagnitude > deadzone * deadzone)
      {
         Vector3 dir = new Vector3(right.x, right.y, 0f).normalized;
         float aimDistance = 5f; // tune as needed or expose as a field
         return BashAbleObj != null ? BashAbleObj.transform.position + dir * aimDistance : Vector3.zero;
      }

      // Fallback to mouse position
      if (Camera.main != null)
      {
         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
         Plane gamePlane = new Plane(Vector3.forward, Vector3.zero);
         if (gamePlane.Raycast(ray, out float distance))
         {
            Vector3 worldPosition = ray.GetPoint(distance);
            worldPosition.z = 0;
            return worldPosition;
         }
      }

      return BashAbleObj != null ? BashAbleObj.transform.position : Vector3.zero;
   }

   #endregion

   #region Jump

   private void JumpChecks()
   {
      if (InputManager.jumpWasPressed)
      {
         jumpBufferTimer = MoveStats.jumpBufferTime;
         jumpReleasedDuringBuffer = false;
      }

      if (InputManager.jumpWasReleased)
      {
         if (jumpBufferTimer > 0f)
         {
            jumpReleasedDuringBuffer = true;
         }

         if (isJumping && VerticalVelocity > 0f)
         {
            if (isPastApexThreshold)
            {
               isPastApexThreshold = false;
               isFastFalling = true;
               fastFallTime = MoveStats.timeforUpwardsCancel;
               VerticalVelocity = 0f;
            }
            else
            {
               isFastFalling = true;
               fastFallReleaseSpeed = VerticalVelocity;
            }
         }
      }

      if (jumpBufferTimer > 0f && !isJumping && (isGrounded || coyoteTimer > 0f))
      {
         InitiateJump(1);

         if (jumpReleasedDuringBuffer)
         {
            isFastFalling = true;
            fastFallReleaseSpeed = VerticalVelocity;
         }
      }

      else if (jumpBufferTimer > 0f && isFalling && numberofJumpsUsed < MoveStats.numberofJumpsAllowed-1)
      {
         InitiateJump(1);
         isFastFalling = false;
      }

      if ((isJumping || isFalling) && isGrounded && VerticalVelocity <= 0f)
      {
         isJumping = false;
         isFalling = false;
         isFastFalling = false;
         fastFallTime = 0f;
         isPastApexThreshold = false;
         numberofJumpsUsed = 0;

         VerticalVelocity = Physics.gravity.y;
      }
   }

   private void InitiateJump(int numberOfJumpsUsed)
   {
      if (!isJumping)
      {
         isJumping = true;
      }

      jumpBufferTimer = 0f;
      this.numberofJumpsUsed += numberOfJumpsUsed;
      VerticalVelocity = MoveStats.initialJumpVelocity;
   }

   private void Jump()
   {
      if (justBashed)
      {
         VerticalVelocity += MoveStats.Gravity * bashGravityMultiplier * Time.fixedDeltaTime;

         bashFloatTimer -= Time.fixedDeltaTime;

         _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, VerticalVelocity, _rb.linearVelocity.z);

         if (bashFloatTimer <= 0)
         {
            justBashed = false;
            _rb.useGravity = true;
         }

         if (justBashed)
         {
            return;
         }
      }

      if (isFloatingFromGrapple)
      {
         VerticalVelocity += MoveStats.Gravity * bashGravityMultiplier * Time.fixedDeltaTime;

         _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, VerticalVelocity, _rb.linearVelocity.z);

         if (isGrounded)
         {
            isFloatingFromGrapple = false;
         }

         if (isFloatingFromGrapple)
         {
            return;
         }
      }

      if (isJumping)
      {
         if (bumpedHead)
         {
            isFastFalling = true;
         }

         if (VerticalVelocity >= 0f)
         {
            apexPoint = Mathf.InverseLerp(MoveStats.initialJumpVelocity, 0f, VerticalVelocity);
            if (apexPoint > MoveStats.apexThreshold)
            {
               if (!isPastApexThreshold)
               {
                  isPastApexThreshold = true;
                  timepastApexThreshold = 0f;
               }

               if (isPastApexThreshold)
               {
                  timepastApexThreshold += Time.fixedDeltaTime;
                  if (timepastApexThreshold < MoveStats.apexHangTime)
                  {
                     VerticalVelocity = 0f;
                  }
                  else
                  {
                     VerticalVelocity = -0.01f;
                  }
               }
            }

            else
            {
               VerticalVelocity += MoveStats.Gravity*Time.fixedDeltaTime;
               if (isPastApexThreshold)
                  isPastApexThreshold = false;
            }
         }

         else if (!isFastFalling)
         {
            VerticalVelocity += MoveStats.Gravity * MoveStats.gravityonreleaseMultiplier * Time.fixedDeltaTime;
         }

         else if (VerticalVelocity < 0f)
         {
            if (!isFalling)
            {
               isFalling = true;
            }
         }
      }

      if (isFastFalling)
      {
         if (fastFallTime > -MoveStats.timeforUpwardsCancel)
         {
            VerticalVelocity += MoveStats.Gravity *MoveStats.gravityonreleaseMultiplier*Time.fixedDeltaTime;
         }
         else if (fastFallTime < MoveStats.timeforUpwardsCancel)
         {
            VerticalVelocity = Mathf.Lerp(fastFallReleaseSpeed, 0f, (fastFallTime / MoveStats.timeforUpwardsCancel));
         }

         fastFallTime += Time.fixedDeltaTime;
      }

      if (isGliding)
      {
         glideTimer -= Time.fixedDeltaTime;

         // Força uma velocidade de descida constante e suave
         VerticalVelocity = glideFallSpeed;

         // Aplica ao Rigidbody mantendo a movimentação horizontal
         _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, VerticalVelocity, _rb.linearVelocity.z);

         return; // Sai da função para não aplicar a gravidade pesada por cima disto
      }

      if (!isGrounded && !isJumping)
      {
         if (!isFalling) isFalling = true;

         // Aplica a gravidade multiplicada pelo fator de queda de plataforma
         VerticalVelocity += MoveStats.Gravity * MoveStats.fallfromledgemult * Time.fixedDeltaTime;
      }

      VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MoveStats.maxFallSpeed, 160f);

      _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, VerticalVelocity, _rb.linearVelocity.z);
   }
   #endregion
   #region Collision Checks

   private void IsGrounded()
   {
      Vector3 boxCastOrigin = new Vector3(_feetColl.bounds.center.x,_feetColl.bounds.min.y,0);

      Vector3 halfExtents = _feetColl.size/2f;

      float checkHeight = (_feetColl.size.y / 2f) + MoveStats.groundDetectionRayLength;

      Vector3 overlapHalfExtents = new Vector3(
         halfExtents.x,
         checkHeight / 2f,
         halfExtents.z
      );

      Vector3 center = _feetColl.bounds.center;
      center.y -= MoveStats.groundDetectionRayLength+0.05f;

      Collider[] hits = Physics.OverlapBox(
         center,
         overlapHalfExtents,
         Quaternion.identity,
         MoveStats.groundLayer
      );

      isGrounded = (hits.Length > 0);
   }

   private void BumpedHead()
   {
      Vector3 boxCastOrigin = new Vector3(_feetColl.bounds.center.x,_feetColl.bounds.max.y,0);
      Vector3 boxCastSize = new Vector3(_feetColl.bounds.size.x*MoveStats.HeadWidth,MoveStats.hadDetectionRayLength,MoveStats.groundLayer);

     bumpedHead = Physics.BoxCast(boxCastOrigin, boxCastSize, Vector3.up, out _headhit, quaternion.identity,
        MoveStats.hadDetectionRayLength, MoveStats.groundLayer );
   }

   private void CollisionChecks()
   {
      BumpedHead();
      IsGrounded();
   }
   #endregion

   #region Timers

   private void CountTimers()
   {
      jumpBufferTimer -= Time.deltaTime;

      if (isGrounded)
      {
         coyoteTimer -= Time.deltaTime;
      }
      else
      {
         coyoteTimer = MoveStats.jumpCoyoteTime;
      }
   }
   #endregion



   

   private void ApplyGrapplePropulsion()
   {
      _rb.AddForce(Vector3.up * MoveStats.Gravity, ForceMode.Acceleration);
   }

   public void InitiateGrappleLaunch(float verticalImpulse)
   {
      SfxManager.instance.PlaySFX(
         SfxManager.instance.grapple,
         0.3f
      );

      VerticalVelocity = verticalImpulse;

      isFloatingFromGrapple = true;

      _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, _rb.linearVelocity.z);
   }
}
