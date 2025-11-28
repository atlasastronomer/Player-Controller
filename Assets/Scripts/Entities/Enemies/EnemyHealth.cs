using System.Collections;
using Entities.Enemies.Flying_Enemy;
using Entities.Enemies.Grounded_Enemy;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Entities.Enemies
{
    public class EnemyHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private int maxHealth = 3;
        private int _currentHealth;
        
        [Header("Damage Feedback")]
        [SerializeField] private float knockbackForce = 10f;
        [SerializeField] private float knockbackDuration = 0.2f;
        [SerializeField] private Color damageColor = Color.red;
        [SerializeField] private float damageFlashDuration = 0.1f;
        private SpriteRenderer _spriteRenderer;
        
        [Header("Death")]
        [SerializeField] private GameObject deathEffectPrefab;
        [SerializeField] private AudioClip[] deathSounds;
        [SerializeField] private AudioSource audioSource;
        
        private Color _originalColor;
        private bool _isInvulnerable;

        private void OnEnable()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _originalColor = _spriteRenderer.color;
            _spriteRenderer.color = _originalColor;
            _currentHealth = maxHealth;
            _isInvulnerable = false;

            GroundedEnemyMovement groundedEnemyMovement = GetComponent<GroundedEnemyMovement>();
            if (groundedEnemyMovement) groundedEnemyMovement.enabled = true;

            FlyingEnemyMovement flyingEnemyMovement = GetComponent<FlyingEnemyMovement>();
            if (flyingEnemyMovement) flyingEnemyMovement.enabled = true;
            
            BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
            if (boxCollider) boxCollider.enabled = true;
        }
        
        public void TakeDamage(int damage, Vector3 knockbackDirection)
        {
            if (_isInvulnerable)
            {
                return;
            }

            _currentHealth -= damage;
            
            if (_spriteRenderer != null)
            {
                StartCoroutine(DamageFlash());
            }
            
            if (knockbackDirection != Vector3.zero)
            {
                ApplyKnockback(knockbackDirection);
            }

            if (_currentHealth <= 0)
            {
                Die();
            }
        }
        
        private void ApplyKnockback(Vector3 direction)
        {
            GroundedEnemyMovement groundedMovement = GetComponent<GroundedEnemyMovement>();
            if (groundedMovement != null)
            {
                Vector3 knockback = direction.normalized * knockbackForce;
                groundedMovement.ApplyKnockback(knockback, knockbackDuration);
                return;
            }
            
            FlyingEnemyMovement flyingMovement = GetComponent<FlyingEnemyMovement>();
            if (flyingMovement != null)
            {
                Vector3 knockback = direction.normalized * knockbackForce;
                flyingMovement.ApplyKnockback(knockback, knockbackDuration);
            }
        }

        private IEnumerator DamageFlash()
        {
            _isInvulnerable = true;
            
            _spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(damageFlashDuration);
            _spriteRenderer.color = _originalColor;
            
            _isInvulnerable = false;
        }

        private void Die()
        {
            StartCoroutine(DeathSequence());
        }

        private IEnumerator DeathSequence()
        {
            GroundedEnemyMovement groundedMovement = GetComponent<GroundedEnemyMovement>();
            FlyingEnemyMovement flyingMovement = GetComponent<FlyingEnemyMovement>();
            BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
            
            if (groundedMovement) groundedMovement.enabled = false;
            if (flyingMovement) flyingMovement.enabled = false;
            if (boxCollider) boxCollider.enabled = false;

            float flickerTime = 0.5f;
            float interval = 0.05f;
            float elapsed = 0f;
            
            Vector3 originalPos = transform.localPosition;

            while (elapsed < flickerTime)
            {
                _spriteRenderer.color = Color.white;
                transform.localPosition = originalPos + (Vector3)Random.insideUnitCircle * 0.05f;
                
                yield return new WaitForSeconds(interval);
                
                _spriteRenderer.color = _originalColor * 0.6f;
                
                transform.localPosition = originalPos;
                
                yield return new WaitForSeconds(interval);
                
                elapsed += interval * 2f;
            }
            
            transform.localPosition = originalPos;

            if (deathEffectPrefab)
            {
                Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            }

            if (audioSource && deathSounds.Length > 0)
            {
                AudioClip deathSound = deathSounds[Random.Range(0, deathSounds.Length)];
                audioSource.PlayOneShot(deathSound);
            }
            
            gameObject.SetActive(false);
        }
    }
}
