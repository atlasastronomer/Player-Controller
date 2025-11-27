using System.Collections;
using UnityEngine;

namespace Entities.Nuri
{
    public class NuriProjectile : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private float speed = 15f;
        [SerializeField] private float lifetime = 5f;
        
        [Header("Damage Settings")]
        [SerializeField] private int damage = 1;
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private LayerMask enemyLayer;
        
        [Header("Audio")]
        [SerializeField] private AudioClip[] hitSounds;
        
        private Vector3 _direction;

        private void OnEnable()
        {
            StartCoroutine(ReturnToPool(lifetime));
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
                
                StartCoroutine(ReturnToPool());
            }
            else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                StartCoroutine(ReturnToPool());
            }
        }

        private IEnumerator ReturnToPool(float waitTime = 0)
        {
            yield return new WaitForSeconds(waitTime);
            
            if (hitEffectPrefab)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }

            if (hitSounds.Length > 0)
            {
                AudioClip hitSound = hitSounds[Random.Range(0, hitSounds.Length)];
                
                PlayClipAtPointCustom(hitSound);
            }
            gameObject.SetActive(false);
        }
        
        private static void PlayClipAtPointCustom(AudioClip clip, float volume = 1f)
        {
            GameObject obj = new GameObject("TempAudio");
            AudioSource source = obj.AddComponent<AudioSource>();

            source.clip = clip;
            source.volume = volume;

            source.spatialBlend = 0f; 
            
            source.minDistance = 1f;
            source.maxDistance = 50f;
            source.rolloffMode = AudioRolloffMode.Linear;

            source.Play();
            Destroy(obj, clip.length);
        }
    }
}
