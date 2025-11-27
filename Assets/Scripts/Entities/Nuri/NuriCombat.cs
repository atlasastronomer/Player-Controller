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
        private GameObject _lockedTarget;
        private GameObject _previousLockedTarget;
        private bool _isLockOnEnabled;
        private Camera _mainCamera;

        private void Start()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                ToggleLockOn();
            }

            if (_isLockOnEnabled)
            {
                _lockedTarget = FindNearestEnemy();
                
                if (!_lockedTarget)
                {
                    _isLockOnEnabled = false;
                }
            }

            UpdateTargetHighlight();

            if (Input.GetMouseButtonDown(0) && Time.time >= _lastAttackTime + attackCooldown)
            {
                ShootProjectile();
                _lastAttackTime = Time.time;
            }
        }

        private void ToggleLockOn()
        {
            _isLockOnEnabled = !_isLockOnEnabled;

            if (_isLockOnEnabled)
            {
                _lockedTarget = FindNearestEnemy();

                if (!_lockedTarget)
                {
                    _isLockOnEnabled = false;
                }
            }
            else
            {
                RemoveHighlight();
                _lockedTarget = null;
            }
        }

        private GameObject FindNearestEnemy()
        {
            Collider2D[] enemies = Physics2D.OverlapCircleAll(player.transform.position, targetingRange, enemyLayer);
            
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
            if (!projectilePrefab)
            {
                return;
            }

            Vector3 shootDirection;

            if (_isLockOnEnabled && _lockedTarget)
                shootDirection = (_lockedTarget.transform.position - transform.position).normalized;
            else
            {
                Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos.z = 0f;
                shootDirection = (mouseWorldPos - transform.position).normalized;
            }
            
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            NuriProjectile projectileScript = projectile.GetComponent<NuriProjectile>();
            if (projectileScript)
            {
                projectileScript.SetDirection(shootDirection);
            }

            if (audioSource && shootSound)
            {
                audioSource.PlayOneShot(shootSound);
            }
        }

        private void UpdateTargetHighlight()
        {
            if (_previousLockedTarget == _lockedTarget)
            {
                return;
            }

            if (_previousLockedTarget)
            {
                TargetHighlight highlight = _previousLockedTarget.GetComponent<TargetHighlight>();
                if (highlight)
                {
                    highlight.DisableHighlight();
                }
            }
            if (_lockedTarget)
            {
                TargetHighlight highlight = _lockedTarget.GetComponent<TargetHighlight>();
                if (highlight) highlight.EnableHighlight();
            }

            _previousLockedTarget = _lockedTarget;
        }

        private void RemoveHighlight()
        {
            if (_previousLockedTarget)
            {
                TargetHighlight highlight = _previousLockedTarget.GetComponent<TargetHighlight>();
                if (highlight)
                {
                    highlight.DisableHighlight();
                }
            }

            _previousLockedTarget = null;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!player)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.transform.position, targetingRange);
            
            if (_isLockOnEnabled && _lockedTarget)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(player.transform.position, _lockedTarget.transform.position);
            }
        }
    }
}
