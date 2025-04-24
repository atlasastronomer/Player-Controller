using UnityEngine;
using UnityEngine.SceneManagement;

public class GemController : MonoBehaviour {
    public static int gemsRemaining;

    void Awake() {
        Gem.OnGemCollect += DecrementGems;
    }
    void Start() {
        gemsRemaining = transform.childCount;
        Debug.Log(gemsRemaining);
    }

    private void DecrementGems() {
        gemsRemaining -= 1;
        Debug.Log(gemsRemaining);

        if (gemsRemaining == 0) {
            SceneManager.LoadScene("WinScreen");
        }
    }
    
    void OnDestroy() {
        // Unsubscribe from the event to prevent multiple subscriptions
        Gem.OnGemCollect -= DecrementGems;
    }
}
