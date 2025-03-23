using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour {
    public int maxHealth = 5;
    private int currentHealth;

    public HealthUI healthUI;
    private SpriteRenderer spriteRenderer; 
    
    void Start()
    {
        currentHealth = maxHealth;
        healthUI.SetMaxHearts(maxHealth);
        
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        GroundedEnemyMovement enemy = collision.GetComponent<GroundedEnemyMovement>();
        if (enemy) {
            TakeDamage(enemy.damage);
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
