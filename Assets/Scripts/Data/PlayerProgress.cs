using System;
using UnityEngine;

namespace WreckWing.Data
{
    [Serializable]
    public class PlayerProgress
    {
        public int playerLevel = 1;
        public int xp;
        public int xpToNextLevel = 100;

        public event Action<int> OnLevelUp;
        public event Action<int, int> OnXPChanged;

        private const string LevelKey = "Progress_Level";
        private const string XPKey = "Progress_XP";
        private const string XPToNextKey = "Progress_XPToNext";

        public void AddXP(int amount)
        {
            if (amount <= 0) return;
            xp += amount;

            while (xp >= xpToNextLevel)
            {
                xp -= xpToNextLevel;
                LevelUp();
            }

            Save();
            OnXPChanged?.Invoke(xp, xpToNextLevel);
        }

        public void LevelUp()
        {
            playerLevel++;
            xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.5f);
            Save();
            OnLevelUp?.Invoke(playerLevel);
        }

        public float GetLevelProgress()
        {
            if (xpToNextLevel <= 0) return 0f;
            return (float)xp / xpToNextLevel;
        }

        public void Save()
        {
            PlayerPrefs.SetInt(LevelKey, playerLevel);
            PlayerPrefs.SetInt(XPKey, xp);
            PlayerPrefs.SetInt(XPToNextKey, xpToNextLevel);
            PlayerPrefs.Save();
        }

        public void Load()
        {
            playerLevel = PlayerPrefs.GetInt(LevelKey, 1);
            xp = PlayerPrefs.GetInt(XPKey, 0);
            xpToNextLevel = PlayerPrefs.GetInt(XPToNextKey, 100);
        }
    }
}
