using UnityEngine;

namespace Entities.Player
{
    public class PlayerAnimation : MonoBehaviour
    {
        private readonly int _run = Animator.StringToHash("run");
        private readonly int _jump = Animator.StringToHash("jump");
        private readonly int _fall = Animator.StringToHash("fall");
        private bool _facingRight = true;
        private Animator _animator;
        private PlayerMovement _player;

        public void Start()
        {
            _animator = GetComponent<Animator>();
            _player = GetComponent<PlayerMovement>();
        }

        void Update()
        {
            float inputHorizontal = Input.GetAxisRaw("Horizontal");
            if (!_player.IsDashing && (inputHorizontal > 0 && !_facingRight || inputHorizontal < 0 && _facingRight))
            {
                Flip();
            }

            _animator.SetBool(_run, (inputHorizontal != 0 && _player.IsGrounded));
            _animator.SetBool(_jump, _player.IsJumping);
            _animator.SetBool(_fall, _player.IsFalling);
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