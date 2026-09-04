using System;
using UnityEngine;
using UnityEngine.Events;

namespace WreckWing.Gameplay
{
    /// <summary>
    /// Represents a destructible section of the stadium.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class StadiumSection : MonoBehaviour
    {
        [Header("Section Info")]
        [SerializeField] private string sectionName = "Section";
        [SerializeField] private bool isVIPZone = false;
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private SpriteRenderer[] visualRenderers;
        [SerializeField] private GameObject destroyedVisuals;
        [SerializeField] private ParticleSystem destructionEffect;

        [Header("Events")]
        public UnityEvent OnSectionDestroyed;
        public UnityEvent<float> OnDamageTaken;

        private float currentHealth;
        private Collider destructionZone;
        private bool isDestroyed;

        public string SectionName => sectionName;
        public bool IsVIPZone => isVIPZone;
        public bool IsDestroyed => isDestroyed;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public Collider DestructionZone => destructionZone;

        private void Awake()
        {
            destructionZone = GetComponent<Collider>();
            currentHealth = maxHealth;
            isDestroyed = false;

            if (destroyedVisuals != null)
                destroyedVisuals.SetActive(false);
        }

        private void OnEnable()
        {
            ResetSection();
        }

        /// <summary>
        /// Returns the percentage of destruction (0 = pristine, 1 = fully destroyed).
        /// </summary>
        public float GetDestructionPercent()
        {
            if (maxHealth <= 0f)
                return 1f;

            return Mathf.Clamp01(1f - (currentHealth / maxHealth));
        }

        /// <summary>
        /// Resets the section to its original state.
        /// </summary>
        public void ResetSection()
        {
            currentHealth = maxHealth;
            isDestroyed = false;

            if (destroyedVisuals != null)
                destroyedVisuals.SetActive(false);

            foreach (var renderer in visualRenderers)
            {
                if (renderer != null)
                    renderer.enabled = true;
            }
        }

        /// <summary>
        /// Applies damage to this stadium section.
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (isDestroyed || damage <= 0f)
                return;

            currentHealth = Mathf.Max(0f, currentHealth - damage);
            OnDamageTaken?.Invoke(GetDestructionPercent());

            if (currentHealth <= 0f)
                DestroySection();
        }

        private void DestroySection()
        {
            isDestroyed = true;

            if (destructionEffect != null)
                destructionEffect.Play();

            if (destroyedVisuals != null)
                destroyedVisuals.SetActive(true);

            foreach (var renderer in visualRenderers)
            {
                if (renderer != null)
                    renderer.enabled = false;
            }

            OnSectionDestroyed?.Invoke();
        }
    }
}