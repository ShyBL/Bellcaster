using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save_data.json");
    private static SaveData _currentData = new SaveData();
    private static bool _isLoaded = false;

    [System.Serializable]
    public class SaveEntry
    {
        public string key;
        public string value;
    }

    [System.Serializable]
    public class SaveData
    {
        public List<SaveEntry> entries = new List<SaveEntry>();

        public void Set(string key, string value)
        {
            var entry = entries.Find(e => e.key == key);
            if (entry != null)
            {
                entry.value = value;
            }
            else
            {
                entries.Add(new SaveEntry { key = key, value = value });
            }
        }

        public string Get(string key)
        {
            var entry = entries.Find(e => e.key == key);
            return entry?.value;
        }
    }

    /// <summary>
    /// Updates a key in memory only — no disk write. Call Flush() once
    /// after setting every key for a save event, rather than writing the
    /// entire file per key. This is what lets a caller with many values
    /// to persist at once (e.g. QuestManager saving a quest's full state)
    /// do it with a single disk write instead of one per key.
    /// </summary>
    public static void Set(string key, string value)
    {
        if (!_isLoaded) Load();
        _currentData.Set(key, value);
    }

    /// <summary>
    /// Writes the current in-memory data to disk. Call after one or more
    /// Set() calls. Safe to call even if nothing changed since the last
    /// flush — just an extra identical write, not an error.
    /// </summary>
    public static void Flush()
    {
        string json = JsonUtility.ToJson(_currentData, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[SaveSystem] Saved data to {SavePath}");
    }

    /// <summary>
    /// Sets a single key and immediately writes to disk. Unchanged
    /// behavior from before the Set/Flush split — existing callers
    /// (SavedIntVariable, SavedFloatVariable) keep working identically.
    /// For multiple keys at once, prefer Set() in a loop followed by one
    /// Flush() call.
    /// </summary>
    public static void RequestSave(string key, string value)
    {
        Set(key, value);
        Flush();
    }

    public static string RequestLoad(string key)
    {
        if (!_isLoaded) Load();
        return _currentData.Get(key);
    }

    private static void Load()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            _currentData = JsonUtility.FromJson<SaveData>(json);
            Debug.Log("[SaveSystem] Data loaded.");
        }
        else
        {
            _currentData = new SaveData();
            Debug.Log("[SaveSystem] No save file found, created new data.");
        }
        _isLoaded = true;
    }

    public static void Clear()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
        _currentData = new SaveData();
        Debug.Log("[SaveSystem] Save data cleared.");
    }
}