using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour {
    private void OnStartClick() {
        SceneManager.LoadScene("GameScene");
    }
}
