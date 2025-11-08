using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour {
    /** Audio */
    [SerializeField] private AudioClip damageTakenSound;
    [SerializeField] private AudioSource damageTakenAudioSource;
    
    /** Configuration */
    [SerializeField] private int maxHealth;
    private int _currentHealth;
    private float _gracePeriod;
    [SerializeField] public float gracePeriodCooldown = 0.75f;
    public static event Action OnDamageTaken;
    public static event Action OnDeath;

    public HealthUI healthUI;
    private SpriteRenderer _spriteRenderer;
    
    private Vector3 _respawnPoint;
    
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
        _gracePeriod -=  Time.deltaTime;
    }
    
    private void OnTriggerStay2D(Collider2D collision) {
        DamageValues enemy = collision.GetComponent<DamageValues>();
        if (enemy && _gracePeriod <= 0) {
            damageTakenAudioSource.Stop();
            damageTakenAudioSource.PlayOneShot(damageTakenSound);
            
            OnDamageTaken?.Invoke();
            TakeDamage(enemy.Damage);
            _gracePeriod = gracePeriodCooldown;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.layer == 11) {
            _currentHealth = 0;
            damageTakenAudioSource.PlayOneShot(damageTakenSound);
            CheckDeath();
        }
    }

    private void TakeDamage(int damage = 1) {
        _currentHealth -= damage;
        healthUI.updateHearts(_currentHealth);
        StartCoroutine(FlashRed());
        CheckDeath();
    }

    private void CheckDeath() {
        if (_currentHealth <= 0) {
            OnDeath?.Invoke();
            transform.position = _respawnPoint;
            _currentHealth = maxHealth;
            healthUI.SetMaxHearts(maxHealth);
        }
    }

    private IEnumerator FlashRed() {
        _spriteRenderer.color = new Color(0.996078431372549f,0.4392156862745098f,0.4392156862745098f);

        yield return new WaitForSeconds(0.2f);
        _spriteRenderer.color = Color.white;
    }
}
