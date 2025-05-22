using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controller2D))]

public class PlayerMovement : MonoBehaviour
{
    // States
    private bool _isGrounded;
    public bool IsGrounded => _isGrounded;
    
    private bool _isJumping;
    public bool IsJumping => _isJumping;
    
    private bool _isFalling;
    public bool IsFalling => _isFalling;
    
    private bool _canJump;
    private bool _canWallJump;
    private bool _isTouchingCeiling;
    private bool _isWallSlidingRight;
    private bool _isWallSlidingLeft;
    private bool _canDash = true;
    public bool isDashing;
    
    // Run  Variables
    [SerializeField] private float moveSpeed = 6;
    [SerializeField] private float accelerationTimeAirborne = 0.2f;
    [SerializeField] private float accelerationTimeGrounded = 0.1f;
    private float _displacementXSmoothing;
    
    // Dash Variables;
    private float _dashingTime = 0.2f;
    [SerializeField] private float dashSpeed = 34f;
    [SerializeField] private float dashCooldown = 0.5f;
    [SerializeField] private float dashAccelerationTime;
    [SerializeField] private TrailRenderer tr;
    [SerializeField] private ParticleSystem ps;

    // Jump Variables
    [SerializeField] private float maxJumpHeight;
    [SerializeField] private float timeToJumpApex;
    
    private float _jumpBufferWindow = 0.1f;
    private float _bufferWindow;
    private float _coyoteTimeWindow = 0.1f;
    private float _timeLastTouchedGround;
    private float _displacementYSmoothing;
    
    // Fast Fall Variables
    [SerializeField] private float gravityDown;
    private float _gravityDown;

    // Wall Jump Variables
    private float _wallSlideDisplacementY;
    private float _wallSlideDeceleration = 0.1f;

    [SerializeField] private Vector3 wallClimb;
    [SerializeField] private Vector3 wallLeap;
    [SerializeField] private Vector3 wallHop;
    
    // Gravity
    private float _gravity;
    private float _jumpForce;
    [SerializeField] private float terminalVelocity = 80f; // max fall speed

    // Update Variables
    public Vector3 displacement;
    public Vector3 previousDisplacement;

    Controller2D _controller;
    
    private void Start() {
        _controller = GetComponent<Controller2D>();
        
        _gravity = -2 * maxJumpHeight / Mathf.Pow(timeToJumpApex, 2);
        _gravityDown = _gravity * gravityDown;
        
        _jumpForce = 2 * maxJumpHeight / timeToJumpApex;
        
        _wallSlideDisplacementY = _gravity / 10;
        
    }

    private void Update() {
        // States
        _isGrounded = _controller.collisions.below;
        _isTouchingCeiling = _controller.collisions.above;
        _isWallSlidingRight = _controller.collisions.right && displacement.y < 0 && !_isGrounded;
        _isWallSlidingLeft = _controller.collisions.left && displacement.y < 0 && !_isGrounded;
        
        if (_isTouchingCeiling || _isGrounded) {
            displacement.y = 0;
        }
        if (_isGrounded) {
            _timeLastTouchedGround = 0;
            _isJumping = false;
            _isFalling = false;
        }
        if (_isTouchingCeiling) { // prevents buffering multiple jumps in two-tile high passageway
            _bufferWindow = -1;
        }

        // Jumping or Falling
        if (!_isGrounded && displacement.y > 0) {
            _isJumping = true;
            _isFalling = false;
        }
        if (!_isGrounded && displacement.y <= 0) {
            _isJumping = false;
            _isFalling = true;
            _gravity = -2 * maxJumpHeight / Mathf.Pow(timeToJumpApex, 2);
        }
        
        // Timers
        _bufferWindow -= Time.deltaTime;
        _timeLastTouchedGround += Time.deltaTime;
        
        // Jump
        if (Input.GetKeyDown(KeyCode.Space)) {
            _bufferWindow = _jumpBufferWindow;
            // Coyote Time
            if (_timeLastTouchedGround <= _coyoteTimeWindow && displacement.y <= 0) {
                Jump();
            }
            // Wall Jump
            if (_isWallSlidingLeft || _isWallSlidingRight) {
                // Wall Climb
                if (Mathf.Sign(Input.GetAxisRaw("Horizontal")) == Mathf.Sign(_controller.collisions.facingDirection) && Input.GetAxisRaw("Horizontal") != 0) {
                    WallClimb();
                }
                // Wall Hop
                else if (Input.GetAxisRaw("Horizontal") == 0) {
                    WallHop();
                }
                // Wall Leap
                else {
                    WallLeap();
                }
            }
        }
        // Jump Buffer
        if(_isGrounded && _bufferWindow >= 0) {
            Jump();
        }
        
        // Jump Cut
        if (Input.GetKeyUp(KeyCode.Space) && _isJumping) {
            _gravity = _gravityDown;
        }
    
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        float targetdisplacementX = input.x * moveSpeed;
        
        // Gravity
        if ((_isWallSlidingRight || _isWallSlidingLeft)) {
            displacement.y = Mathf.SmoothDamp(displacement.y, _wallSlideDisplacementY, ref _displacementYSmoothing,  _wallSlideDeceleration);
        }
        else {
            displacement.y += _gravity * Time.deltaTime;
            displacement.y = Mathf.Max(displacement.y, -terminalVelocity);
        }
        
        // Run
        if (isDashing) {
            displacement.x = Mathf.SmoothDamp(displacement.x, dashSpeed * Mathf.Sign(_controller.collisions.facingDirection), ref _displacementXSmoothing, dashAccelerationTime);
            displacement.y = 0f;
        }
        else {
            displacement.x = Mathf.SmoothDamp(displacement.x, targetdisplacementX, ref _displacementXSmoothing, (_isGrounded) ? accelerationTimeGrounded : accelerationTimeAirborne);
        }
        _controller.Move(displacement * Time.deltaTime);
        
        // Dash
        if (Input.GetKey(KeyCode.Q) && _canDash) {
            StartCoroutine(Dash());
        }
    }
    
    private void Jump() {
        displacement.y = _jumpForce;
        
        _gravity = -2 * maxJumpHeight / Mathf.Pow(timeToJumpApex, 2);
    }

    private void WallClimb() {
        displacement.x = -_controller.collisions.facingDirection * wallClimb.x;
        
        displacement.y = _jumpForce;
        _gravity = -2 * maxJumpHeight / Mathf.Pow(timeToJumpApex, 2);
    }

    private void WallHop() {
        displacement.x = -_controller.collisions.facingDirection * wallHop.x;
        
        displacement.y = _jumpForce / 2;
        _gravity = -2 * maxJumpHeight / Mathf.Pow(timeToJumpApex, 2);
    }
    
    private void WallLeap() {
        displacement.x = -_controller.collisions.facingDirection * wallLeap.x;
        
        displacement.y = _jumpForce;
        _gravity = -2 * maxJumpHeight / Mathf.Pow(timeToJumpApex, 2);
    }
    
    private IEnumerator Dash() {
        _canDash = false;
        isDashing = true;
        ps.Stop();
        tr.emitting = true;
        yield return new WaitForSeconds(_dashingTime);
        ps.Play();
        tr.emitting = false;
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        _canDash = true;
    }
}
