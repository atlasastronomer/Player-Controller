using UnityEngine;

namespace Entities.Nuri
{
    public class NuriCombat : MonoBehaviour
    {
        [Header("Combat Settings")]
        [SerializeField] private GameObject player;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float attackCooldown = 0.3f;
        [SerializeField] private float targetingRange = 15f;
        [SerializeField] private LayerMask enemyLayer;

        [Header("Audio")]
        [SerializeField] private AudioClip shootSound;
        [SerializeField] private AudioSource audioSource;

        private float _lastAttackTime = Mathf.NegativeInfinity;
        private GameObject _currentTarget;

        private void Update()
        {
            _currentTarget = FindNearestEnemy();

            if (Input.GetKey(KeyCode.E) && Time.time >= _lastAttackTime + attackCooldown && _currentTarget != null)
            {
                ShootProjectile();
                _lastAttackTime = Time.time;
            }
        }

        private GameObject FindNearestEnemy()
        {
            Collider2D[] enemies = Physics2D.OverlapCircleAll(player.transform.position, targetingRange, enemyLayer);
            
            Debug.Log($"Found {enemies.Length} enemies within range");
            foreach (var enemy in enemies)
            {
                Debug.Log($"Enemy found: {enemy.name} on layer {LayerMask.LayerToName(enemy.gameObject.layer)}");
            }
            
            if (enemies.Length == 0)
            {
                return null;
            }

            GameObject nearestEnemy = null;
            float nearestDistance = Mathf.Infinity;

            foreach (Collider2D enemy in enemies)
            {
                float distance = Vector3.Distance(player.transform.position, enemy.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy.gameObject;
                }
            }

            return nearestEnemy;
        }

        private void ShootProjectile()
        {
            if (!projectilePrefab || !_currentTarget)
            {
                return;
            }
            
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            NuriProjectile projectileScript = projectile.GetComponent<NuriProjectile>();
            if (projectileScript)
            {
                projectileScript.SetTarget(_currentTarget);
            }

            if (audioSource && shootSound)
            {
                audioSource.PlayOneShot(shootSound);
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.transform.position, targetingRange);
            
            if (_currentTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(player.transform.position, _currentTarget.transform.position);
            }
        }
    }
}
