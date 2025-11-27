using System.Collections.Generic;
using UnityEngine;

namespace Core.Respawn
{
    public class ObjectPooler : MonoBehaviour
    {
        public GameObject prefab;
        public int poolSize = 10;

        private List<GameObject> _pool;

        void Start()
        {
            InitializePool();
        }

        private void InitializePool()
        {
            _pool = new List<GameObject>();

            for (int i = 0; i < poolSize; i++)
            {
                CreateNewObject();
            }
        }

        public GameObject GetPooledObject()
        {
            foreach (GameObject obj in _pool)
            {
                if (!obj.activeInHierarchy)
                {
                    obj.SetActive(true);
                    return obj;
                }
            }

            return CreateNewObject();
        }
        
        private GameObject CreateNewObject()
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            _pool.Add(obj);

            return obj;
        }
    }
}
