using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : RaycastController
{
    private readonly float _maxClimbAngle = 46;
    private readonly float _maxDescendAngle = 48;
    
    public struct CollisionInfo
    {
        public bool Above, Below;
        public bool Left, Right;

        public bool ClimbingSlope;
        public bool DescendingSlope;
        public float SlopeAngle, SlopeAngleOld;
        
        public int MovementDirection;
        
        public Vector3 DisplacementOld;
        
        public void Reset()
        {
            Above = Below = false;
            Left = Right = false;
            ClimbingSlope = false;
            DescendingSlope = false;
            
            SlopeAngleOld = SlopeAngle;
            SlopeAngle = 0;
        }
    }

    public override void Start()
    {
        base.Start();
        Collisions.MovementDirection = 1;
    }

    public CollisionInfo Collisions;
    
    public void Move(Vector3 displacement, bool standingOnPlatform = false)
    {
        UpdateRaycastOrigins();
        Collisions.Reset();
        Collisions.DisplacementOld = displacement;

        if (displacement.x != 0) 
        {
            Collisions.MovementDirection = (int)Mathf.Sign(displacement.x);
        }
        
        HorizontalCollisions(ref displacement);
        
        if (displacement.y <= 0)
        {
            DescendSlope(ref displacement);
        }
        
        if (displacement.y != 0)
        {
            VerticalCollisions(ref displacement);
        }
        
        transform.Translate(displacement);
        
        if (standingOnPlatform) 
        {
            Collisions.Below = true;
        }
    }
    
    private void HorizontalCollisions(ref Vector3 displacement)
    {
        float directionX;
        if (displacement.x == 0) 
        {
            directionX = Collisions.MovementDirection;
        }
        else 
        {
            directionX = Mathf.Sign(displacement.x);
        }
        
        float rayLength = Mathf.Abs(displacement.x) + SkinWidth;

        if (Math.Abs(displacement.x) < SkinWidth) 
        {
            rayLength = 2 * SkinWidth;
        }
        
        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (Mathf.Approximately(directionX, -1)) ? rayCastOrigins.BottomLeft : rayCastOrigins.BottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            
            Debug.DrawRay(rayOrigin, Vector2.right * directionX, Color.red);
            
            if (hit)
            {
                if (hit.distance == 0)
                {
                    continue;
                }
                
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                
                if (i == 0 && slopeAngle <= _maxClimbAngle)
                {
                    if (Collisions.DescendingSlope)
                    {
                        Collisions.DescendingSlope = false;
                        displacement = Collisions.DisplacementOld;
                    }
                    
                    float distanceToSlopeStart = 0;
                    if (!Mathf.Approximately(slopeAngle, Collisions.SlopeAngleOld))
                    {
                        distanceToSlopeStart = hit.distance - SkinWidth;
                        displacement.x -= distanceToSlopeStart * directionX;
                    }
                    
                    ClimbSlope(ref displacement, slopeAngle);
                    displacement.x += distanceToSlopeStart * directionX;
                }

                if (!Collisions.ClimbingSlope || slopeAngle > _maxClimbAngle)
                {
                    displacement.x = (hit.distance - SkinWidth) * directionX;
                    rayLength = hit.distance;

                    if (Collisions.ClimbingSlope)
                    {
                        displacement.y = Mathf.Tan(Collisions.SlopeAngle * Mathf.Deg2Rad) * Mathf.Abs(displacement.x);
                    }
                    
                    Collisions.Left = Mathf.Approximately(directionX, -1);
                    Collisions.Right = Mathf.Approximately(directionX, 1);
                }
            }
        } 
    }

    private void VerticalCollisions(ref Vector3 displacement)
    {
        float directionY = Mathf.Sign(displacement.y);
        float rayLength = Mathf.Abs(displacement.y) + SkinWidth;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (Mathf.Approximately(directionY, -1)) ? rayCastOrigins.BottomLeft : rayCastOrigins.TopLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + displacement.x);

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY, Color.red);
            
            if (hit)
            {
                displacement.y = (hit.distance - SkinWidth) * directionY;
                rayLength = hit.distance;

                if (Collisions.ClimbingSlope)
                {
                    displacement.x = displacement.y / Mathf.Tan(Collisions.SlopeAngle * Mathf.Deg2Rad) * Mathf.Abs(displacement.y);
                }
                
                Collisions.Below = Mathf.Approximately(directionY, -1);
                Collisions.Above = Mathf.Approximately(directionY, 1);
            }
        }

        if (Collisions.ClimbingSlope)
        {
            float directionX = Mathf.Sign(displacement.x);
            rayLength = Mathf.Abs(displacement.x) + SkinWidth;
            Vector2 rayOrigin = ((Mathf.Approximately(directionX, -1)) ? rayCastOrigins.BottomLeft : rayCastOrigins.BottomRight) + Vector2.up * displacement.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            
            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (!Mathf.Approximately(slopeAngle, Collisions.SlopeAngleOld))
                {
                    displacement.x = (hit.distance - SkinWidth) * directionX;
                    Collisions.SlopeAngle = slopeAngle;
                }
            }
        }
    }
    
    private void ClimbSlope(ref Vector3 displacement, float slopeAngle)
    {
        float moveDistance = Mathf.Abs(displacement.x);
        float climbDisplacementY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
        
        if (displacement.y <= climbDisplacementY)
        {
            displacement.y = climbDisplacementY;
            displacement.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(displacement.x);
            Collisions.Below = true;
            Collisions.ClimbingSlope = true;
            Collisions.SlopeAngle = slopeAngle;
        }
    }
    
    private void DescendSlope(ref Vector3 displacement)
    {
        float directionX = Mathf.Sign(displacement.x);
        Vector2 rayOrigin = (Mathf.Approximately(directionX, -1)) ? rayCastOrigins.BottomRight : rayCastOrigins.BottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);
        
        if (hit) 
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            
            if (slopeAngle != 0 && slopeAngle <= _maxDescendAngle) 
            {
                if (Mathf.Approximately(Mathf.Sign(hit.normal.x), Mathf.Sign(Collisions.MovementDirection))) 
                {
                    if (hit.distance - SkinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(displacement.x)) 
                    {
                        float moveDistance = Mathf.Abs(displacement.x);
                        float descendDisplacementY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                        displacement.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(displacement.x);
                        displacement.y -= descendDisplacementY;
                        
                        Collisions.SlopeAngle = slopeAngle;
                        Collisions.DescendingSlope = true;
                        Collisions.Below = true;
                    }
                }
            }
        }
    }
}