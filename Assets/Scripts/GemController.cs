using UnityEngine;
using UnityEngine.SceneManagement;

public class GemController : MonoBehaviour {
    public static int gemsRemaining;
    void Start()
    {
        gemsRemaining = transform.childCount;
        Debug.Log(gemsRemaining);
        Gem.OnGemCollect += DecrementGems;

    }

    private void DecrementGems() {
        gemsRemaining = gemsRemaining - 1;
        Debug.Log(gemsRemaining);

        if (gemsRemaining == 0) {
            SceneManager.LoadScene("WinScreen");
        }
    }
}
