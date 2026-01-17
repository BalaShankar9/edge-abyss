using System;
using System.IO;
using UnityEngine;

namespace EdgeAbyss.Gameplay.Ghost
{
    /// <summary>
    /// Handles saving and loading ghost data to persistent storage.
    /// Uses binary format for efficiency.
    /// </summary>
    public static class GhostSerializer
    {
        private const string GHOST_FOLDER = "Ghosts";
        private const string FILE_EXTENSION = ".ghost";
        private const uint MAGIC_NUMBER = 0x47485354; // "GHST"

        /// <summary>
        /// Gets the full path for a ghost file.
        /// </summary>
        public static string GetGhostPath(string trackId)
        {
            string folder = Path.Combine(Application.persistentDataPath, GHOST_FOLDER);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return Path.Combine(folder, $"{trackId}_best{FILE_EXTENSION}");
        }

        /// <summary>
        /// Saves ghost data to disk.
        /// </summary>
        public static bool Save(GhostData data, string trackId)
        {
            if (data == null || data.FrameCount == 0)
            {
                Debug.LogWarning("[GhostSerializer] Cannot save empty ghost data.");
                return false;
            }

            try
            {
                string path = GetGhostPath(trackId);

                using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
                using (var writer = new BinaryWriter(stream))
                {
                    // Header
                    writer.Write(MAGIC_NUMBER);
                    writer.Write(data.version);
                    writer.Write(data.trackId ?? "");
                    writer.Write(data.totalTime);
                    writer.Write(data.recordedTimestamp);
                    writer.Write(data.playerName ?? "Player");
                    writer.Write(data.recordInterval);

                    // Frame count
                    writer.Write(data.FrameCount);

                    // Frames (with delta compression)
                    Vector3 lastPos = Vector3.zero;
                    Quaternion lastRot = Quaternion.identity;

                    for (int i = 0; i < data.FrameCount; i++)
                    {
                        var frame = data.GetFrame(i);

                        // Time (always absolute for seeking)
                        writer.Write(frame.time);

                        // Position delta
                        Vector3 posDelta = frame.position - lastPos;
                        WriteVector3Compressed(writer, posDelta);
                        lastPos = frame.position;

                        // Rotation (compressed to smallest-three representation)
                        WriteQuaternionCompressed(writer, frame.rotation);
                        lastRot = frame.rotation;

                        // Speed and lean
                        writer.Write(frame.speed);
                        writer.Write(frame.leanAngle);
                    }
                }

                Debug.Log($"[GhostSerializer] Saved ghost ({data.FrameCount} frames) to {path}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[GhostSerializer] Failed to save ghost: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads ghost data from disk.
        /// </summary>
        public static GhostData Load(string trackId)
        {
            string path = GetGhostPath(trackId);

            if (!File.Exists(path))
            {
                Debug.Log($"[GhostSerializer] No ghost file found for track '{trackId}'.");
                return null;
            }

            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                using (var reader = new BinaryReader(stream))
                {
                    // Verify magic number
                    uint magic = reader.ReadUInt32();
                    if (magic != MAGIC_NUMBER)
                    {
                        Debug.LogError("[GhostSerializer] Invalid ghost file format.");
                        return null;
                    }

                    // Read header
                    int version = reader.ReadInt32();
                    string loadedTrackId = reader.ReadString();
                    float totalTime = reader.ReadSingle();
                    long timestamp = reader.ReadInt64();
                    string playerName = reader.ReadString();
                    float recordInterval = reader.ReadSingle();

                    var data = new GhostData(loadedTrackId, recordInterval)
                    {
                        version = version,
                        totalTime = totalTime,
                        recordedTimestamp = timestamp,
                        playerName = playerName
                    };

                    // Read frame count
                    int frameCount = reader.ReadInt32();
                    data.frames.Capacity = frameCount;

                    // Read frames (decompress)
                    Vector3 lastPos = Vector3.zero;

                    for (int i = 0; i < frameCount; i++)
                    {
                        float time = reader.ReadSingle();

                        // Position delta
                        Vector3 posDelta = ReadVector3Compressed(reader);
                        Vector3 position = lastPos + posDelta;
                        lastPos = position;

                        // Rotation
                        Quaternion rotation = ReadQuaternionCompressed(reader);

                        // Speed and lean
                        float speed = reader.ReadSingle();
                        float lean = reader.ReadSingle();

                        data.frames.Add(new GhostFrame(time, position, rotation, speed, lean));
                    }

                    Debug.Log($"[GhostSerializer] Loaded ghost ({frameCount} frames) from {path}");
                    return data;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[GhostSerializer] Failed to load ghost: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Checks if a ghost exists for the given track.
        /// </summary>
        public static bool GhostExists(string trackId)
        {
            return File.Exists(GetGhostPath(trackId));
        }

        /// <summary>
        /// Deletes the ghost for the given track.
        /// </summary>
        public static bool DeleteGhost(string trackId)
        {
            string path = GetGhostPath(trackId);
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }
            return false;
        }

        private static void WriteVector3Compressed(BinaryWriter writer, Vector3 v)
        {
            // Use float for simplicity (can use half-precision in future with custom encoding)
            writer.Write(v.x);
            writer.Write(v.y);
            writer.Write(v.z);
        }

        private static Vector3 ReadVector3Compressed(BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            return new Vector3(x, y, z);
        }

        private static void WriteQuaternionCompressed(BinaryWriter writer, Quaternion q)
        {
            // Smallest-three compression: drop largest component, store 3 + index
            q = q.normalized;

            float absX = Mathf.Abs(q.x);
            float absY = Mathf.Abs(q.y);
            float absZ = Mathf.Abs(q.z);
            float absW = Mathf.Abs(q.w);

            int largestIndex = 0;
            float largestValue = absX;

            if (absY > largestValue) { largestIndex = 1; largestValue = absY; }
            if (absZ > largestValue) { largestIndex = 2; largestValue = absZ; }
            if (absW > largestValue) { largestIndex = 3; }

            // Get sign of largest to ensure positive reconstruction
            float sign = largestIndex switch
            {
                0 => Mathf.Sign(q.x),
                1 => Mathf.Sign(q.y),
                2 => Mathf.Sign(q.z),
                _ => Mathf.Sign(q.w)
            };

            // Write index
            writer.Write((byte)largestIndex);

            // Write other three components (scaled by sign)
            float a, b, c;
            switch (largestIndex)
            {
                case 0: a = q.y * sign; b = q.z * sign; c = q.w * sign; break;
                case 1: a = q.x * sign; b = q.z * sign; c = q.w * sign; break;
                case 2: a = q.x * sign; b = q.y * sign; c = q.w * sign; break;
                default: a = q.x * sign; b = q.y * sign; c = q.z * sign; break;
            }

            writer.Write(a);
            writer.Write(b);
            writer.Write(c);
        }

        private static Quaternion ReadQuaternionCompressed(BinaryReader reader)
        {
            int largestIndex = reader.ReadByte();
            float a = reader.ReadSingle();
            float b = reader.ReadSingle();
            float c = reader.ReadSingle();

            // Reconstruct largest component
            float largest = Mathf.Sqrt(1f - (a * a + b * b + c * c));

            return largestIndex switch
            {
                0 => new Quaternion(largest, a, b, c),
                1 => new Quaternion(a, largest, b, c),
                2 => new Quaternion(a, b, largest, c),
                _ => new Quaternion(a, b, c, largest)
            };
        }
    }
}
