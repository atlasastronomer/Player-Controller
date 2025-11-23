using UnityEngine;

namespace Entities.Nuri
{
    public class NuriProjectile : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float speed = 15f;
        [SerializeField] private float lifetime = 5f;
        
        [Header("Damage Settings")]
        [SerializeField] private int damage = 1;
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private LayerMask enemyLayer;
        
        private Vector3 _direction;

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            transform.position += _direction * (speed * Time.deltaTime);
            
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        
        public void SetDirection(Vector3 direction)
        {
            _direction = direction.normalized;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
            {
                var enemyHealth = collision.GetComponent<Enemies.EnemyHealth>();
                if (enemyHealth)
                {
                    Vector3 knockbackDirection = _direction;
                    enemyHealth.TakeDamage(damage, knockbackDirection);
                }

                if (hitEffectPrefab)
                {
                    Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                }

                Destroy(gameObject);
            }
            else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                if (hitEffectPrefab)
                {
                    Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                }

                Destroy(gameObject);
            }
        }
    }
}