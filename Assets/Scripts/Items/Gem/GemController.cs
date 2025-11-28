using Entities.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Items.Gem
{
    public class GemController : MonoBehaviour {
        private static int _totalGems;
        private static int _gemsRemaining;
    
        private void Awake()
        {
            global::Gem.OnGemCollect += DecrementGems;
            PlayerHealth.OnDeath += RespawnGems;
        }

        private void Start()
        {
            _totalGems = transform.childCount;
            _gemsRemaining = _totalGems;
        }

        private void DecrementGems()
        {
            _gemsRemaining -= 1;

            if (_gemsRemaining == 0) 
            {
                SceneManager.LoadScene("WinScreen");
            }
        }
    
        private void RespawnGems()
        {
            _gemsRemaining = _totalGems;
            foreach (Transform gem in transform) 
            {
                gem.gameObject.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            global::Gem.OnGemCollect -= DecrementGems;
        }
    }
}
