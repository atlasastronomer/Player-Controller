using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour {
    public AudioClip damageTakenSound;
    public AudioSource audioSource;
    
    public int maxHealth = 5;
    private int currentHealth;

    public HealthUI healthUI;
    private SpriteRenderer spriteRenderer;
    
    [SerializeField] public float gracePeriodCooldown = 0.75f;
    private float gracePeriod = 0;
    
    public static event Action OnDamageTaken;
    
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        currentHealth = maxHealth;
        healthUI.SetMaxHearts(maxHealth);
        
        gracePeriod = gracePeriodCooldown;
    }

    private void Update()
    {
        gracePeriod -=  Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        DamageValues enemy = collision.GetComponent<DamageValues>();
        if (enemy && gracePeriod <= 0) {
            audioSource.Stop();
            audioSource.PlayOneShot(damageTakenSound);
            
            OnDamageTaken?.Invoke();
            TakeDamage(enemy.damage);
            gracePeriod = gracePeriodCooldown;
        }
    }

    private void TakeDamage(int damage = 1) {
        currentHealth -= damage;
        healthUI.updateHearts(currentHealth);
        
        // Flash Red
        StartCoroutine(FlashRed());
        if (currentHealth <= 0) {
            SceneManager.LoadScene("GameOver");
        }
    }

    private IEnumerator FlashRed() {
        spriteRenderer.color = new Color(0.996078431372549f,0.4392156862745098f,0.4392156862745098f);

        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = Color.white;
    }
}
