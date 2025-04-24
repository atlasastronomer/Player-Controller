using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
    // LayerMask specifies the objects we want to collide with
    public LayerMask collisionMask;
    
    // We set a skin width to allow a small amount of space for our rays to fire from.
    // In brief, skin width prevents jittery movement and getting stuck
    public const float skinWidth = .075f;
    
    [HideInInspector]
    public int horizontalRayCount = 4;
    [HideInInspector]
    public int verticalRayCount = 4;
    
    [HideInInspector]
    public BoxCollider2D collider;
    public RayCastOrigins rayCastOrigins;
    
    public float horizontalRaySpacing;
    public float verticalRaySpacing;
    
    // We must have at least 4 rays firing from each corner of our box collider
    public struct RayCastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
    
    
    public virtual void Start()
    {
        collider = GetComponent<BoxCollider2D>();
        calculateRaySpacing();
    }
    
    // Every frame, we update our raycast origins so that their origins begin
    // from the corners of our box collider
    public void UpdateRaycastOrigins()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);
        
        rayCastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        rayCastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        rayCastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        rayCastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }
    
    // The spacing between each ray
    public void calculateRaySpacing()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);
        
        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);
        
        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }
}
