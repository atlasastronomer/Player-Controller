using UnityEngine;

public class EnemyRespawn : MonoBehaviour
{
    private Vector3 _respawnPosition;

    private void Start() {
        _respawnPosition = transform.position;
        PlayerHealth.OnDeath += Respawn;
    }
    
    private void OnDestroy() {
        PlayerHealth.OnDeath -= Respawn;
    }

    private void Respawn() {
        transform.position = _respawnPosition;
    }
}
