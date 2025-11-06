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
   
   //movement vars
   private Vector3 moveVelocity;
   private bool _isFacingRight;
   
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

   private void Awake()
   {
      _isFacingRight = true;
      
      _rb = GetComponent<Rigidbody>();
   }

   private void Update()
   {
      CountTimers();
      JumpChecks();
      
     
   }

   private void FixedUpdate()
   {
      CollisionChecks();
      Jump();

      if (isGrounded)
      {
         Move(MoveStats.groundAcceleration, MoveStats.groundDeceleration,InputManager.movement);
      }
      else
      {
         Move(MoveStats.AirAcceleration, MoveStats.AirDeceleration,InputManager.movement);

      }
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
   #region Jump

   private void JumpChecks()
   {

      if (InputManager.jumpWasPressed)
      {
         Debug.Log("salto pressionado");
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
         
         VerticalVelocity =+ MoveStats.Gravity*Time.fixedDeltaTime;
      }

      VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MoveStats.maxFallSpeed, 50f);
      
      _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, VerticalVelocity, _rb.linearVelocity.z);
   }
   #endregion
   #region Collision Checks

   private void IsGrounded()
   {
      Vector3 boxCastOrigin = new Vector3(_feetColl.bounds.center.x,_feetColl.bounds.min.y,0);
     // Vector3 boxCastSize = new Vector3(_feetColl.bounds.size.x,MoveStats.groundDetectionRayLength,0);

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
         MoveStats.groundLayer // O LayerMask
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
      // Apenas desenha se o script estiver a funcionar (opcional, mas bom)
      if (!Application.isPlaying) 
         return; 
    
      // Obtemos o collider dos pés, tal como fazemos no IsGrounded()
      if (_feetColl == null) return;
      BoxCollider feetBox = (BoxCollider)_feetColl;

      // --- CÁLCULOS EXATOS DO OVERLAPBOX (REPETIMOS A LÓGICA DO ISGROUNDED) ---

      // 1. Half Extents
      Vector3 halfExtents = feetBox.size / 2f; 
      float checkHeight = (feetBox.size.y / 2f) + MoveStats.groundDetectionRayLength;
    
      Vector3 overlapHalfExtents = new Vector3(
         halfExtents.x, 
         checkHeight / 2f, 
         halfExtents.z     
      );

      // Se o Z for zero, aumentamos ligeiramente para visualizar no 3D
      if (overlapHalfExtents.z == 0) overlapHalfExtents.z = 0.05f; 

      // 2. Centro da Caixa
      Vector3 center = feetBox.bounds.center;
      center.y -= MoveStats.groundDetectionRayLength+0.05f;

      // --- DESENHAR O GIZMO ---

      // Cor: Vermelho se não houver colisão (isGrounded == false) e Verde se houver.
      if (isGrounded)
      {
         Gizmos.color = new Color(0f, 1f, 0f, 0.5f); // Verde (transparente)
      }
      else
      {
         Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // Vermelho (transparente)
      }

      // Desenha o cubo na posição e com o tamanho definido pelo OverlapBox.
      // O Gizmos.DrawWireCube espera o tamanho total, não o halfExtents.
      // Por isso, multiplicamos o halfExtents por 2.
      Gizmos.DrawWireCube(center, overlapHalfExtents * 2f);
   }
}
