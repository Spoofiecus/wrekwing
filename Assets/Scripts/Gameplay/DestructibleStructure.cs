using UnityEngine;
using WreckWing.Core;

namespace WreckWing.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class DestructibleStructure : MonoBehaviour
    {
        [Header("Structure Settings")]
        [SerializeField] private StructureMaterial material = StructureMaterial.Concrete;
        [SerializeField] private float customBreakThreshold = -1f;
        [SerializeField] private bool isLoadBearing = false;
        [SerializeField] private float structuralImportance = 1f;

        [Header("Visual")]
        [SerializeField] private GameObject intactModel;
        [SerializeField] private GameObject destroyedModel;
        [SerializeField] private ParticleSystem destructionParticles;

        [Header("Audio")]
        [SerializeField] private AudioClip destructionSound;

        [Header("Chain Reaction")]
        [SerializeField] private DestructibleStructure[] connectedStructures;
        [SerializeField] private float chainReactionRadius = 10f;

        private bool _isDestroyed;
        private float _currentIntegrity;
        private float _breakThreshold;
        private Collider _collider;
        private Rigidbody _rigidbody;

        public System.Action<DestructibleStructure> OnDestroyed;

        public bool IsDestroyed => _isDestroyed;
        public float BreakThreshold => _breakThreshold;
        public StructureMaterial Material => material;
        public bool IsLoadBearing => isLoadBearing;
        public float StructuralImportance => structuralImportance;
        public float IntegrityPercent => _currentIntegrity / _breakThreshold;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _rigidbody = GetComponent<Rigidbody>();

            if (customBreakThreshold > 0)
                _breakThreshold = customBreakThreshold;
            else if (DestructionManager.Instance != null)
                _breakThreshold = DestructionManager.Instance.GetBreakThreshold(material);
            else
                _breakThreshold = 50f;

            _currentIntegrity = _breakThreshold;
            _isDestroyed = false;
        }

        private void Start()
        {
            if (DestructionManager.Instance != null)
                DestructionManager.Instance.RegisterStructure(this);
            UpdateVisuals();
        }

        public void BreakStructure(float force)
        {
            if (_isDestroyed) return;
            _isDestroyed = true;
            _currentIntegrity = 0f;
            UpdateVisuals();

            if (destructionParticles != null)
                destructionParticles.Play();

            if (destructionSound != null)
                AudioSource.PlayClipAtPoint(destructionSound, transform.position);

            SpawnDebris();
            TriggerChainReaction(force);
            OnDestroyed?.Invoke(this);
        }

        private void UpdateVisuals()
        {
            if (intactModel != null)
                intactModel.SetActive(!_isDestroyed);
            if (destroyedModel != null)
                destroyedModel.SetActive(_isDestroyed);
            if (_collider != null)
                _collider.enabled = !_isDestroyed;
        }

        private void SpawnDebris()
        {
            if (DestructionManager.Instance == null) return;
            Vector3 spawnPosition = transform.position;
            Vector3 force = Random.insideUnitSphere * 20f;
            force.y = Mathf.Abs(force.y);
            DestructionManager.Instance.SpawnDebris(spawnPosition, force, (DebrisMaterial)material);
        }

        private void TriggerChainReaction(float force)
        {
            if (connectedStructures == null) return;
            foreach (DestructibleStructure connected in connectedStructures)
            {
                if (connected == null || connected.IsDestroyed) continue;
                float distance = Vector3.Distance(transform.position, connected.transform.position);
                if (distance <= chainReactionRadius)
                {
                    float reducedForce = force * (1f - (distance / chainReactionRadius));
                    connected.TakeDamage(reducedForce * 0.5f);
                }
            }
        }

        public void TakeDamage(float damage)
        {
            if (_isDestroyed) return;
            _currentIntegrity -= damage;
            if (_currentIntegrity <= 0f)
                BreakStructure(Mathf.Abs(damage));
        }

        public void ResetStructure()
        {
            _isDestroyed = false;
            _currentIntegrity = _breakThreshold;
            UpdateVisuals();
        }

        private void OnDestroy()
        {
            if (DestructionManager.Instance != null)
                DestructionManager.Instance.UnregisterStructure(this);
        }
    }
}
