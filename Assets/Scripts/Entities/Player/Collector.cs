using UnityEngine;

namespace Entities.Player
{
    public class Collector : MonoBehaviour
    {
        [SerializeField] private AudioClip coinCollectSound;
        [SerializeField] private AudioSource coinCollectAudioSource;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Item"))
            {
                IItem item = collision.GetComponent<IItem>();
                if (item != null)
                {
                    coinCollectAudioSource.Stop();
                    coinCollectAudioSource.PlayOneShot(coinCollectSound);
                    item.Collect();
                }
            }
        }
    }
}