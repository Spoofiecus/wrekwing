using UnityEngine;

namespace WreckWing.Core
{
    public static class QualityManager
    {
        public static QualityTier CurrentTier { get; private set; }
        public static bool IsAdaptiveQualityEnabled { get; set; } = true;

        private static float _frameTimeAccumulator;
        private static int _frameCount;
        private static float _checkInterval = 2f;
        private static float _lastCheckTime;
        private static int _consecutiveLowFPSCount;
        private static int _consecutiveHighFPSCount;

        private const int LOWFPS_THRESHOLD = 3;
        private const int HIGHFPS_THRESHOLD = 5;
        private const float LOW_FRAMETIME_THRESHOLD = 1f / 45f;
        private const float HIGH_FRAMETIME_THRESHOLD = 1f / 55f;

        public static void Initialize()
        {
            CurrentTier = DetectDeviceTier();
            ApplyQualityTier(CurrentTier);
            _lastCheckTime = Time.unscaledTime;
            _frameTimeAccumulator = 0f;
            _frameCount = 0;
        }

        public static void Update()
        {
            if (!IsAdaptiveQualityEnabled)
                return;

            _frameTimeAccumulator += Time.unscaledDeltaTime;
            _frameCount++;

            if (Time.unscaledTime - _lastCheckTime >= _checkInterval)
            {
                EvaluatePerformance();
                _lastCheckTime = Time.unscaledTime;
            }
        }

        private static void EvaluatePerformance()
        {
            if (_frameCount == 0)
                return;

            float averageFrameTime = _frameTimeAccumulator / _frameCount;
            float currentFPS = 1f / averageFrameTime;

            _frameTimeAccumulator = 0f;
            _frameCount = 0;

            if (averageFrameTime > LOW_FRAMETIME_THRESHOLD)
            {
                _consecutiveLowFPSCount++;
                _consecutiveHighFPSCount = 0;

                if (_consecutiveLowFPSCount >= LOWFPS_THRESHOLD && CurrentTier > QualityTier.Low)
                {
                    CurrentTier--;
                    ApplyQualityTier(CurrentTier);
                    _consecutiveLowFPSCount = 0;
                }
            }
            else if (averageFrameTime < HIGH_FRAMETIME_THRESHOLD)
            {
                _consecutiveHighFPSCount++;
                _consecutiveLowFPSCount = 0;

                if (_consecutiveHighFPSCount >= HIGHFPS_THRESHOLD && CurrentTier < QualityTier.High)
                {
                    CurrentTier++;
                    ApplyQualityTier(CurrentTier);
                    _consecutiveHighFPSCount = 0;
                }
            }
            else
            {
                _consecutiveLowFPSCount = 0;
                _consecutiveHighFPSCount = 0;
            }
        }

        private static QualityTier DetectDeviceTier()
        {
            int systemMemoryMB = SystemInfo.systemMemorySize;
            string gpuName = SystemInfo.graphicsDeviceName.ToLower();
            int gpuMemoryMB = SystemInfo.graphicsMemorySize;

            if (systemMemoryMB >= 6000 && gpuMemoryMB >= 3000)
            {
                if (gpuName.Contains("adreno 6") || gpuName.Contains("adreno 7") ||
                    gpuName.Contains("mali-g7") || gpuName.Contains("mali-g8"))
                {
                    return QualityTier.High;
                }
            }

            if (systemMemoryMB >= 3000 && gpuMemoryMB >= 1500)
            {
                return QualityTier.Medium;
            }

            return QualityTier.Low;
        }

        public static void ApplyQualityTier(QualityTier tier)
        {
            CurrentTier = tier;

            switch (tier)
            {
                case QualityTier.Low:
                    ApplyLowQuality();
                    break;
                case QualityTier.Medium:
                    ApplyMediumQuality();
                    break;
                case QualityTier.High:
                    ApplyHighQuality();
                    break;
            }
        }

        private static void ApplyLowQuality()
        {
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.antiAliasing = 0;
            QualitySettings.pixelLightCount = 1;
            QualitySettings.lodBias = 0.5f;
            Application.targetFrameRate = 30;
        }

        private static void ApplyMediumQuality()
        {
            QualitySettings.shadows = ShadowQuality.HardOnly;
            QualitySettings.shadowResolution = ShadowResolution.Low;
            QualitySettings.antiAliasing = 2;
            QualitySettings.pixelLightCount = 2;
            QualitySettings.lodBias = 1.0f;
            Application.targetFrameRate = 60;
        }

        private static void ApplyHighQuality()
        {
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowResolution = ShadowResolution.Medium;
            QualitySettings.antiAliasing = 4;
            QualitySettings.pixelLightCount = 4;
            QualitySettings.lodBias = 1.5f;
            Application.targetFrameRate = 60;
        }

        public static void SetQualityTier(QualityTier tier)
        {
            IsAdaptiveQualityEnabled = false;
            ApplyQualityTier(tier);
        }
    }

    public enum QualityTier
    {
        Low = 0,
        Medium = 1,
        High = 2
    }
}
