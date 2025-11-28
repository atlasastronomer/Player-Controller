using Entities.Player;
using UnityEngine;

namespace Core.Respawn
{
    public class ObjectRespawn : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;


        private void OnEnable()
        {
            PlayerHealth.OnDeath += Respawn;
        }
        
        private void OnDestroy()
        {
            PlayerHealth.OnDeath -= Respawn;
        }
        
        private void Respawn()
        {
            gameObject.SetActive(false);
            
            transform.position = spawnPoint.position;
            
            gameObject.SetActive(true);
        }
    }
}
