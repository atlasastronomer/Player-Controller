using System.Collections;
using Core.Movement;
using UnityEngine;

namespace Entities.Enemies.Flying_Enemy
{
    [RequireComponent(typeof(Controller2D))]
    public class FlyingEnemyMovement : MonoBehaviour
    {
        // States
        private bool _isGrounded;
        private bool _isTouchingCeiling;
        private bool _isChasing;

        // Player variables
        [SerializeField] GameObject player;

        // AI Variables
        private float _horizontalInput;
        private float _verticalInput;
        private float _displacementXSmoothing;
        private float _displacementYSmoothing;
        [SerializeField] private float chaseDistance = 5f;

        // Run  Variables
        [SerializeField] private float moveSpeed = 6;
        [SerializeField] private float accelerationTime = 0.2f;

        // Jump Variables
        [SerializeField] private float jumpHeight = 4;
        [SerializeField] private float timeToJumpApex = 0.4f;


        Vector3 _displacement;
        Controller2D _controller;

        void Start()
        {
            _controller = GetComponent<Controller2D>();
        }

        void Update()
        {
            // States
            _isGrounded = _controller.Collisions.Below;
            _isTouchingCeiling = _controller.Collisions.Above;

            float distance = Vector3.Distance(player.transform.position, transform.position);
            _isChasing = distance < chaseDistance;

            if (_isTouchingCeiling || _isGrounded)
            {
                _displacement.y = 0;
            }

            // Flying Implementation
            _horizontalInput = (player.transform.position.x > transform.position.x) ? 1 : -1;
            _verticalInput = (player.transform.position.y > transform.position.y) ? 1 : -1;
            float targetDisplacementX = _horizontalInput * moveSpeed;
            float targetDisplacementY = _verticalInput * moveSpeed;
            if (_isChasing)
            {
                _displacement.x = Mathf.SmoothDamp(_displacement.x, targetDisplacementX, ref _displacementXSmoothing,
                    accelerationTime);
                _displacement.y = Mathf.SmoothDamp(_displacement.y, targetDisplacementY, ref _displacementYSmoothing,
                    accelerationTime);
            }
            else
            {
                _displacement.y = Mathf.Sin(Time.time * 2f) * 2.5f;
                _displacement.x = Mathf.SmoothDamp(_displacement.x, 0, ref _displacementXSmoothing, accelerationTime);
            }

            _controller.Move(_displacement * Time.deltaTime);
        }
        
        public void ApplyKnockback(Vector3 knockbackForce, float duration)
        {
            StartCoroutine(KnockbackCoroutine(knockbackForce, duration));
        }

        private IEnumerator KnockbackCoroutine(Vector3 knockbackForce, float duration)
        {
            Vector3 knockbackVelocity = knockbackForce;
            float elapsed = 0f;
    
            while (elapsed < duration)
            {
                _displacement.x = knockbackVelocity.x;
                _displacement.y = knockbackVelocity.y;
        
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }
}