using System.Collections;
using Core.Movement;
using UnityEngine;
using UnityEngine.Serialization;

namespace Entities.Player
{
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

        private bool IsKnockedbacked { get; set; }

        [SerializeField] private int maxMidairJumps = 2;
        private int _currentMidairJumps;
        
        [SerializeField] private int maxMidairDashes = 1;
        private int _currentMidairDashes;

        private bool _isDashing;
        public bool IsDashing => _isDashing;
        
        private bool _canSwitchDirections;
        private bool _canJump;
        private bool _canWallJump;
        private bool _isTouchingCeiling;
        private bool _isWallSlidingRight;
        private bool _isWallSlidingLeft;
        private bool _canDash = true;
        private int _dashDirection;

        // Run  Variables
        [SerializeField] private float moveSpeed = 6;
        [SerializeField] private float accelerationTimeAirborne = 0.2f;
        [SerializeField] private float accelerationTimeGrounded = 0.1f;
        private float _displacementXSmoothing;
        private int _facingDirection = 1;

        // Dash Variables;
        [SerializeField] private float dashingTime = 0.2f;
        [SerializeField] private float dashSpeed = 34f;
        [SerializeField] private float dashCooldown = 0.5f;
        [SerializeField] private float dashAccelerationTime;
        [SerializeField] private TrailRenderer tr;
        [SerializeField] private ParticleSystem ps;
        [SerializeField] private AudioClip dashSound;
        [SerializeField] private AudioSource dashAudioSource;

        private Coroutine _dashCoroutine;
        private ParticleSystem.EmissionModule _psEmission;

        // Jump Variables
        [SerializeField] private float maxJumpHeight;
        [SerializeField] private float timeToJumpApex;

        private const float JumpBufferWindow = 0.1f;
        private float _bufferWindow;
        private const float CoyoteTimeWindow = 0.1f;
        private float _timeLastTouchedGround;
        private float _displacementYSmoothing;

        // Fast Fall Variables
        [SerializeField] private float gravityDown;
        private float _gravityDown;

        // Wall Jump Variables
        private float _wallSlideDisplacementY;
        private const float WallSlideDeceleration = 0.1f;

        [SerializeField] private Vector3 wallClimb;
        [SerializeField] private Vector3 wallLeap;
        [SerializeField] private Vector3 wallHop;

        // Gravity
        private float _gravity;
        private float _jumpForce;
        [SerializeField] private float terminalVelocity = 80f;

        // Knockback Variables
        [SerializeField] private float knockbackGravity = 40f;
        private Vector3 _knockbackVelocity;

        // Update Variables
        public Vector3 displacement;
        Controller2D _controller;

        private void Start()
        {
            _controller = GetComponent<Controller2D>();

            _gravity = -2 * maxJumpHeight / Mathf.Pow(timeToJumpApex, 2);

            _gravityDown = _gravity * gravityDown;

            _jumpForce = 2 * maxJumpHeight / timeToJumpApex;

            _wallSlideDisplacementY = _gravity / 10;

            _psEmission = ps.emission;
        }

        private void Update()
        {
            if (IsKnockedbacked)
            {
                _knockbackVelocity.y -= knockbackGravity * Time.deltaTime;

                _controller.Move(_knockbackVelocity * Time.deltaTime);

                return;
            }

            // States
            _isGrounded = _controller.Collisions.Below;
            _isTouchingCeiling = _controller.Collisions.Above;
            _isWallSlidingRight = _controller.Collisions.Right && displacement.y < 0 && !_isGrounded;
            _isWallSlidingLeft = _controller.Collisions.Left && displacement.y < 0 && !_isGrounded;

            if (_isTouchingCeiling || _isGrounded)
            {
                displacement.y = 0;
            }

            if (_isGrounded)
            {
                _timeLastTouchedGround = 0;
                _currentMidairJumps = 0;
                _currentMidairDashes = 0;
                _isJumping = false;
                _isFalling = false;
            }
            
            if (_isTouchingCeiling)
            {
                _bufferWindow = -1;
            }
            
            // Cancel dash if dashing into a wall
            if (_isDashing && ((_dashDirection > 0 && _controller.Collisions.Right) || (_dashDirection < 0 && _controller.Collisions.Left)))
            {
                _isDashing = false;
                if (_dashCoroutine != null)
                {
                    StopCoroutine(_dashCoroutine);
                    _dashCoroutine = null;
                }
                _canDash = true;
                ps.Play();
                tr.emitting = false;
            }
            
            // Jumping
            if (!_isGrounded && displacement.y > 0)
            {
                _isJumping = true;
                _isFalling = false;
            }

            // Falling
            if (!_isGrounded && displacement.y <= 0)
            {
                _isJumping = false;
                _isFalling = true;
                _gravity = -2 * maxJumpHeight / Mathf.Pow(timeToJumpApex, 2);
            }

            // Timers
            _bufferWindow -= Time.deltaTime;
            _timeLastTouchedGround += Time.deltaTime;

            // Jump
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_isDashing)
                {
                    _isDashing = false;
                    if (_dashCoroutine != null)
                    {
                        StopCoroutine(_dashCoroutine);
                        _dashCoroutine = null;
                    }
                    _canDash = true;
                    ps.Play();
                    tr.emitting = false;
                }
                
