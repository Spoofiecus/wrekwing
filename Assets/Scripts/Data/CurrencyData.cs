using System;
using UnityEngine;

namespace WreckWing.Data
{
    [Serializable]
    public class CurrencyData
    {
        public int coins;
        public int gems;

        public event Action<int> OnCoinsChanged;
        public event Action<int> OnGemsChanged;

        private const string CoinsKey = "Currency_Coins";
        private const string GemsKey = "Currency_Gems";

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            coins += amount;
            Save();
            OnCoinsChanged?.Invoke(coins);
        }

        public bool SpendCoins(int amount)
        {
            if (amount <= 0 || coins < amount) return false;
            coins -= amount;
            Save();
            OnCoinsChanged?.Invoke(coins);
            return true;
        }

        public void AddGems(int amount)
        {
            if (amount <= 0) return;
            gems += amount;
            Save();
            OnGemsChanged?.Invoke(gems);
        }

        public bool SpendGems(int amount)
        {
            if (amount <= 0 || gems < amount) return false;
            gems -= amount;
            Save();
            OnGemsChanged?.Invoke(gems);
            return true;
        }

        public void Save()
        {
            PlayerPrefs.SetInt(CoinsKey, coins);
            PlayerPrefs.SetInt(GemsKey, gems);
            PlayerPrefs.Save();
        }

        public void Load()
        {
            coins = PlayerPrefs.GetInt(CoinsKey, 0);
            gems = PlayerPrefs.GetInt(GemsKey, 0);
        }
    }
}
