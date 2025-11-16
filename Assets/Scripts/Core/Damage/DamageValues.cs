using UnityEngine;

namespace Core.Damage
{
    public class DamageValues : MonoBehaviour
    {
        [SerializeField] private int damage;

        public int Damage => damage;
    }
}