using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class PlatformController : RaycastController
{
    public Vector3 move;
    public LayerMask passengerMask;

    private List<PassengerMovement> passengerMovement;
    private Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

    public override void Start() {
        base.Start();
    }

    void Update() { 
        UpdateRaycastOrigins();

        Vector3 displacement = move * Time.deltaTime;
        
        CalculatePassengerMovement(displacement);
        
        MovePassengers(true);
        transform.Translate(displacement);
        MovePassengers(false);
    }

    void MovePassengers(bool beforeMovePlatform) {
        foreach (PassengerMovement passenger in passengerMovement) {
            if (!passengerDictionary.ContainsKey(passenger.transform)) {
                passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller2D>());
            }
            
            if (passenger.moveBeforePlatform == beforeMovePlatform) {
                passengerDictionary[passenger.transform].Move(passenger.displacement, passenger.standingOnPlatform);
            }
        }
    }
    
    void CalculatePassengerMovement(Vector3 displacement) {
        HashSet<Transform> movePassengers = new HashSet<Transform>();
        passengerMovement = new List<PassengerMovement>();
        
        float directionX = Mathf.Sign(displacement.x);
        float directionY = Mathf.Sign(displacement.y);

        // Vertically moving platform
        if (displacement.y != 0) {
            float rayLength = Mathf.Abs(displacement.y) + skinWidth;

            for (int i = 0; i < verticalRayCount; i++) {
                Vector2 rayOrigin = (directionY == -1) ? rayCastOrigins.bottomLeft : rayCastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);
                
                if (hit) {
                    if (!movePassengers.Contains(hit.transform)) {
                        movePassengers.Add(hit.transform);
                        
                        float pushX = (directionY == 1) ? displacement.x : 0;
                        float pushY = displacement.y - (hit.distance - skinWidth) * directionY;
                    
                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY),directionY == 1, true));
                    }
                }
            }
        }
        
        // Horizontally moving platforms
        if (displacement.x != 0) {
            float rayLength = Mathf.Abs(displacement.x) + skinWidth;

            for (int i = 0; i < horizontalRayCount; i++) {
                Vector2 rayOrigin = (directionX == -1) ? rayCastOrigins.bottomLeft : rayCastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);
                
                if (hit) {
                    if (!movePassengers.Contains(hit.transform)) {
                        movePassengers.Add(hit.transform);
                        float pushX = displacement.x - (hit.distance - skinWidth) * directionX;
                        float pushY = -skinWidth;
                    
                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY),false, true));
                    }
                }
            }
        }
        
        // Passenger on top of horizontally or downward moving platform
        if (directionY == -1 || displacement.y == 0 && displacement.x != 0) {
            float rayLength = skinWidth * 2;
            
            for (int i = 0; i < verticalRayCount; i++) {
                Vector2 rayOrigin = rayCastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

                if (hit) {
                    if (!movePassengers.Contains(hit.transform)) {
                        movePassengers.Add(hit.transform);
                        float pushX = displacement.x;
                        float pushY = displacement.y;
                            
                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
                    }
                }
            }
        }
    }

    struct PassengerMovement {
        public Transform transform;
        public Vector3 displacement;
        public bool standingOnPlatform;
        public bool moveBeforePlatform;

        public PassengerMovement(Transform _transform, Vector3 _displacement, bool _standingOnPlatform, bool _moveBeforePlatform)
        {
            transform = _transform;
            displacement = _displacement;
            standingOnPlatform = _standingOnPlatform;
            moveBeforePlatform = _moveBeforePlatform;
        }
    }
}