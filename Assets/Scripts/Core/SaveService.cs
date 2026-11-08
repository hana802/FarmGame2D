using System;
using System.IO;
using UnityEngine;

namespace FarmGame2D.Core
{
    public sealed class SaveService
    {
        private const string FileName = "farmgame2d-save.json";
        private readonly string explicitSavePath;

        public SaveService()
        {
        }

        public SaveService(string savePath)
        {
            explicitSavePath = savePath;
        }

        public string SavePath => string.IsNullOrWhiteSpace(explicitSavePath)
            ? Path.Combine(Application.persistentDataPath, FileName)
            : explicitSavePath;

        public bool TryLoad(out GameSaveData data)
        {
            data = null;
            if (!File.Exists(SavePath))
            {
                return false;
            }

            try
            {
                return TryDeserialize(File.ReadAllText(SavePath), out data);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Save load failed: {exception.Message}");
                return false;
            }
        }

        public bool TryDeserialize(string json, out GameSaveData data)
        {
            data = null;
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                data = JsonUtility.FromJson<GameSaveData>(json);
                return data != null
                    && data.version >= 1
                    && data.version <= GameSaveData.CurrentVersion;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Save parse failed: {exception.Message}");
                return false;
            }
        }

        public bool TrySerialize(GameSaveData data, out string json)
        {
            json = string.Empty;
            if (data == null)
            {
                return false;
            }

            try
            {
                json = JsonUtility.ToJson(data, true);
                return !string.IsNullOrWhiteSpace(json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Save serialization failed: {exception.Message}");
                return false;
            }
        }

        public bool TrySave(GameSaveData data)
        {
            try
            {
                if (!TrySerialize(data, out var json))
                {
                    return false;
                }

                var directory = Path.GetDirectoryName(SavePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var temporaryPath = SavePath + ".tmp";
                File.WriteAllText(temporaryPath, json);
                File.Copy(temporaryPath, SavePath, true);
                File.Delete(temporaryPath);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Save write failed: {exception.Message}");
                return false;
            }
        }
    }
}
