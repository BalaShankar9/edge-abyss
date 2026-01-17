using System;
using System.Collections.Generic;
using UnityEngine;

namespace EdgeAbyss.Gameplay.Ghost
{
    /// <summary>
    /// Represents a single frame of ghost data.
    /// Optimized for low memory footprint.
    /// </summary>
    [Serializable]
    public struct GhostFrame
    {
        /// <summary>Time since run start (seconds).</summary>
        public float time;

        /// <summary>Position (compressed as Vector3).</summary>
        public Vector3 position;

        /// <summary>Rotation (compressed as Quaternion).</summary>
        public Quaternion rotation;

        /// <summary>Current speed for effects.</summary>
        public float speed;

        /// <summary>Lean angle for visual fidelity.</summary>
        public float leanAngle;

        public GhostFrame(float time, Vector3 position, Quaternion rotation, float speed, float leanAngle)
        {
            this.time = time;
            this.position = position;
            this.rotation = rotation;
            this.speed = speed;
            this.leanAngle = leanAngle;
        }
    }

    /// <summary>
    /// Container for a complete ghost run recording.
    /// Supports serialization to binary format.
    /// </summary>
    [Serializable]
    public class GhostData
    {
        /// <summary>Version for forward compatibility.</summary>
        public const int CURRENT_VERSION = 1;

        /// <summary>Data format version.</summary>
        public int version = CURRENT_VERSION;

        /// <summary>Track/level identifier.</summary>
        public string trackId;

        /// <summary>Total run time (seconds).</summary>
        public float totalTime;

        /// <summary>Date/time of recording.</summary>
        public long recordedTimestamp;

        /// <summary>Player name or identifier.</summary>
        public string playerName;

        /// <summary>Recording interval in seconds.</summary>
        public float recordInterval;

        /// <summary>All recorded frames.</summary>
        public List<GhostFrame> frames = new List<GhostFrame>();

        /// <summary>Number of frames in this recording.</summary>
        public int FrameCount => frames?.Count ?? 0;

        /// <summary>
        /// Creates a new empty ghost data container.
        /// </summary>
        public GhostData(string trackId, float recordInterval = 0.05f)
        {
            this.trackId = trackId;
            this.recordInterval = recordInterval;
            this.recordedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            this.playerName = "Player";
            this.frames = new List<GhostFrame>(1200); // Pre-allocate for ~1 minute at 20Hz
        }

        /// <summary>
        /// Adds a frame to the recording.
        /// </summary>
        public void AddFrame(GhostFrame frame)
        {
            frames.Add(frame);
            totalTime = frame.time;
        }

        /// <summary>
        /// Gets the frame at the specified index.
        /// </summary>
        public GhostFrame GetFrame(int index)
        {
            if (index < 0 || index >= frames.Count)
            {
                return default;
            }
            return frames[index];
        }

        /// <summary>
        /// Finds the two frames surrounding the given time for interpolation.
        /// Returns true if valid frames found.
        /// </summary>
        public bool GetFramesForTime(float time, out GhostFrame before, out GhostFrame after, out float t)
        {
            before = default;
            after = default;
            t = 0f;

            if (frames == null || frames.Count == 0)
                return false;

            // Clamp time to valid range
            if (time <= frames[0].time)
            {
                before = frames[0];
                after = frames[0];
                t = 0f;
                return true;
            }

            if (time >= frames[^1].time)
            {
                before = frames[^1];
                after = frames[^1];
                t = 1f;
                return true;
            }

            // Binary search for efficiency
            int low = 0;
            int high = frames.Count - 1;

            while (high - low > 1)
            {
                int mid = (low + high) / 2;
                if (frames[mid].time <= time)
                    low = mid;
                else
                    high = mid;
            }

            before = frames[low];
            after = frames[high];

            float duration = after.time - before.time;
            t = duration > 0 ? (time - before.time) / duration : 0f;

            return true;
        }

        /// <summary>
        /// Clears all frames and resets the recording.
        /// </summary>
        public void Clear()
        {
            frames.Clear();
            totalTime = 0f;
        }

        /// <summary>
        /// Trims frames after the specified time (for respawn handling).
        /// </summary>
        public void TrimAfterTime(float time)
        {
            int removeIndex = frames.Count;
            for (int i = frames.Count - 1; i >= 0; i--)
            {
                if (frames[i].time <= time)
                {
                    removeIndex = i + 1;
                    break;
                }
            }

            if (removeIndex < frames.Count)
            {
                frames.RemoveRange(removeIndex, frames.Count - removeIndex);
                totalTime = frames.Count > 0 ? frames[^1].time : 0f;
            }
        }
    }
}
