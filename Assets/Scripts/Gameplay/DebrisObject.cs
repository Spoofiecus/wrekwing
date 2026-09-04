using UnityEngine;
using WreckWing.Core;

namespace WreckWing.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public class DebrisObject : MonoBehaviour, IPoolable
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private float defaultLifetime = 5f;

        private float _lifetime;
        private float _spawnTime;
        private bool _isInitialized;

        private void Update()
        {
            if (!_isInitialized) return;
            if (Time.time - _spawnTime >= _lifetime)
                ReturnToPool();
        }

        public void Initialize(DebrisMaterial material, float lifetime)
        {
            _lifetime = lifetime;
            _spawnTime = Time.time;
            _isInitialized = true;
            ApplyMaterialVisuals(material);
        }

        private void ApplyMaterialVisuals(DebrisMaterial material)
        {
            if (meshRenderer == null) return;
            switch (material)
            {
                case DebrisMaterial.Concrete:
                    meshRenderer.material.color = Color.gray;
                    break;
                case DebrisMaterial.Glass:
                    meshRenderer.material.color = new Color(0.7f, 0.9f, 1f, 0.5f);
                    break;
                case DebrisMaterial.Wood:
                    meshRenderer.material.color = new Color(0.55f, 0.27f, 0.07f);
                    break;
                case DebrisMaterial.Metal:
                    meshRenderer.material.color = new Color(0.6f, 0.6f, 0.7f);
                    break;
            }
        }

        public void OnSpawnFromPool()
        {
            _isInitialized = false;
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        public void OnReturnToPool()
        {
            _isInitialized = false;
            ObjectPooler.Instance?.ReturnToPool("Debris", gameObject);
        }

        private void ReturnToPool()
        {
            OnReturnToPool();
        }
    }
}
