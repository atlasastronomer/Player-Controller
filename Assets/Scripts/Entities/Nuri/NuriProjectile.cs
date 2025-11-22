using UnityEngine;

namespace Entities.Nuri
{
    public class NuriProjectile : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float speed = 15f;
        [SerializeField] private float rotationSpeed = 200f;
        [SerializeField] private float lifetime = 5f;
        
        [Header("Damage Settings")]
        [SerializeField] private int damage = 1;
        [SerializeField] private GameObject hitEffectPrefab;

        [SerializeField] private LayerMask enemyLayer;
        
        private GameObject _target;
        private Vector3 _direction;

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            if (!_target)
            {
                transform.position += _direction * (speed * Time.deltaTime);
                return;
            }
            
            Vector3 targetDirection = (_target.transform.position - transform.position).normalized;
            
            _direction = Vector3.RotateTowards(_direction, targetDirection, rotationSpeed * Mathf.Deg2Rad * Time.deltaTime, 0.0f);
            
            transform.position += _direction * (speed * Time.deltaTime);
            
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        
        public void SetTarget(GameObject target)
        {
            _target = target;
            if (_target)
            {
                _direction = (_target.transform.position - transform.position).normalized;
            }
            else
            {
                _direction = Vector3.right;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject == _target || ((1 << collision.gameObject.layer) & enemyLayer) != 0)
            {
                var enemyHealth = collision.GetComponent<Entities.Enemies.EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);
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
