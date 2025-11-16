using Entities.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GemController : MonoBehaviour {
    private static int _totalGems;
    private static int _gemsRemaining;
    
    private void Awake() {
        Gem.OnGemCollect += DecrementGems;
        PlayerHealth.OnDeath += RespawnGems;
    }

    private void Start() {
        _totalGems = transform.childCount;
        _gemsRemaining = _totalGems;
    }

    private void DecrementGems() {
        _gemsRemaining -= 1;
        Debug.Log(_gemsRemaining);

        if (_gemsRemaining == 0) {
            SceneManager.LoadScene("WinScreen");
        }
    }
    
    private void RespawnGems() {
        _gemsRemaining = _totalGems;
        foreach (Transform gem in transform) {
            gem.gameObject.SetActive(true);
        }
    }

    private void OnDestroy() {
        Gem.OnGemCollect -= DecrementGems;
    }
}
