using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

[RequireComponent(typeof(Controller2D))]

public class GroundedEnemyMovement : MonoBehaviour {
    // States
    public bool isGrounded;
    private bool canJump = true;
    private bool canWallJump;
    private bool isTouchingCeiling;
    private bool isWallSlidingRight;
    private bool isWallSlidingLeft;
    private bool canDash = true;
    public bool isDashing;
    
    // Player variables
    [SerializeField] GameObject player;
    private CharacterController playerController;
    
    // AI Variables
    private float horizontalInput;
    private float verticalInput;
    private bool enemyJumped = false;
    private bool enemyDashed = false;
    [SerializeField] private float jumpingProximity = 10f;
    [SerializeField] private float jumpCooldown = -1f;
    
    public int damage = 1;
    
    // Run  Variables
    [SerializeField] private float moveSpeed = 6;
    [SerializeField] private float accelerationTimeAirborne = 0.2f;
    [SerializeField] private float accelerationTimeGrounded = 0.1f;
    private float displacementXSmoothing;
    
    // Dash Variables;
    private float dashingTime = 0.2f;
    [SerializeField] private float dashSpeed = 34f;
    [SerializeField] private float dashCooldown = 0.5f;
    [SerializeField] private float dashAccelerationTime = 0.0f;
    [SerializeField] private TrailRenderer tr;
    [SerializeField] private ParticleSystem ps;

    // Jump Variables
    [SerializeField] private float jumpHeight = 4;
    [SerializeField] private float timeToJumpApex = 0.4f;
    
    private float coyoteTimeWindow = 0.1f;
    private float timeLastTouchedGround;
    private float displacementYSmoothing;

    // Wall Jump Variables
    private float wallSlideDisplacementY;
    private float wallSlideDeceleration = 0.1f;

    [SerializeField] private Vector3 wallClimb;
    [SerializeField] private Vector3 wallLeap;
    [SerializeField] private Vector3 wallHop;
    
    // Gravity
    private float gravity;
    private float jumpForce;
    [SerializeField] private float gravityMult = 0.5f; // jump released early
    [SerializeField] private float terminalVelocity = 80f; // max fall speed

    Vector3 displacement;
    Controller2D controller;

    void Start() {
        controller = GetComponent<Controller2D>();
        playerController = player.GetComponent<CharacterController>();
        
        gravity = -(2 * jumpHeight + 1) / Mathf.Pow(timeToJumpApex, 2);
        
        wallSlideDisplacementY = gravity / 10;
        
        jumpForce = Mathf.Abs(gravity) * timeToJumpApex;
        
    }

    void Update() {
        // States
        isGrounded = controller.collisions.below;
        isTouchingCeiling = controller.collisions.above;
        isWallSlidingRight = controller.collisions.right && displacement.y < 0 && !isGrounded;
        isWallSlidingLeft = controller.collisions.left && displacement.y < 0 && !isGrounded;
        
        if (isTouchingCeiling || isGrounded) {
            displacement.y = 0;
        }
        if (isGrounded) {
            timeLastTouchedGround = 0;
        }
        
        // Timers
        timeLastTouchedGround += Time.deltaTime;
        jumpCooldown -= Time.deltaTime;
        
        // Jump
        float distance = Vector3.Distance(player.transform.position, transform.position);
        enemyJumped = distance < jumpingProximity;
        
        if (enemyJumped && canJump) {
            // Coyote Time
            if (timeLastTouchedGround <= coyoteTimeWindow && displacement.y <= 0) {
                displacement.y = jumpForce;
            }
            // Wall Jump
            if (isWallSlidingLeft || isWallSlidingRight) {
                // Wall Climb
                if (Mathf.Sign(horizontalInput) == Mathf.Sign(controller.collisions.facingDirection) && horizontalInput != 0) {
                    displacement.x = -controller.collisions.facingDirection * wallClimb.x;
                    displacement.y = wallClimb.y;
                }
                // Wall Hop
                else if (horizontalInput == 0) {
                    displacement.x = -controller.collisions.facingDirection * wallHop.x;
                    displacement.y = wallHop.y;
                }
                // Wall Leap
                else
                {
                    displacement.x = -controller.collisions.facingDirection * wallLeap.x;
                    displacement.y = wallLeap.y;
                }
            }
        }
        
        // Gravity
        if ((isWallSlidingRight || isWallSlidingLeft)) {
            displacement.y = Mathf.SmoothDamp(displacement.y, wallSlideDisplacementY, ref displacementYSmoothing,  wallSlideDeceleration);
        }
        else {
            displacement.y += gravity * Time.deltaTime;
            displacement.y = Mathf.Max(displacement.y, -terminalVelocity);
        }
        
        // Run
        horizontalInput = (player.transform.position.x > transform.position.x) ? 1 : -1;
        float targetdisplacementX = horizontalInput * moveSpeed;
        if (isDashing) {
            displacement.x = Mathf.SmoothDamp(displacement.x, dashSpeed * Mathf.Sign(controller.collisions.facingDirection), ref displacementXSmoothing, dashAccelerationTime);
            displacement.y = 0f;
        }
        else {
            displacement.x = Mathf.SmoothDamp(displacement.x, targetdisplacementX, ref displacementXSmoothing, (isGrounded) ? accelerationTimeGrounded : accelerationTimeAirborne);
        }
        controller.Move(displacement * Time.deltaTime);
        
        // Dash
        if (enemyDashed && canDash) {
            StartCoroutine(Dash());
        }
    }
    
    private IEnumerator Dash() {
        canDash = false;
        isDashing = true;
        ps.Stop();
        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        ps.Play();
        tr.emitting = false;
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}
