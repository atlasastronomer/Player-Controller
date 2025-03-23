using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

[RequireComponent(typeof(Controller2D))]

public class PlayerMovement : MonoBehaviour
{
    // States
    public bool isGrounded;
    private bool canJump;
    private bool canWallJump;
    private bool isTouchingCeiling;
    private bool isWallSlidingRight;
    private bool isWallSlidingLeft;
    private bool canDash = true;
    public bool isDashing;
    
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
    
    private float jumpBufferWindow = 0.1f;
    private float bufferWindow;
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
        if (isTouchingCeiling) { // prevents buffering multiple jumps in two-tile high passageway
            bufferWindow = -1;
        }
        
        // Timers
        bufferWindow -= Time.deltaTime;
        timeLastTouchedGround += Time.deltaTime;
        
        // Jump
        if (Input.GetKeyDown(KeyCode.Space)) {
            bufferWindow = jumpBufferWindow;
            // Coyote Time
            if (timeLastTouchedGround <= coyoteTimeWindow && displacement.y <= 0) {
                displacement.y = jumpForce;
            }
            // Wall Jump
            if (isWallSlidingLeft || isWallSlidingRight) {
                // Wall Climb
                if (Mathf.Sign(Input.GetAxisRaw("Horizontal")) == Mathf.Sign(controller.collisions.facingDirection) && Input.GetAxisRaw("Horizontal") != 0) {
                    displacement.x = -controller.collisions.facingDirection * wallClimb.x;
                    displacement.y = wallClimb.y;
                }
                // Wall Hop
                else if (Input.GetAxisRaw("Horizontal") == 0) {
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
            // Jump Buffer
        if(isGrounded && bufferWindow >= 0) {
            displacement.y = jumpForce;
        }
            // Jump Cut
            if (Input.GetKeyUp(KeyCode.Space) && displacement.y > 0) {
                displacement.y *= gravityMult;
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
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        
        float targetdisplacementX = input.x * moveSpeed;
        if (isDashing) {
            displacement.x = Mathf.SmoothDamp(displacement.x, dashSpeed * Mathf.Sign(controller.collisions.facingDirection), ref displacementXSmoothing, dashAccelerationTime);
            displacement.y = 0f;
        }
        else {
            displacement.x = Mathf.SmoothDamp(displacement.x, targetdisplacementX, ref displacementXSmoothing, (isGrounded) ? accelerationTimeGrounded : accelerationTimeAirborne);
        }
        controller.Move(displacement * Time.deltaTime);
        
        // Dash
        if (Input.GetKey(KeyCode.Q) && canDash) {
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
