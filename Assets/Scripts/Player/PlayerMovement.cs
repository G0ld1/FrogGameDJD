using System;
using Unity.Mathematics;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
   [Header("References")] 
   public PlayerMovementStats MoveStats;

   [SerializeField] private BoxCollider _feetColl;
   [SerializeField] private Collider _bodyColl;

   private Rigidbody _rb;
   
   [Header("Animation")] 
   public Animator playerAnimator;
   
   //movement vars
   private Vector3 moveVelocity;
   private bool _isFacingRight;
   private float Dir;
   
   
   [Header("Bash")]
   [SerializeField] private float Raduis;
   [SerializeField] GameObject BashAbleObj;
   private bool NearToBashAbleObj;
   private bool IsChosingDir;
   private bool IsBashing;
   [SerializeField] private float BashPower;

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


      Dir = Input.GetAxis("Horizontal") * MoveStats.maxRunSpeed;


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
            _rb.linearVelocity = new Vector2(Dir * Time.deltaTime, _rb.linearVelocity.y);
             
            if (isGrounded)
            {
               Move(MoveStats.groundAcceleration, MoveStats.groundDeceleration,InputManager.movement);
            }
            else
            {
               Move(MoveStats.AirAcceleration, MoveStats.AirDeceleration,InputManager.movement);
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

    // 3. Lógica de Apontar e Carregar
    if (NearToBashAbleObj)
    {
    
        BashAbleObj.GetComponent<Renderer>().material.color = Color.yellow; 
        
      
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            Time.timeScale = 0;
    
            BashAbleObj.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f); 
           Arrow.SetActive(true);
          _arrowMovement.Initialize(BashAbleObj.transform);
            IsChosingDir = true;
        }
        
    
        else if (IsChosingDir && Input.GetKeyUp(KeyCode.Mouse1))
        {
           Time.timeScale = 1f;
           BashAbleObj.transform.localScale = new Vector3(1f, 1f, 1f); 
           IsChosingDir = false;
           Arrow.SetActive(false);

           Vector3 mouseWorldPos = GetMouseWorldPosition();
           BashDir = mouseWorldPos - BashAbleObj.transform.position;
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
   


private Vector3 GetMouseWorldPosition()
{

    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    
   
    Plane gamePlane = new Plane(Vector3.forward, Vector3.zero); 

    if (gamePlane.Raycast(ray, out float distance))
    {
        Vector3 worldPosition = ray.GetPoint(distance);
        worldPosition.z = 0;
        return worldPosition;
    }
    return Vector3.zero;
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

      if (!isGrounded && !isJumping)
      {
         if (!isFalling)
         {
            isFalling = true;
         }
         
         VerticalVelocity =+ MoveStats.Gravity*Time.fixedDeltaTime*MoveStats.fallfromledgemult;
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
   
   
   
   private void OnDrawGizmos()
   {
     
      if (!Application.isPlaying) 
         return; 
    

      if (_feetColl == null) return;
      BoxCollider feetBox = (BoxCollider)_feetColl;


      Vector3 halfExtents = feetBox.size / 2f; 
      float checkHeight = (feetBox.size.y / 2f) + MoveStats.groundDetectionRayLength;
    
      Vector3 overlapHalfExtents = new Vector3(
         halfExtents.x, 
         checkHeight / 2f, 
         halfExtents.z     
      );

      if (overlapHalfExtents.z == 0) overlapHalfExtents.z = 0.05f; 

      Vector3 center = feetBox.bounds.center;
      center.y -= MoveStats.groundDetectionRayLength+0.05f;


      if (isGrounded)
      {
         Gizmos.color = new Color(0f, 1f, 0f, 0.5f); 
      }
      else
      {
         Gizmos.color = new Color(1f, 0f, 0f, 0.5f); 
      }

      Gizmos.DrawWireCube(center, overlapHalfExtents * 2f);
   }
   
   private void ApplyGrapplePropulsion()
   {

 
      _rb.AddForce(Vector3.up * MoveStats.Gravity, ForceMode.Acceleration);
    
     
   }
   
   public void InitiateGrappleLaunch(float verticalImpulse)
   {
 
      VerticalVelocity = verticalImpulse; 
      
      isFloatingFromGrapple = true; 
      
      _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, _rb.linearVelocity.z); 
  
   }
   
  
}
