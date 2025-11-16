using UnityEngine;

namespace Entities.Enemies.Grounded_Enemy
{
    public class GroundedEnemyAnimation : MonoBehaviour
    {
        private static readonly int Run = Animator.StringToHash("run");
        private static readonly int Jump = Animator.StringToHash("jump");
        private static readonly int Fall = Animator.StringToHash("fall");
        private bool _facingRight = true;
        private Animator _animator;
        private GroundedEnemyMovement _enemy;

        public void Start()
        {
            _animator = GetComponent<Animator>();
            _enemy = GetComponent<GroundedEnemyMovement>();
        }

        void Update()
        {
            // Flip sprite;
            if (_enemy.horizontalInput > 0 && !_facingRight || _enemy.horizontalInput < 0 && _facingRight)
            {
                Flip();
            }

            // Animations
            _animator.SetBool(Run, _enemy.horizontalInput != 0 && _enemy.IsGrounded);
            _animator.SetBool(Jump, _enemy.IsJumping);
            _animator.SetBool(Fall, _enemy.IsFalling);
        }

        void Flip()
        {
            Vector3 currentScale = gameObject.transform.localScale;
            currentScale.x *= -1;
            gameObject.transform.localScale = currentScale;

            _facingRight = !_facingRight;
        }
    }
}