using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour {
    public void onStartClick() {
        SceneManager.LoadScene("GameScene");
    }
}
