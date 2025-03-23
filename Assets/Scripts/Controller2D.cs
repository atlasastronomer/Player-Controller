using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : RaycastController
{
    
    float maxClimbAngle = 46;
    float maxDescendAngle = 46;
    
    // Booleans that tell us where our raycasts are detecting collision
    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public bool climbingSlope;
        public bool descendingSlope;
        public float slopeAngle, slopeAngleOld;
        
        public int facingDirection;
        
        public Vector3 displacementOld;
        
        // Every frame, we reset the booleans
        public void Reset()
        {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false;
            
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }

    public override void Start()
    {
        base.Start();
        collisions.facingDirection = 1;
    }

    public CollisionInfo collisions;
    
    // Keyword ref means the function directly modifies displacement.
    public void Move(Vector3 displacement, bool standingOnPlatform = false)
    {
        UpdateRaycastOrigins();
        collisions.Reset();
        collisions.displacementOld = displacement;
        // Note! We only for collisions when our velocity is zero because these are movement collisions.
        // Other objects (e.g. cars) would have their own collision detection

        if (displacement.x != 0) {
            collisions.facingDirection = (int)Mathf.Sign(displacement.x);
        }
        HorizontalCollisions(ref displacement);
        
        if (displacement.y < 0)
        {
            DescendSlope(ref displacement);
        }
        
        if (displacement.y != 0)
        {
            VerticalCollisions(ref displacement);
        }
        transform.Translate(displacement);
        
        if (standingOnPlatform) {
            collisions.below = true;
        }
    }

    // Same logic from VerticalCollisions() applies here, except we go from x to y-axis
    void HorizontalCollisions(ref Vector3 displacement)
    {
        float directionX;
        if (displacement.x == 0) {
            directionX = collisions.facingDirection;
        }
        else {
            directionX = Mathf.Sign(displacement.x);
        }
        
        float rayLength = Mathf.Abs(displacement.x) + skinWidth;

        if (Math.Abs(displacement.x) < skinWidth) {
            rayLength = 2 * skinWidth;
        }
        
        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? rayCastOrigins.bottomLeft : rayCastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength,
                collisionMask);
            
            Debug.DrawRay(rayOrigin, Vector2.right * directionX,Color.red);
            
            if (hit)
            {

                if (hit.distance == 0)
                {
                    continue;
                }
                
                // When our raycast hits an angled surface, it stores the surface
                // as normal, or a vector perpendicular to the surface
                // If we use global up as our second direction, we can obtain
                // the slope angle
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                
                // We only proceed to climb the slope if the slope is less than or equal to
                // the maximum angle at which we can climb
                if (i == 0 && slopeAngle <= maxClimbAngle)
                {
                    if (collisions.descendingSlope)
                    {
                        collisions.descendingSlope = false;
                        displacement = collisions.displacementOld;
                    }
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != collisions.slopeAngleOld)
                    {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        displacement.x -= distanceToSlopeStart * directionX;
                    }
                    ClimbSlope(ref displacement, slopeAngle);
                    displacement.x += distanceToSlopeStart * directionX;
                }

                if (!collisions.climbingSlope || slopeAngle > maxClimbAngle)
                {
                    displacement.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance;

                    if (collisions.climbingSlope)
                    {
                        displacement.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(displacement.x);
                    }
                    collisions.left = directionX == -1;
                    collisions.right = directionX == 1;
                }
            }
        } 
    }

    void VerticalCollisions(ref Vector3 displacement)
    {
        float directionY = Mathf.Sign(displacement.y);
        // The length of our rays will be equal to our displacement plus the length of our skin width
        float rayLength = Mathf.Abs(displacement.y) + skinWidth;
        
        // When we're moving down, we want our rays to start from the bottom left corner.
        // When we're moving up, we want our rays to start from the top left corner
        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? rayCastOrigins.bottomLeft : rayCastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + displacement.x);
            // Boolean for the layer 
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY,Color.red);
            if (hit)
            {
                // If our raycast hits something above or below, we set the y displacement to the amount we have to move
                // to get from our current position to the point where a ray intersected with an obstacle
                // Note that if we do not do this, we sink through the floor
                // Finally, we update ray length to equal the distance of hit to ensure our rays do not collide
                // with multiple objects (e.g. a ledge elevated slightly off the ground)
                displacement.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;

                if (collisions.climbingSlope)
                {
                    displacement.x = displacement.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(displacement.y);
                }
                
                collisions.below = directionY == -1;
                collisions.above = directionY == 1;
            }
        }
        // When we move on a "curved" slope, we get stuck for a frame since our collision2D moves inside the slope
        // before getting ejected out again. To prevent this, we fire a single horizontal ray to check if there's a new slope
        if (collisions.climbingSlope)
        {
            float directionX = Mathf.Sign(displacement.x);
            rayLength = Mathf.Abs(displacement.x) + skinWidth;
            Vector2 rayOrigin = ((directionX == -1) ? rayCastOrigins.bottomLeft : rayCastOrigins.bottomRight) + Vector2.up * displacement.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != collisions.slopeAngleOld)
                {
                    displacement.x = (hit.distance - skinWidth) * directionX;
                    collisions.slopeAngle = slopeAngle;
                }
            }
        }
    }
    
    
    private void ClimbSlope(ref Vector3 displacement, float slopeAngle)
    {
        float moveDistance = Mathf.Abs(displacement.x);
        float climbdisplacementY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
        if (displacement.y <= climbdisplacementY)
        {
            displacement.y = climbdisplacementY;
            displacement.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(displacement.x);
            collisions.below = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
        }
    }
    
    private void DescendSlope(ref Vector3 displacement)
    {
        float directionX = Mathf.Sign(displacement.x);
        Vector2 rayOrigin = (directionX == -1) ? rayCastOrigins.bottomRight : rayCastOrigins.bottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);
        
        if (hit) {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0 && slopeAngle <= maxDescendAngle) {
                if (Mathf.Sign(hit.normal.x) == directionX) {
                    // This if statement checks for if we're actually touching the slope
                    // (since we're casting a ray with infinite length)
                    if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(displacement.x)) {
                        float moveDistance = Mathf.Abs(displacement.x);
                        float descenddisplacementY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                        displacement.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(displacement.x);
                        displacement.y -= descenddisplacementY;
                        
                        collisions.slopeAngle = slopeAngle;
                        collisions.descendingSlope = true;
                        collisions.below = true;
                    }
                }
            }
        }
    }
}
