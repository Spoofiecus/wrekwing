using UnityEngine;

namespace WreckWing.Gameplay
{
    [System.Serializable]
    public struct StadiumThemeData
    {
        public string themeName;
        public Material standsMaterial;
        public Material fieldMaterial;
        public Color lightColor;
        public Color accentColor;
        public Texture2D bannerTexture;
        public int crowdDensity;
    }

    public class StadiumVisualController : MonoBehaviour
    {
        [Header("Lighting")]
        [SerializeField] private Light[] stadiumLights;
        [SerializeField] private Light[] accentLights;

        [Header("Renderers")]
        [SerializeField] private MeshRenderer[] standsRenderers;
        [SerializeField] private MeshRenderer[] fieldRenderers;
        [SerializeField] private MeshRenderer[] bannerRenderers;

        [Header("Crowd")]
        [SerializeField] private GameObject[] crowdMembers;
        [SerializeField] private int maxCrowdDensity = 100;

        [Header("Default Theme")]
        [SerializeField] private Color defaultLightColor = Color.white;
        [SerializeField] private Color defaultAccentColor = Color.blue;

        public void ApplyTheme(StadiumThemeData themeData)
        {
            if (themeData.standsMaterial != null)
                foreach (var r in standsRenderers) if (r != null) r.material = themeData.standsMaterial;

            if (themeData.fieldMaterial != null)
                foreach (var r in fieldRenderers) if (r != null) r.material = themeData.fieldMaterial;

            SetLighting(themeData.lightColor);
            SetAccentColor(themeData.accentColor);

            if (themeData.bannerTexture != null)
                foreach (var r in bannerRenderers) if (r != null) r.material.mainTexture = themeData.bannerTexture;

            SetCrowdDensity(themeData.crowdDensity);
        }

        public void SetLighting(Color color)
        {
            foreach (var light in stadiumLights)
                if (light != null) light.color = color;
        }

        public void SetAccentColor(Color color)
        {
            foreach (var light in accentLights)
                if (light != null) light.color = color;
        }

        public void SetCrowdDensity(int density)
        {
            density = Mathf.Clamp(density, 0, maxCrowdDensity);
            if (crowdMembers == null || crowdMembers.Length == 0) return;

            float ratio = (float)density / maxCrowdDensity;
            int activeCount = Mathf.RoundToInt(crowdMembers.Length * ratio);

            for (int i = 0; i < crowdMembers.Length; i++)
                if (crowdMembers[i] != null) crowdMembers[i].SetActive(i < activeCount);
        }

        public void ResetToDefault()
        {
            SetLighting(defaultLightColor);
            SetAccentColor(defaultAccentColor);
            SetCrowdDensity(maxCrowdDensity);
        }
    }
}