using Entities.Player;
using UnityEngine;

namespace Core.Respawn
{
    public class ObjectRespawn : MonoBehaviour
    {
        private Vector3 _initialLocalPosition;
        private Quaternion _initialLocalRotation;
        
        private void Awake()
        {
            _initialLocalPosition = transform.localPosition;
            _initialLocalRotation = transform.localRotation;
        }
        
        private void OnEnable()
        {
            PlayerHealth.OnDeath += Respawn;
        }
        
        private void OnDestroy()
        {
            PlayerHealth.OnDeath -= Respawn;
        }
        
        private void Respawn()
        {
            gameObject.SetActive(false);
            
            transform.localPosition = _initialLocalPosition;
            transform.localRotation = _initialLocalRotation;
            
            gameObject.SetActive(true);
        }
    }
}