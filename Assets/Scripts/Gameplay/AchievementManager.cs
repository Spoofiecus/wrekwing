using System;
using System.Collections.Generic;
using UnityEngine;

namespace WreckWing.Data
{
    [Serializable]
    public class AchievementData
    {
        public string achievementId;
        public bool isUnlocked;
        public int progress;
    }
}

namespace WreckWing.Gameplay
{
    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance { get; private set; }

        public event Action<string> OnAchievementUnlocked;

        private List<string> unlockedAchievements = new List<string>();
        private Dictionary<string, int> achievementProgress = new Dictionary<string, int>();

        private const string AchievementsKey = "Achievements_Unlocked";
        private const string ProgressPrefix = "Achievement_Progress_";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        public void UnlockAchievement(string achievementId)
        {
            if (string.IsNullOrEmpty(achievementId) || unlockedAchievements.Contains(achievementId)) return;
            unlockedAchievements.Add(achievementId);
            Save();
            OnAchievementUnlocked?.Invoke(achievementId);
        }

        public bool IsAchievementUnlocked(string achievementId)
        {
            if (string.IsNullOrEmpty(achievementId)) return false;
            return unlockedAchievements.Contains(achievementId);
        }

        public int GetAchievementProgress(string achievementId)
        {
            if (string.IsNullOrEmpty(achievementId)) return 0;
            return achievementProgress.ContainsKey(achievementId) ? achievementProgress[achievementId] : 0;
        }

        public void SetAchievementProgress(string achievementId, int progress)
        {
            if (string.IsNullOrEmpty(achievementId)) return;
            achievementProgress[achievementId] = progress;
            PlayerPrefs.SetInt(ProgressPrefix + achievementId, progress);
            PlayerPrefs.Save();
        }

        public void Save()
        {
            string json = string.Join(",", unlockedAchievements);
            PlayerPrefs.SetString(AchievementsKey, json);
            PlayerPrefs.Save();
        }

        public void Load()
        {
            string json = PlayerPrefs.GetString(AchievementsKey, "");
            unlockedAchievements = string.IsNullOrEmpty(json)
                ? new List<string>()
                : new List<string>(json.Split(','));
        }
    }
}
