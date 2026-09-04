using UnityEngine;
using WreckWing.Core;

namespace WreckWing.VFX
{
    public class VFXManager : MonoBehaviour
    {
        public static VFXManager Instance { get; private set; }

        [Header("VFX Prefabs")]
        [SerializeField] private GameObject impactPrefab;
        [SerializeField] private GameObject destructionPrefab;
        [SerializeField] private GameObject dustPrefab;

        [Header("Pool Settings")]
        [SerializeField] private int poolSize = 10;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void PlayImpactEffect(Vector3 position)
        {
            if (impactPrefab == null) return;
            GameObject effect = ObjectPooler.Instance?.SpawnFromPool("ImpactVFX", position, Quaternion.identity);
            if (effect == null) effect = Instantiate(impactPrefab, position, Quaternion.identity);
            ReturnAfterDelay(effect, 2f, "ImpactVFX");
        }

        public void PlayDestructionEffect(Vector3 position)
        {
            if (destructionPrefab == null) return;
            GameObject effect = ObjectPooler.Instance?.SpawnFromPool("DestructionVFX", position, Quaternion.identity);
            if (effect == null) effect = Instantiate(destructionPrefab, position, Quaternion.identity);
            ReturnAfterDelay(effect, 3f, "DestructionVFX");
        }

        public void PlayDustEffect(Vector3 position)
        {
            if (dustPrefab == null) return;
            GameObject effect = ObjectPooler.Instance?.SpawnFromPool("DustVFX", position, Quaternion.identity);
            if (effect == null) effect = Instantiate(dustPrefab, position, Quaternion.identity);
            ReturnAfterDelay(effect, 4f, "DustVFX");
        }

        private void ReturnAfterDelay(GameObject obj, float delay, string poolTag)
        {
            if (obj == null) return;
            StartCoroutine(ReturnCoroutine(obj, delay, poolTag));
        }

        private System.Collections.IEnumerator ReturnCoroutine(GameObject obj, float delay, string poolTag)
        {
            yield return new WaitForSeconds(delay);
            if (obj != null)
            {
                ObjectPooler.Instance?.ReturnToPool(poolTag, obj);
            }
        }
    }
}
