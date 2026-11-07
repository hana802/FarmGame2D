using System;
using System.IO;
using UnityEngine;

namespace FarmGame2D.Core
{
    public sealed class SaveService
    {
        private const string FileName = "farmgame2d-save.json";

        public string SavePath => Path.Combine(Application.persistentDataPath, FileName);

        public bool TryLoad(out GameSaveData data)
        {
            data = null;
            if (!File.Exists(SavePath))
            {
                return false;
            }

            try
            {
                data = JsonUtility.FromJson<GameSaveData>(File.ReadAllText(SavePath));
                return data != null && data.version == 1;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Save load failed: {exception.Message}");
                return false;
            }
        }

        public bool TrySave(GameSaveData data)
        {
            try
            {
                var directory = Path.GetDirectoryName(SavePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var temporaryPath = SavePath + ".tmp";
                File.WriteAllText(temporaryPath, JsonUtility.ToJson(data, true));
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
