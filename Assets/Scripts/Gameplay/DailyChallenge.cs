using System;
using System.Collections.Generic;
using UnityEngine;

namespace WreckWing.Gameplay
{
    [Serializable]
    public class DailyChallenge
    {
        public string title;
        public string description;
        public int targetValue;
        public int currentProgress;
        public bool isCompleted;
        public bool isClaimed;
        public int rewardAmount;
        public ChallengeType challengeType;
    }

    public enum ChallengeType { Score, Distance, Destruction, Combo, Flights }

    [Serializable]
    internal class ChallengeSaveData { public List<DailyChallenge> challenges; }
}