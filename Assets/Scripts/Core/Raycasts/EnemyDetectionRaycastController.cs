using UnityEngine;
using System;

public class EnemyDetectionRaycastController : RaycastController 
{
    public static Action<GameObject> OnEnemyCollision;

    public override void Start() 
    {
        horizontalRayCount = 8;
        verticalRayCount = 8;
        base.Start();
    }
    
    void FixedUpdate()
    {
        DetectEnemies();
    }
    
    void DetectEnemies()
    {
        UpdateRaycastOrigins();
        RaycastHit2D hit;
        
        for (int i = 0; i < horizontalRayCount; i++)
        {
            var rayOriginLeft = rayCastOrigins.BottomLeft + Vector2.up * (horizontalRaySpacing * i);
            var rayOriginRight = rayCastOrigins.BottomRight + Vector2.up * (horizontalRaySpacing * i);

            Debug.DrawRay(rayOriginLeft, Vector2.left, Color.green);
            Debug.DrawRay(rayOriginRight, Vector2.right, Color.green);

            hit = Physics2D.Raycast(rayOriginLeft, Vector2.left, SkinWidth + 0.1f, collisionMask);
            if (hit)
            {
                OnEnemyCollision?.Invoke(hit.transform.gameObject);
            }
            hit = Physics2D.Raycast(rayOriginRight, Vector2.right, SkinWidth + 0.1f, collisionMask);
            if (hit)
            {
                OnEnemyCollision?.Invoke(hit.transform.gameObject);
            }
        }
        
        for (int i = 0; i < verticalRayCount; i++)
        {
            var rayOriginTop = rayCastOrigins.TopLeft + Vector2.right * (verticalRaySpacing * i);
            var rayOriginBottom = rayCastOrigins.BottomLeft + Vector2.right * (verticalRaySpacing * i);

            Debug.DrawRay(rayOriginTop, Vector2.up, Color.green);
            Debug.DrawRay(rayOriginBottom, Vector2.down, Color.green);

            hit = Physics2D.Raycast(rayOriginTop, Vector2.up, SkinWidth + 0.1f, collisionMask);
            if (hit)
            {
                OnEnemyCollision?.Invoke(hit.transform.gameObject);
            }
            hit = Physics2D.Raycast(rayOriginBottom, Vector2.down, SkinWidth + 0.1f, collisionMask);
            if (hit)
            {
                OnEnemyCollision?.Invoke(hit.transform.gameObject);
            }
        }
    }
}
