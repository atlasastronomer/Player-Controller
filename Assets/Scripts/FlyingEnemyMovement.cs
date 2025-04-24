using System;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

[RequireComponent(typeof(Controller2D))]

public class FlyingEnemeyMovement : MonoBehaviour {
    // States
    public bool isGrounded;
    public bool isJumping;
    public bool isFalling;
    private bool canJump = true;
    private bool canWallJump;
    private bool isTouchingCeiling;
    private bool isWallSlidingRight;
    private bool isWallSlidingLeft;
    private bool canDash = true;
    public bool isChasing;
    public bool isPatrolling;
    
    // Player variables
    [SerializeField] GameObject player;
    private CharacterController playerController;
    
    // AI Variables
    public float horizontalInput;
    public float verticalInput;
    [SerializeField] private float chaseDistance = 5f;
    [SerializeField] private float jumpCooldown = -1f;
    
    public int damage = 1;
    
    // Run  Variables
    [SerializeField] private float moveSpeed = 6;
    [SerializeField] private float accelerationTime = 0.2f;
    private float displacementXSmoothing;

    // Jump Variables
    [SerializeField] private float jumpHeight = 4;
    [SerializeField] private float timeToJumpApex = 0.4f;
    
    private float coyoteTimeWindow = 0.1f;
    private float timeLastTouchedGround;
    private float displacementYSmoothing;
    
    Vector3 displacement;
    Controller2D controller;

    void Start() {
        controller = GetComponent<Controller2D>();
        playerController = player.GetComponent<CharacterController>();
        
    }

    void Update() {
        // States
        isGrounded = controller.collisions.below;
        isTouchingCeiling = controller.collisions.above;
        isWallSlidingRight = controller.collisions.right && displacement.y < 0 && !isGrounded;
        isWallSlidingLeft = controller.collisions.left && displacement.y < 0 && !isGrounded;
        
        float distance = Vector3.Distance(player.transform.position, transform.position);
        isChasing = distance < chaseDistance;
        
        if (isTouchingCeiling || isGrounded) {
            displacement.y = 0;
        }
        if (isGrounded) {
            timeLastTouchedGround = 0;
            isJumping = false;
            isFalling = false;
        }
        
        // Jumping or Falling
        if (!isGrounded && displacement.y > 0) {
            isJumping = true;
            isFalling = false;
        }
        if (!isGrounded && displacement.y <= 0) {
            isJumping = false;
            isFalling = true;
        }
        
        // Timers
        timeLastTouchedGround += Time.deltaTime;
        jumpCooldown -= Time.deltaTime;
        
        // Run
        horizontalInput = (player.transform.position.x > transform.position.x) ? 1 : -1;
        verticalInput = (player.transform.position.y > transform.position.y) ? 1 : -1;
        float targetdisplacementX = horizontalInput * moveSpeed;
        float targetDisplacementY = verticalInput * moveSpeed;
        if (isChasing) {
            displacement.x = Mathf.SmoothDamp(displacement.x, targetdisplacementX, ref displacementXSmoothing, accelerationTime);
            displacement.y = Mathf.SmoothDamp(displacement.y, targetDisplacementY, ref displacementYSmoothing, accelerationTime);
        }
        else {
            displacement.y = Mathf.Sin(Time.time * 2f) * 2.5f;
            displacement.x = Mathf.SmoothDamp(displacement.x, 0, ref displacementXSmoothing, accelerationTime);
        }
        controller.Move(displacement * Time.deltaTime);
    }
}
