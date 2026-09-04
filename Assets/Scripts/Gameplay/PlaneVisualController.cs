using UnityEngine;

namespace WreckWing.Gameplay
{
    /// <summary>
    /// Controls plane visual customization including skins, trails, and emission.
    /// </summary>
    [System.Serializable]
    public struct PlaneSkinData
    {
        public string skinName;
        public Material bodyMaterial;
        public Material wingMaterial;
        public Texture2D decalTexture;
        public Color primaryColor;
        public Color secondaryColor;
    }

    public class PlaneVisualController : MonoBehaviour
    {
        [Header("Renderers")]
        [SerializeField] private MeshRenderer bodyRenderer;
        [SerializeField] private MeshRenderer wingRenderer;
        [SerializeField] private MeshRenderer detailRenderer;

        [Header("Effects")]
        [SerializeField] private ParticleSystem trailEffect;
        [SerializeField] private ParticleSystem[] emissionParticles;

        [Header("Emission")]
        [SerializeField] private float emissionIntensity = 1.5f;
        [SerializeField] private string emissionColorProperty = "_EmissionColor";

        private GameObject activeTrailObject;
        private MaterialPropertyBlock propertyBlock;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        /// <summary>
        /// Applies a complete skin to the plane.
        /// </summary>
        public void ApplySkin(PlaneSkinData skinData)
        {
            if (skinData.bodyMaterial != null && bodyRenderer != null)
                bodyRenderer.material = skinData.bodyMaterial;

            if (skinData.wingMaterial != null && wingRenderer != null)
                wingRenderer.material = skinData.wingMaterial;

            if (skinData.decalTexture != null && detailRenderer != null)
                detailRenderer.material.mainTexture = skinData.decalTexture;

            SetEmissionColor(skinData.primaryColor * emissionIntensity);
        }

        /// <summary>
        /// Sets the trail effect prefab for the plane.
        /// </summary>
        public void SetTrailEffect(GameObject trailPrefab)
        {
            if (activeTrailObject != null)
                Destroy(activeTrailObject);

            if (trailPrefab != null)
            {
                activeTrailObject = Instantiate(trailPrefab, transform);
                activeTrailObject.transform.localPosition = Vector3.zero;
                activeTrailObject.transform.localRotation = Quaternion.identity;
            }
        }

        /// <summary>
        /// Sets the emission color across all relevant materials.
        /// </summary>
        public void SetEmissionColor(Color color)
        {
            if (bodyRenderer != null)
            {
                bodyRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(emissionColorProperty, color);
                bodyRenderer.SetPropertyBlock(propertyBlock);
            }

            if (trailEffect != null)
            {
                var main = trailEffect.main;
                main.startColor = color;
            }

            foreach (var ps in emissionParticles)
            {
                if (ps != null)
                {
                    var main = ps.main;
                    main.startColor = color;
                }
            }
        }

        /// <summary>
        /// Sets emission intensity multiplier.
        /// </summary>
        public void SetEmissionIntensity(float intensity)
        {
            emissionIntensity = intensity;
        }

        /// <summary>
        /// Enables or disables the trail effect.
        /// </summary>
        public void SetTrailActive(bool active)
        {
            if (trailEffect != null)
            {
                if (active)
                    trailEffect.Play();
                else
                    trailEffect.Stop();
            }
        }
    }
}