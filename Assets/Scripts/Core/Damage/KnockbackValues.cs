using UnityEngine;

namespace Core.Damage
{
    public class KnockbackValues : MonoBehaviour
    {
        [SerializeField] private float knockbackForce = 50f;

        public float KnockbackForce => knockbackForce;
    }
}
