using UnityEngine;

public class GroundedEnemyAnimation : MonoBehaviour
{
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
        _animator.SetBool("run", _enemy.horizontalInput != 0 && _enemy.IsGrounded);
        _animator.SetBool("jump", _enemy.IsJumping);
        _animator.SetBool("fall", _enemy.IsFalling);
    }
    
    void Flip()
    {
        Vector3 currentScale = gameObject.transform.localScale;
        currentScale.x *= -1;
        gameObject.transform.localScale = currentScale;

        _facingRight = !_facingRight;
    }
}