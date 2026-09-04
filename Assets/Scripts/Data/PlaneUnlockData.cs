using System;
using System.Collections.Generic;
using UnityEngine;

namespace WreckWing.Data
{
    [Serializable]
    public class PlaneUnlockData
    {
        public List<string> unlockedPlanes = new List<string>();
        public string selectedPlane;

        public event Action<string> OnPlaneUnlocked;
        public event Action<string> OnPlaneSelected;

        private const string UnlockedKey = "Plane_Unlocked";
        private const string SelectedKey = "Plane_Selected";

        public void UnlockPlane(string planeId)
        {
            if (string.IsNullOrEmpty(planeId) || unlockedPlanes.Contains(planeId)) return;
            unlockedPlanes.Add(planeId);
            Save();
            OnPlaneUnlocked?.Invoke(planeId);
        }

        public bool IsPlaneUnlocked(string planeId)
        {
            if (string.IsNullOrEmpty(planeId)) return false;
            return unlockedPlanes.Contains(planeId);
        }

        public void SelectPlane(string planeId)
        {
            if (string.IsNullOrEmpty(planeId) || !unlockedPlanes.Contains(planeId)) return;
            selectedPlane = planeId;
            Save();
            OnPlaneSelected?.Invoke(planeId);
        }

        public void Save()
        {
            string unlockedJson = string.Join(",", unlockedPlanes);
            PlayerPrefs.SetString(UnlockedKey, unlockedJson);
            PlayerPrefs.SetString(SelectedKey, selectedPlane);
            PlayerPrefs.Save();
        }

        public void Load()
        {
            string unlockedJson = PlayerPrefs.GetString(UnlockedKey, "");
            unlockedPlanes = string.IsNullOrEmpty(unlockedJson)
                ? new List<string>()
                : new List<string>(unlockedJson.Split(','));
            selectedPlane = PlayerPrefs.GetString(SelectedKey, "");
        }
    }
}
