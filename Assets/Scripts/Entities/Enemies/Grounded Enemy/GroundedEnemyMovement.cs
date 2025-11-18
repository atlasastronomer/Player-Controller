using System.Collections;
using Core.Movement;
using UnityEngine;
using UnityEngine.Serialization;

namespace Entities.Enemies.Grounded_Enemy
{
    [RequireComponent(typeof(Controller2D))]
    public class GroundedEnemyMovement : MonoBehaviour
    {
        // States
        private bool _isGrounded;
        public bool IsGrounded => _isGrounded;

        private bool _isJumping;
        public bool IsJumping => _isJumping;

        private bool _isFalling;
        public bool IsFalling => _isFalling;

        private bool _canJump = true;
        private bool _canWallJump;
        private bool _isTouchingCeiling;
        private bool _isWallSlidingRight;
        private bool _isWallSlidingLeft;
        private bool _canDash = true;
        private bool _isDashing;

        // Player variables
        [SerializeField] GameObject player;

        // AI Variables
        private float _horizontalInput;
        public float HorizontalInput => _horizontalInput;
        private float _verticalInput;
        private bool _enemyJumped;
        private bool _enemyDashed = false;
        [SerializeField] private float jumpingProximity = 10f;

        public int damage = 1;

        // Run  Variables
        [SerializeField] private float moveSpeed = 6;
        [SerializeField] private float accelerationTimeAirborne = 0.2f;
        [SerializeField] private float accelerationTimeGrounded = 0.1f;
        private float _displacementXSmoothing;

        // Dash Variables;
        [SerializeField] private float dashingTime = 0.2f;
        [SerializeField] private float dashSpeed = 34f;
        [SerializeField] private float dashCooldown = 0.5f;
        [SerializeField] private float dashAccelerationTime;
        [SerializeField] private TrailRenderer tr;
        [SerializeField] private ParticleSystem ps;

        // Jump Variables
        [SerializeField] private float maxJumpHeight = 4;
        [SerializeField] private float timeToJumpApex = 0.4f;

        private float _coyoteTimeWindow = 0.1f;
        private float _timeLastTouchedGround;
        private float _displacementYSmoothing;

        // Wall Jump Variables
        private float _wallSlideDisplacementY;
        private float _wallSlideDeceleration = 0.1f;

        [SerializeField] private Vector3 wallClimb;
        [SerializeField] private Vector3 wallLeap;
        [SerializeField] private Vector3 wallHop;

        // Gravity
        private float _gravity;
        private float _jumpForce;
        [SerializeField] private float terminalVelocity = 80f;

        // Update Variables
        private Vector3 _displacement;
        Controller2D _controller;

        void Start()
        {
            _controller = GetComponent<Controller2D>();

            player.GetComponent<CharacterController>();

            _gravity = -2 * maxJumpHeight / Mathf.Pow(timeToJumpApex, 2);

            _jumpForce = 2 * maxJumpHeight / timeToJumpApex;

            _wallSlideDisplacementY = _gravity / 10;
        }

        void Update()
        {
            // States
            _isGrounded = _controller.Collisions.Below;
            _isTouchingCeiling = _controller.Collisions.Above;
            _isWallSlidingRight = _controller.Collisions.Right && _displacement.y < 0 && !_isGrounded;
            _isWallSlidingLeft = _controller.Collisions.Left && _displacement.y < 0 && !_isGrounded;

            if (_isTouchingCeiling || _isGrounded)
            {
                _displacement.y = 0;
            }

            if (_isGrounded)
            {
                _timeLastTouchedGround = 0;
                _isJumping = false;
                _isFalling = false;
            }

            // Jumping or Falling
            if (!_isGrounded && _displacement.y > 0)
            {
                _isJumping = true;
                _isFalling = false;
            }

            if (!_isGrounded && _displacement.y <= 0)
            {
                _isJumping = false;
                _isFalling = true;
            }

            // Timers
            _timeLastTouchedGround += Time.deltaTime;

            // Jump
            float distance = Vector3.Distance(player.transform.position, transform.position);
            bool isBelowPlayer = player.transform.position.y > transform.position.y;

            _enemyJumped = distance < jumpingProximity;

            if (_enemyJumped && _canJump)
            {
                // Coyote Time
                if (_timeLastTouchedGround <= _coyoteTimeWindow && _displacement.y <= 0)
                {
                    Jump();
                }
            }

            if ((_controller.Collisions.Left || _controller.Collisions.Right) && _isGrounded && isBelowPlayer)
            {
                Jump();
            }

            // Wall Jump
            if ((_isWallSlidingLeft || _isWallSlidingRight) && isBelowPlayer)
            {
                // Wall Climb
                if (Mathf.Approximately(Mathf.Sign(_horizontalInput), Mathf.Sign(_controller.Collisions.MovementDirection)) &&
                    _horizontalInput != 0)
                {
                    WallClimb();
                }
                // Wall Hop
                else if (_horizontalInput == 0)
                {
                    WallHop();
                }
                // Wall Leap
                else
                {
                    WallLeap();
                }
            }

            // Gravity
            if ((_isWallSlidingRight || _isWallSlidingLeft))
            {
                _displacement.y = Mathf.SmoothDamp(_displacement.y, _wallSlideDisplacementY, ref _displacementYSmoothing,
                    _wallSlideDeceleration);
            }
            else
            {
                _displacement.y += _gravity * Time.deltaTime;
                _displacement.y = Mathf.Max(_displacement.y, -terminalVelocity);
            }

            // Run
            _horizontalInput = (player.transform.position.x > transform.position.x) ? 1 : -1;
            float targetDisplacementX = _horizontalInput * moveSpeed;
            if (_isDashing)
            {
                _displacement.x = Mathf.SmoothDamp(_displacement.x,
                    dashSpeed * Mathf.Sign(_controller.Collisions.MovementDirection), ref _displacementXSmoothing,
                    dashAccelerationTime);
                _displacement.y = 0f;
            }
            else
            {
                _displacement.x = Mathf.SmoothDamp(_displacement.x, targetDisplacementX, ref _displacementXSmoothing,
                    (_isGrounded) ? accelerationTimeGrounded : accelerationTimeAirborne);
            }

            _controller.Move(_displacement * Time.deltaTime);

            // Dash
            if (_enemyDashed && _canDash)
            {
                StartCoroutine(Dash());
            }
        }

        private void Jump()
        {
            _displacement.y = _jumpForce;
        }

        private void WallClimb()
        {
            _displacement.x = -_controller.Collisions.MovementDirection * wallClimb.x;
            _displacement.y = _jumpForce;
        }

        private void WallHop()
        {
            _displacement.x = -_controller.Collisions.MovementDirection * wallHop.x;
            _displacement.y = _jumpForce / 2;
        }

        private void WallLeap()
        {
            _displacement.x = -_controller.Collisions.MovementDirection * wallLeap.x;
            _displacement.y = _jumpForce;
        }

        private IEnumerator Dash()
        {
            _canDash = false;
            _isDashing = true;
            ps.Stop();
            tr.emitting = true;
            yield return new WaitForSeconds(dashingTime);
            ps.Play();
            tr.emitting = false;
            _isDashing = false;
            yield return new WaitForSeconds(dashCooldown);
            _canDash = true;
        }
    }
}