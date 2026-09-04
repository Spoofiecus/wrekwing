using UnityEngine;
using WreckWing.Core;

namespace WreckWing.VFX
{
    public static class VFXQualityScaler
    {
        private static float _globalMultiplier = 1f;

        public static void SetGlobalMultiplier(float multiplier)
        {
            _globalMultiplier = Mathf.Clamp(multiplier, 0.1f, 2f);
        }

        public static int GetMaxParticles()
        {
            switch (QualityManager.CurrentTier)
            {
                case QualityTier.Low: return Mathf.RoundToInt(200 * _globalMultiplier);
                case QualityTier.Medium: return Mathf.RoundToInt(500 * _globalMultiplier);
                case QualityTier.High: return Mathf.RoundToInt(1000 * _globalMultiplier);
                default: return 500;
            }
        }

        public static int GetDebrisCount()
        {
            switch (QualityManager.CurrentTier)
            {
                case QualityTier.Low: return Mathf.RoundToInt(10 * _globalMultiplier);
                case QualityTier.Medium: return Mathf.RoundToInt(25 * _globalMultiplier);
                case QualityTier.High: return Mathf.RoundToInt(50 * _globalMultiplier);
                default: return 25;
            }
        }

        public static float GetVFXLifetimeMultiplier()
        {
            switch (QualityManager.CurrentTier)
            {
                case QualityTier.Low: return 0.5f;
                case QualityTier.Medium: return 0.75f;
                case QualityTier.High: return 1f;
                default: return 0.75f;
            }
        }

        public static int GetMaxActiveVFX()
        {
            switch (QualityManager.CurrentTier)
            {
                case QualityTier.Low: return 5;
                case QualityTier.Medium: return 10;
                case QualityTier.High: return 20;
                default: return 10;
            }
        }
    }
}
