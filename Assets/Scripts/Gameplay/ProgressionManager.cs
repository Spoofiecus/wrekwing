using System;
using System.Collections.Generic;
using UnityEngine;

namespace WreckWing.Gameplay
{
    public class ProgressionManager : MonoBehaviour
    {
        public static ProgressionManager Instance { get; private set; }

        public Data.CurrencyData CurrencyData { get; private set; }
        public Data.PlayerProgress PlayerProgress { get; private set; }
        public Data.PlaneUnlockData PlaneUnlockData { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            CurrencyData = new Data.CurrencyData();
            PlayerProgress = new Data.PlayerProgress();
            PlaneUnlockData = new Data.PlaneUnlockData();

            LoadAll();
        }

        public void AddCoins(int amount) => CurrencyData.AddCoins(amount);
        public bool SpendCoins(int amount) => CurrencyData.SpendCoins(amount);
        public void AddGems(int amount) => CurrencyData.AddGems(amount);
        public bool SpendGems(int amount) => CurrencyData.SpendGems(amount);
        public void AddXP(int amount) => PlayerProgress.AddXP(amount);

        public void UnlockPlane(string planeId) => PlaneUnlockData.UnlockPlane(planeId);
        public bool IsPlaneUnlocked(string planeId) => PlaneUnlockData.IsPlaneUnlocked(planeId);
        public void SelectPlane(string planeId) => PlaneUnlockData.SelectPlane(planeId);

        public void SaveAll()
        {
            CurrencyData.Save();
            PlayerProgress.Save();
            PlaneUnlockData.Save();
        }

        public void LoadAll()
        {
            CurrencyData.Load();
            PlayerProgress.Load();
            PlaneUnlockData.Load();
        }
    }
}
