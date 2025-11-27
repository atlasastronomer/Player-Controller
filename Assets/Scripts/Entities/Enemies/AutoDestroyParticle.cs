using UnityEngine;

namespace Entities.Enemies
{
    public class AutoDestroyParticle : MonoBehaviour
    {
        private ParticleSystem _ps;

        private void Awake()
        {
            _ps = GetComponent<ParticleSystem>();
        }

        private void Update()
        {
            if (!_ps || !_ps.IsAlive())
            {
                Destroy(gameObject);
            }
        }
    }
}
