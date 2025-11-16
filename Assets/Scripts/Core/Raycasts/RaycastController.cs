using UnityEngine;

namespace Core.Raycasts
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class RaycastController : MonoBehaviour
    {
        public LayerMask collisionMask;

        protected const float SkinWidth = .075f;

        [HideInInspector] public int horizontalRayCount = 4;
        [HideInInspector] public int verticalRayCount = 4;

        [HideInInspector] public new BoxCollider2D collider;
        protected RayCastOrigins rayCastOrigins;

        public float horizontalRaySpacing;
        public float verticalRaySpacing;

        protected struct RayCastOrigins
        {
            public Vector2 TopLeft, TopRight;
            public Vector2 BottomLeft, BottomRight;
        }


        public virtual void Start()
        {
            collider = GetComponent<BoxCollider2D>();
            CalculateRaySpacing();
        }

        protected void UpdateRaycastOrigins()
        {
            Bounds bounds = collider.bounds;
            bounds.Expand(SkinWidth * -2);

            rayCastOrigins.BottomLeft = new Vector2(bounds.min.x, bounds.min.y);
            rayCastOrigins.BottomRight = new Vector2(bounds.max.x, bounds.min.y);
            rayCastOrigins.TopLeft = new Vector2(bounds.min.x, bounds.max.y);
            rayCastOrigins.TopRight = new Vector2(bounds.max.x, bounds.max.y);
        }

        private void CalculateRaySpacing()
        {
            Bounds bounds = collider.bounds;
            bounds.Expand(SkinWidth * -2);

            horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
            verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

            horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
            verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
        }
    }
}