using System;
using UnityEngine;

namespace EdgeAbyss.Gameplay.Run
{
    /// <summary>
    /// Data container for run results.
    /// Passed to results screen after run completion.
    /// </summary>
    [Serializable]
    public class RunResults
    {
        [Header("Identification")]
        public string trackId;
        public GameMode gameMode;
        public DateTime completedAt;

        [Header("Time")]
        public float runTime;
        public float bestTime;
        public bool isNewBestTime;
        public float timeDifference;

        [Header("Score")]
        public int score;
        public int highScore;
        public bool isNewHighScore;

        [Header("Performance")]
        public float maxSpeed;
        public float averageSpeed;
        public int maxStreak;
        public float totalDistance;

        [Header("Penalties")]
        public int fallCount;
        public int respawnCount;

        [Header("Completion")]
        public bool completed;
        public RunEndReason endReason;

        /// <summary>
        /// Creates an empty results container.
        /// </summary>
        public RunResults()
        {
            completedAt = DateTime.Now;
        }

        /// <summary>
        /// Creates results for a completed run.
        /// </summary>
        public RunResults(string trackId, GameMode mode, float time, int score)
        {
            this.trackId = trackId;
            this.gameMode = mode;
            this.runTime = time;
            this.score = score;
            this.completedAt = DateTime.Now;
            this.completed = true;
            this.endReason = RunEndReason.FinishedTrack;
        }

        /// <summary>
        /// Formats run time as MM:SS.mmm
        /// </summary>
        public string FormattedTime => FormatTime(runTime);

        /// <summary>
        /// Formats best time as MM:SS.mmm
        /// </summary>
        public string FormattedBestTime => FormatTime(bestTime);

        /// <summary>
        /// Formats time difference with +/- prefix.
        /// </summary>
        public string FormattedTimeDifference
        {
            get
            {
                if (bestTime <= 0) return "--";
                string prefix = timeDifference >= 0 ? "+" : "";
                return prefix + FormatTime(Mathf.Abs(timeDifference));
            }
        }

        /// <summary>
        /// Gets a grade based on performance (S, A, B, C, D).
        /// </summary>
        public string Grade
        {
            get
            {
                if (!completed) return "F";
                if (fallCount == 0 && isNewBestTime) return "S";
                if (isNewBestTime || isNewHighScore) return "A";
                if (fallCount <= 1) return "B";
                if (fallCount <= 3) return "C";
                return "D";
            }
        }

        private static string FormatTime(float time)
        {
            if (time <= 0) return "--:--.---";

            int minutes = (int)(time / 60f);
            float seconds = time % 60f;
            return $"{minutes:00}:{seconds:00.000}";
        }
    }

    /// <summary>
    /// Game mode types.
    /// </summary>
    public enum GameMode
    {
        Story,
        TimeTrial,
        Endless,
        Practice
    }

    /// <summary>
    /// Reason for run ending.
    /// </summary>
    public enum RunEndReason
    {
        FinishedTrack,
        PlayerQuit,
        TimedOut,
        TooManyFalls,
        ExternalCancel
    }
}
