using UnityEngine;

public class GroundedEnemyAnimation : MonoBehaviour
{
    private bool facingRight = true;
    private Animator animator;
    private GroundedEnemyMovement enemy;

    public void Start()
    {
        animator = GetComponent<Animator>();
        enemy = GetComponent<GroundedEnemyMovement>();
    }
    void Update()
    {
        // Flip sprite;
        if (enemy.horizontalInput > 0 && !facingRight || enemy.horizontalInput < 0 && facingRight)
        {
            Flip();
        }
        
        // Animations
        animator.SetBool("run", enemy.horizontalInput != 0 && enemy.isGrounded);
        animator.SetBool("jump", enemy.isJumping);
        animator.SetBool("fall", enemy.isFalling);
    }
    
    void Flip()
    {
        Vector3 currentScale = gameObject.transform.localScale;
        currentScale.x *= -1;
        gameObject.transform.localScale = currentScale;

        facingRight = !facingRight;
    }
}