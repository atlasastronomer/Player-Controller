using System;
using System.Collections;
using Core.Damage;
using Core.Raycasts;
using UnityEngine;

namespace Entities.Player
{
    public class PlayerHealth : MonoBehaviour 
    {
        [SerializeField] private LayerMask killBoxMask;
        
        [Header("Audio")]
        [SerializeField] private AudioClip damageTakenSound;
        [SerializeField] private AudioSource damageTakenAudioSource;
    
        [Header("Health Settings")]
        [SerializeField] private int maxHealth;
        [SerializeField] private float gracePeriodCooldown = 0.75f;
        public float GracePeriodCooldown => gracePeriodCooldown;
        
        [Header("UI")]
        public HealthUI healthUI;
        
        [Header("Internal Variables")]
        private int _currentHealth;
        private float _gracePeriod;
        private SpriteRenderer _spriteRenderer;
        private Vector3 _respawnPoint;
        
        // Events
        public static event Action OnDamageTaken;
        public static event Action OnDeath;
    
        private void OnEnable()
        {
            EnemyDetectionRaycastController.OnEnemyCollision += HandleEnemyCollision;
        }

        private void OnDisable()
        {
            EnemyDetectionRaycastController.OnEnemyCollision -= HandleEnemyCollision;
        }
    
        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _respawnPoint = transform.position;
        
            _currentHealth = maxHealth;
            healthUI.SetMaxHearts(maxHealth);
        
            _gracePeriod = gracePeriodCooldown;
        }
    
        private void Update()
        {
            _gracePeriod -= Time.deltaTime;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((killBoxMask.value & (1 << other.transform.gameObject.layer)) > 0)
            {
                damageTakenAudioSource.Stop();
                damageTakenAudioSource.PlayOneShot(damageTakenSound);
                KillPlayer();
            }
        }
        
        private void HandleEnemyCollision(GameObject enemy)
        {
            if (_gracePeriod > 0) 
            {
                return;
            }

            DamageValues damageValues = enemy.GetComponent<DamageValues>();
            KnockbackValues knockbackValues = enemy.GetComponent<KnockbackValues>();

            damageTakenAudioSource.Stop();
            damageTakenAudioSource.PlayOneShot(damageTakenSound);
            OnDamageTaken?.Invoke();

            TakeDamage(damageValues.Damage);
            ApplyKnockback(knockbackValues, enemy);

            _gracePeriod = gracePeriodCooldown;
        }

    
        private void ApplyKnockback(KnockbackValues values, GameObject enemy)
        {
            float horizontalDirection = Mathf.Sign(transform.position.x - enemy.transform.position.x);
        
            if (horizontalDirection == 0) 
            {
                horizontalDirection = 1f;
            }
        
            Vector2 direction = new Vector2(horizontalDirection, 1f).normalized;

            GetComponent<PlayerMovement>().ApplyKnockback(
                direction * values.KnockbackForce,
                0.14f
            );
        }
    
        private void TakeDamage(int damage = 1) 
        {
            _currentHealth -= damage;
            healthUI.updateHearts(_currentHealth);
            StartCoroutine(FlashRed());
            CheckDeath();
        }

        private void KillPlayer()
        {
            _currentHealth = 0;
            healthUI.SetMaxHearts(0);
            CheckDeath();
        }
        
        private void CheckDeath() 
        {
            if (_currentHealth <= 0) 
            {
                OnDeath?.Invoke();
                transform.position = _respawnPoint;
                _currentHealth = maxHealth;
                healthUI.SetMaxHearts(maxHealth);
            }
        }

        private IEnumerator FlashRed() 
        {
            _spriteRenderer.color = new Color(0.996078431372549f, 0.4392156862745098f, 0.4392156862745098f);

            yield return new WaitForSeconds(0.2f);
            _spriteRenderer.color = Color.white;
        }
    }
}