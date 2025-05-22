using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour {
    /** Audio */
    public AudioClip damageTakenSound;
    public AudioSource audioSource;
    
    /** Configuration */
    [SerializeField] private int maxHealth;
    private int _currentHealth;
    private float _gracePeriod;
    [SerializeField] public float gracePeriodCooldown = 0.75f;
    public static event Action OnDamageTaken;
    public static event Action OnDeath;

    public HealthUI healthUI;
    private SpriteRenderer _spriteRenderer;

    /** Respawn Point */
    private Vector3 _respawnPoint;
    
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
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

    private void OnTriggerEnter2D(Collider2D collision) {
        DamageValues enemy = collision.GetComponent<DamageValues>();
        if (enemy && _gracePeriod <= 0) {
            audioSource.Stop();
            audioSource.PlayOneShot(damageTakenSound);
            
            OnDamageTaken?.Invoke();
            TakeDamage(enemy._damage);
            _gracePeriod = gracePeriodCooldown;
        }
    }

    private void TakeDamage(int damage = 1) {
        _currentHealth -= damage;
        healthUI.updateHearts(_currentHealth);
        
        // Flash Red
        StartCoroutine(FlashRed());
        
        // Death Handling
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
