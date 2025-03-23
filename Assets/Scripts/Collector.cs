using UnityEngine;

public class Collector : MonoBehaviour {
    public AudioClip coinCollectSound;
    public AudioSource audioSource;
    
    void Start() {
        audioSource = GetComponent<AudioSource>();
    }
    
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Item")) {
            iItem item = collision.GetComponent<iItem>();
            if (item != null) {
                audioSource.Stop();
                audioSource.PlayOneShot(coinCollectSound);
                item.Collect();
            } 
        }
    }
}