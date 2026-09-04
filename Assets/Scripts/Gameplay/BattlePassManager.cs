using System;
using System.Collections.Generic;
using UnityEngine;

namespace WreckWing.Gameplay
{
    public class BattlePassManager : MonoBehaviour
    {
        public static BattlePassManager Instance { get; private set; }

        [Header("Battle Pass Settings")]
        [SerializeField] private int totalTiers = 100;
        [SerializeField] private int premiumCost = 1000;
        [SerializeField] private int xpPerTier = 500;

        [Header("Runtime State")]
        [SerializeField] private int currentXP;
        [SerializeField] private int currentTier;
        [SerializeField] private bool hasPremium;
        [SerializeField] private List<int> claimedTiers = new List<int>();

        public event Action<int> OnTierUp;
        public event Action<bool> OnPremiumPurchased;
        public event Action<int> OnRewardClaimed;

        public int CurrentXP => currentXP;
        public int CurrentTier => currentTier;
        public bool HasPremium => hasPremium;
        public int TotalTiers => totalTiers;
        public float TierProgress => (float)(currentXP % xpPerTier) / xpPerTier;

        private const string XP_KEY = "BattlePass_XP";
        private const string TIER_KEY = "BattlePass_Tier";
        private const string PREMIUM_KEY = "BattlePass_Premium";
        private const string CLAIMED_KEY = "BattlePass_Claimed";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();
        }

        public void AddXP(int amount)
        {
            if (amount <= 0) return;
            currentXP += amount;

            while (currentXP >= xpPerTier && currentTier < totalTiers)
            {
                currentXP -= xpPerTier;
                currentTier++;
                OnTierUp?.Invoke(currentTier);
            }

            SaveData();
        }

        public void PurchasePremium()
        {
            if (hasPremium) return;
            hasPremium = true;
            SaveData();
            OnPremiumPurchased?.Invoke(true);
        }

        public bool CanClaimTier(int tier)
        {
            if (tier < 1 || tier > currentTier) return false;
            if (claimedTiers.Contains(tier)) return false;
            return true;
        }

        public void ClaimTierReward(int tier)
        {
            if (!CanClaimTier(tier)) return;
            claimedTiers.Add(tier);
            SaveData();
            OnRewardClaimed?.Invoke(tier);
        }

        public bool IsTierClaimed(int tier) => claimedTiers.Contains(tier);

        public void ResetBattlePass()
        {
            currentXP = 0;
            currentTier = 0;
            hasPremium = false;
            claimedTiers.Clear();
            SaveData();
        }

        private void SaveData()
        {
            PlayerPrefs.SetInt(XP_KEY, currentXP);
            PlayerPrefs.SetInt(TIER_KEY, currentTier);
            PlayerPrefs.SetInt(PREMIUM_KEY, hasPremium ? 1 : 0);
            PlayerPrefs.SetString(CLAIMED_KEY, string.Join(",", claimedTiers));
            PlayerPrefs.Save();
        }

        private void LoadData()
        {
            currentXP = PlayerPrefs.GetInt(XP_KEY, 0);
            currentTier = PlayerPrefs.GetInt(TIER_KEY, 0);
            hasPremium = PlayerPrefs.GetInt(PREMIUM_KEY, 0) == 1;
            string claimed = PlayerPrefs.GetString(CLAIMED_KEY, "");
            claimedTiers.Clear();
            if (!string.IsNullOrEmpty(claimed))
            {
                foreach (string s in claimed.Split(','))
                {
                    if (int.TryParse(s, out int tier))
                        claimedTiers.Add(tier);
                }
            }
        }
    }
}