                _bufferWindow = JumpBufferWindow;
                // Accounts for regularly jumping off the ground and coyote time
                if (_timeLastTouchedGround <= CoyoteTimeWindow && displacement.y <= 0)
                {
                    Jump();
                }
                else if (_currentMidairJumps < maxMidairJumps)
                {
                    Jump();
                    _currentMidairJumps++;
                }

                // Wall Jump
                if (_isWallSlidingLeft || _isWallSlidingRight)
                {
                    // Wall Climb
                    if (Mathf.Approximately(Mathf.Sign(Input.GetAxisRaw("Horizontal")), Mathf.Sign(_facingDirection)) &&
                        Input.GetAxisRaw("Horizontal") != 0)
                    {
                        WallClimb();
                    }
                    // Wall Hop
                    else if (Input.GetAxisRaw("Horizontal") == 0)
                    {
                        WallHop();
                    }
                    // Wall Leap
                    else
                    {
                        WallLeap();
                    }
                }
            }

            // Jump Buffer
            if (_isGrounded && _bufferWindow >= 0)
            {
                Jump();
            }

            // Jump Cut
            if (Input.GetKeyUp(KeyCode.Space) && _isJumping)
            {
                _gravity = _gravityDown;
            }

            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            float targetDisplacementX = input.x * moveSpeed;

            if (input.x != 0)
            {
                _facingDirection = (int)Mathf.Sign(input.x);
            }

            // Gravity
            if (_isWallSlidingRight || _isWallSlidingLeft)
            {
                displacement.y = Mathf.SmoothDamp(displacement.y, _wallSlideDisplacementY, ref _displacementYSmoothing,
                    WallSlideDeceleration);
                _currentMidairJumps = 0;
                if (!_isDashing)
                {
                    _currentMidairDashes = 0;
                }
            }
            else
            {
                displacement.y += _gravity * Time.deltaTime;
                displacement.y = Mathf.Max(displacement.y, -terminalVelocity);
            }

            // Run
            if (_isDashing)
            {
                displacement.x = Mathf.SmoothDamp(displacement.x, dashSpeed * _dashDirection, ref _displacementXSmoothing,
                    dashAccelerationTime);
                displacement.y = 0f;
            }
            else
            {
                displacement.x = Mathf.SmoothDamp(displacement.x, targetDisplacementX, ref _displacementXSmoothing,
                    (_isGrounded) ? accelerationTimeGrounded : accelerationTimeAirborne);
            }

            _controller.Move(displacement * Time.deltaTime);

            // Dash
            if (Input.GetKeyDown(KeyCode.Q) && _canDash && _currentMidairDashes < maxMidairDashes)
            {
                _dashCoroutine = StartCoroutine(Dash());
                _currentMidairDashes++;
            }

            // Trail
            bool isMovingHorizontally = Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f;
            if (_isGrounded && isMovingHorizontally && !_isDashing)
            {
                _psEmission.enabled = true;
            }
            else
            {
                _psEmission.enabled = false;
            }
        }

        public void ApplyKnockback(Vector3 knockbackForce, float duration)
        {
            StartCoroutine(KnockbackCoroutine(knockbackForce, duration));
        }
        
        private IEnumerator KnockbackCoroutine(Vector3 knockbackForce, float duration)
        {
            IsKnockedbacked = true;

            if (_isDashing)
            {
                _isDashing = false;
                if (_dashCoroutine != null)
                {
                    StopCoroutine(_dashCoroutine);
                    _dashCoroutine = null;
                }
                _canDash = true;
                ps.Play();
                tr.emitting = false;
            }

            _knockbackVelocity = knockbackForce;

            displacement = Vector3.zero;

            yield return new WaitForSeconds(duration);

            IsKnockedbacked = false;

            displacement = _knockbackVelocity;
        }

        private void Jump()
        {
            displacement.y = _jumpForce;

            _gravity = -2 * maxJumpHeight / Mathf.Pow(timeToJumpApex, 2);
        }

        private void WallClimb()
        {
            displacement.x = -_controller.Collisions.MovementDirection * wallClimb.x;
            displacement.y = _jumpForce;
            
            _gravity = -2 * maxJumpHeight / Mathf.Pow(timeToJumpApex, 2);
        }
        
        private void WallHop()
        {
            displacement.x = -_controller.Collisions.MovementDirection * wallHop.x;
            displacement.y = _jumpForce / 2;
            
            _isDashing = false;
            _canDash = true;
            
            _gravity = -2 * maxJumpHeight / Mathf.Pow(timeToJumpApex, 2);
        }
        
        private void WallLeap()
        {
            displacement.x = -_controller.Collisions.MovementDirection * wallLeap.x;
            displacement.y = _jumpForce;
            
            _gravity = -2 * maxJumpHeight / Mathf.Pow(timeToJumpApex, 2);
        }

        private IEnumerator Dash()
        {
            _canDash = false;
            _isDashing = true;
            _dashDirection = _facingDirection;
            ps.Stop();
            tr.emitting = true;
            dashAudioSource.PlayOneShot(dashSound);
            yield return new WaitForSeconds(dashingTime);
            ps.Play();
            tr.emitting = false;
            _isDashing = false;
            yield return new WaitForSeconds(dashCooldown);
            _canDash = true;
        }
    }
}