using UnityEngine;

public class Animation : MonoBehaviour
{
    private bool facingRight = true;
    private Animator animator;
    private PlayerMovement player;

    public void Start()
    {
        animator = GetComponent<Animator>();
        player = GetComponent<PlayerMovement>();
    }
    void Update()
    {
        // Flip sprite
        float inputHorizontal = Input.GetAxisRaw("Horizontal");
        if (inputHorizontal > 0 && !facingRight || inputHorizontal < 0 && facingRight)
        {
            Flip();
        }
        
        // Run animation
        animator.SetBool("run", (inputHorizontal != 0 && player.isGrounded));
        animator.SetBool("jump", player.isJumping);
        animator.SetBool("fall", player.isFalling);
    }
    
    void Flip()
    {
        Vector3 currentScale = gameObject.transform.localScale;
        currentScale.x *= -1;
        gameObject.transform.localScale = currentScale;

        facingRight = !facingRight;
    }
}
