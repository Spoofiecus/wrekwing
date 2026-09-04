using System.Collections.Generic;
using UnityEngine;

namespace WreckWing.Core
{
    /// <summary>
    /// Generic object pooling system for efficient memory management.
    /// Prevents garbage collection spikes during gameplay.
    /// </summary>
    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler Instance { get; private set; }

        [System.Serializable]
        public class Pool
        {
            public string tag;
            public GameObject prefab;
            public int initialSize = 10;
            public bool expandable = true;
        }

        [SerializeField] private List<Pool> pools;
        private Dictionary<string, Queue<GameObject>> poolDictionary;
        private Dictionary<string, Pool> poolConfigs;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializePools();
        }

        /// <summary>
        /// Initialize all pools with their configured sizes.
        /// </summary>
        private void InitializePools()
        {
            poolDictionary = new Dictionary<string, Queue<GameObject>>();
            poolConfigs = new Dictionary<string, Pool>();

            foreach (Pool pool in pools)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();
                poolConfigs[pool.tag] = pool;

                for (int i = 0; i < pool.initialSize; i++)
                {
                    GameObject obj = CreateNewObject(pool);
                    objectPool.Enqueue(obj);
                }

                poolDictionary[pool.tag] = objectPool;
            }
        }

        /// <summary>
        /// Create a new object for the pool.
        /// </summary>
        private GameObject CreateNewObject(Pool pool)
        {
            GameObject obj = Instantiate(pool.prefab, transform);
            obj.SetActive(false);
            obj.name = pool.tag;
            return obj;
        }

        /// <summary>
        /// Spawn an object from the pool at the specified position and rotation.
        /// </summary>
        public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPooler] Pool with tag '{tag}' doesn't exist.");
                return null;
            }

            Queue<GameObject> pool = poolDictionary[tag];
            GameObject obj;

            if (pool.Count > 0)
            {
                obj = pool.Dequeue();
            }
            else if (poolConfigs[tag].expandable)
            {
                obj = CreateNewObject(poolConfigs[tag]);
            }
            else
            {
                Debug.LogWarning($"[ObjectPooler] Pool '{tag}' is empty and not expandable.");
                return null;
            }

            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);

            // Call IPoolable interface if available
            IPoolable poolable = obj.GetComponent<IPoolable>();
            poolable?.OnSpawnFromPool();

            return obj;
        }

        /// <summary>
        /// Return an object to the pool.
        /// </summary>
        public void ReturnToPool(string tag, GameObject obj)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPooler] Pool with tag '{tag}' doesn't exist.");
                Destroy(obj);
                return;
            }

            // Call IPoolable interface if available
            IPoolable poolable = obj.GetComponent<IPoolable>();
            poolable?.OnReturnToPool();

            obj.SetActive(false);
            obj.transform.SetParent(transform);
            poolDictionary[tag].Enqueue(obj);
        }

        /// <summary>
        /// Get the current count of available objects in a pool.
        /// </summary>
        public int GetPoolCount(string tag)
        {
            if (!poolDictionary.ContainsKey(tag))
                return 0;

            return poolDictionary[tag].Count;
        }

        /// <summary>
        /// Clear all objects from a specific pool.
        /// </summary>
        public void ClearPool(string tag)
        {
            if (!poolDictionary.ContainsKey(tag))
                return;

            while (poolDictionary[tag].Count > 0)
            {
                GameObject obj = poolDictionary[tag].Dequeue();
                if (obj != null)
                {
                    Destroy(obj);
                }
            }

            poolDictionary[tag].Clear();
        }

        /// <summary>
        /// Clear all pools.
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var tag in poolDictionary.Keys)
            {
                ClearPool(tag);
            }
        }
    }

    /// <summary>
    /// Interface for objects that need to handle pool events.
    /// </summary>
    public interface IPoolable
    {
        void OnSpawnFromPool();
        void OnReturnToPool();
    }
}
