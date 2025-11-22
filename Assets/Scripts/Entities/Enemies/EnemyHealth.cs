using System.Collections;
using UnityEngine;

namespace Entities.Enemies
{
    public class EnemyHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private int maxHealth = 3;
        private int _currentHealth;
        
        [Header("Public Health Properties")]
        public int MaxHealth => maxHealth;
        public int CurrentHealth => _currentHealth;
        public bool IsDead => _currentHealth <= 0;
        
        [Header("Damage Feedback")]
        [SerializeField] private float knockbackForce = 10f;
        [SerializeField] private float knockbackDuration = 0.2f;
        [SerializeField] private Color damageColor = Color.red;
        [SerializeField] private float damageFlashDuration = 0.1f;
        private SpriteRenderer _spriteRenderer;
        
        [Header("Death")]
        [SerializeField] private GameObject deathEffectPrefab;
        [SerializeField] private AudioClip deathSound;
        [SerializeField] private AudioSource audioSource;
        
        private Color _originalColor;
        private bool _isInvulnerable;

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _originalColor = _spriteRenderer.color;
            _currentHealth = maxHealth;
        }

        public void TakeDamage(int damage)
        {
            if (_isInvulnerable)
            {
                return;
            }

            _currentHealth -= damage;
            
            if (_spriteRenderer)
            {
                StartCoroutine(DamageFlash());
            }

            if (_currentHealth <= 0)
            {
                Die();
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
            if (deathEffectPrefab)
            {
                Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            }
            
            if (audioSource && deathSound)
            {
                audioSource.PlayOneShot(deathSound);
            }
            
            Destroy(gameObject);
        }
    }
}
