using UnityEngine;

public class BackgroundController : MonoBehaviour {
    private float _startPosition;
    [SerializeField] private GameObject cam;
    [SerializeField] private float parallaxEffectX;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        _startPosition = transform.position.x;
    }

    // Update is called once per frame
    void Update() {
        float distanceX = cam.transform.position.x * parallaxEffectX; // 0 = move with cam || 1 = won't move || 0.5 = half
        
        transform.position = new Vector3(_startPosition + distanceX, transform.position.y, transform.position.z);
    }
}
