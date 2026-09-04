using System;
using System.Collections.Generic;
using UnityEngine;

namespace WreckWing.Gameplay
{
    public class DailyChallengeManager : MonoBehaviour
    {
        public static DailyChallengeManager Instance { get; private set; }

        [SerializeField] private int challengesPerDay = 3;
        [SerializeField] private int maxTargetValue = 10000;
        [SerializeField] private int rewardPerChallenge = 50;
        [SerializeField] private List<DailyChallenge> dailyChallenges = new List<DailyChallenge>();
        [SerializeField] private string lastResetTime = "";

        public event Action<int> OnChallengeCompleted;
        public event Action OnAllChallengesCompleted;

        private const string LAST_RESET_KEY = "DailyChallenge_LastReset";
        private const string CHALLENGES_KEY = "DailyChallenge_Data";
        private const string COUNT_KEY = "DailyChallenge_Count";

        public List<DailyChallenge> DailyChallenges => dailyChallenges;
        public string LastResetTime => lastResetTime;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();
        }

        private void Start()
        {
            if (ShouldResetChallenges()) ResetDailyChallenges();
        }

        public void GenerateDailyChallenges()
        {
            dailyChallenges.Clear();
            var rng = new System.Random(DateTime.Now.DayOfYear + DateTime.Now.Year);
            for (int i = 0; i < challengesPerDay; i++)
            {
                ChallengeType type = (ChallengeType)(i % 5);
                dailyChallenges.Add(new DailyChallenge
                {
                    challengeType = type,
                    targetValue = rng.Next(100, maxTargetValue),
                    rewardAmount = rewardPerChallenge + (i * 25),
                    title = GenerateTitle(type),
                    description = GenerateDescription(type)
                });
            }
            lastResetTime = DateTime.Now.ToString("yyyy-MM-dd");
            SaveData();
        }

        public DailyChallenge GetChallenge(int index)
        {
            if (index < 0 || index >= dailyChallenges.Count) return null;
            return dailyChallenges[index];
        }

        public void CompleteChallenge(int index)
        {
            if (index < 0 || index >= dailyChallenges.Count) return;
            var c = dailyChallenges[index];
            if (c.isCompleted) return;
            c.isCompleted = true;
            c.currentProgress = c.targetValue;
            dailyChallenges[index] = c;
            OnChallengeCompleted?.Invoke(index);
            SaveData();
            CheckAllChallengesComplete();
        }

        public void UpdateProgress(ChallengeType type, int amount)
        {
            for (int i = 0; i < dailyChallenges.Count; i++)
            {
                if (dailyChallenges[i].challengeType == type && !dailyChallenges[i].isCompleted)
                {
                    var c = dailyChallenges[i];
                    c.currentProgress = Mathf.Min(c.currentProgress + amount, c.targetValue);
                    dailyChallenges[i] = c;
                    if (c.currentProgress >= c.targetValue) CompleteChallenge(i);
                }
            }
        }

        public void ResetDailyChallenges() => GenerateDailyChallenges();

        private void CheckAllChallengesComplete()
        {
            foreach (var c in dailyChallenges)
                if (!c.isCompleted) return;
            OnAllChallengesCompleted?.Invoke();
        }

        private bool ShouldResetChallenges()
        {
            return lastResetTime != DateTime.Now.ToString("yyyy-MM-dd");
        }


        private void SaveData()
        {
            PlayerPrefs.SetString(LAST_RESET_KEY, lastResetTime);
            PlayerPrefs.SetInt(COUNT_KEY, dailyChallenges.Count);
            for (int i = 0; i < dailyChallenges.Count; i++)
            {
                var c = dailyChallenges[i];
                string prefix = CHALLENGES_KEY + "_" + i + "_";
                PlayerPrefs.SetString(prefix + "title", c.title);
                PlayerPrefs.SetInt(prefix + "target", c.targetValue);
                PlayerPrefs.SetInt(prefix + "progress", c.currentProgress);
                PlayerPrefs.SetInt(prefix + "reward", c.rewardAmount);
                PlayerPrefs.SetInt(prefix + "type", (int)c.challengeType);
                PlayerPrefs.SetInt(prefix + "completed", c.isCompleted ? 1 : 0);
                PlayerPrefs.SetInt(prefix + "claimed", c.isClaimed ? 1 : 0);
            }
            PlayerPrefs.Save();
        }

        private void LoadData()
        {
            lastResetTime = PlayerPrefs.GetString(LAST_RESET_KEY, "");
            int count = PlayerPrefs.GetInt(COUNT_KEY, 0);
            if (count <= 0) { GenerateDailyChallenges(); return; }
            dailyChallenges.Clear();
            for (int i = 0; i < count; i++)
            {
                string prefix = CHALLENGES_KEY + "_" + i + "_";
                var c = new DailyChallenge
                {
                    title = PlayerPrefs.GetString(prefix + "title", "Challenge"),
                    targetValue = PlayerPrefs.GetInt(prefix + "target", 100),
                    currentProgress = PlayerPrefs.GetInt(prefix + "progress", 0),
                    rewardAmount = PlayerPrefs.GetInt(prefix + "reward", 50),
                    challengeType = (ChallengeType)PlayerPrefs.GetInt(prefix + "type", 0),
                    isCompleted = PlayerPrefs.GetInt(prefix + "completed", 0) == 1,
                    isClaimed = PlayerPrefs.GetInt(prefix + "claimed", 0) == 1
                };
                dailyChallenges.Add(c);
            }
        }

        private string GenerateTitle(ChallengeType type)
        {
            switch (type)
            {
                case ChallengeType.Score: return "High Scorer";
                case ChallengeType.Distance: return "Long Flight";
                case ChallengeType.Destruction: return "Wrecker";
                case ChallengeType.Combo: return "Combo Master";
                default: return "Frequent Flyer";
            }
        }

        private string GenerateDescription(ChallengeType type)
        {
            switch (type)
            {
                case ChallengeType.Score: return "Earn a total of {0} points today";
                case ChallengeType.Distance: return "Fly {0} total distance";
                case ChallengeType.Destruction: return "Destroy {0} points of structure";
                case ChallengeType.Combo: return "Reach a {0} combo streak";
                default: return "Complete {0} flights today";
            }
        }
    }
}
