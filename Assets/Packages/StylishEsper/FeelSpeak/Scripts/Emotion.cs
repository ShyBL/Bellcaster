//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using Esper.FeelSpeak.Editor;
using System.IO;
#endif
using Esper.FeelSpeak.Database;
using UnityEngine;

namespace Esper.FeelSpeak
{
    /// <summary>
    /// Emotion data.
    /// </summary>
    public class Emotion : FeelSpeakObject
    {
        /// <summary>
        /// A sprite that represents the emotion.
        /// </summary>
        public Sprite sprite;

        /// <summary>
        /// The name of the emotion. This is used for searching purposes.
        /// </summary>
        public string emotionName;

        /// <summary>
        /// The value used to multiply the normal text speed for dialogue using this emotion.
        /// </summary>
        public float textSpeedMultiplier = 1f;

        /// <summary>
        /// The text color of this emotion.
        /// </summary>
        public Color textColor = Color.white;

        /// <summary>
        /// The sound of the emotion. This is played when the dialogue that uses this emotion starts.
        /// </summary>
        public AudioClip sound;

        /// <summary>
        /// The name of the character animation to trigger. If the target character GameObject does not have an
        /// Animator component, this is irrelevant.
        /// </summary>
        public string animationName;

        /// <summary>
        /// The database record.
        /// </summary>
        public EmotionRecord DatabaseRecord
        {
            get
            {
                return new EmotionRecord
                {
                    id = id,
                    objectName = name,
                    emotionName = emotionName
                };
            }
        }

        /// <summary>
        /// The path to all generated objects of this type relative to the resources folder.
        /// </summary>
        public static string resourcesPath = "FeelSpeakResources/Emotions";

#if UNITY_EDITOR
        /// <summary>
        /// The directory of all generated objects of this type. Works in the editor only.
        /// </summary>
        public static string DirectoryPath { get => Path.Combine(AssetSearch.FolderOf<TextAsset>("FeelSpeakIdentifier"), "Resources", "FeelSpeakResources", "Emotions"); }

        /// <summary>
        /// Deletes the record of this object from the database.
        /// </summary>
        public void DeleteDatabaseRecord()
        {
            bool disconnectOnComplete = false;

            if (!FeelSpeakDatabase.IsConnected)
            {
                FeelSpeakDatabase.Initialize();
                disconnectOnComplete = true;
            }

            if (FeelSpeakDatabase.HasEmotionRecord(id))
            {
                FeelSpeakDatabase.DeleteEmotionRecord(DatabaseRecord);
            }

            if (disconnectOnComplete)
            {
                FeelSpeakDatabase.Disconnect();
            }
        }
#endif

        /// <summary>
        /// Updates the record of this object in the database.
        /// </summary>
        public void UpdateDatabaseRecord()
        {
#if UNITY_EDITOR
            bool disconnectOnComplete = false;

            if (!FeelSpeakDatabase.IsConnected)
            {
                FeelSpeakDatabase.Initialize();
                disconnectOnComplete = true;
            }
#endif

            if (FeelSpeakDatabase.HasEmotionRecord(id))
            {
                FeelSpeakDatabase.UpdateEmotionRecord(DatabaseRecord);
            }
            else
            {
                FeelSpeakDatabase.InsertEmotionRecord(DatabaseRecord);
            }

#if UNITY_EDITOR
            if (disconnectOnComplete)
            {
                FeelSpeakDatabase.Disconnect();
            }
#endif
        }

        /// <summary>
        /// Creates a copy of this emotion.
        /// </summary>
        /// <returns>The copy.</returns>
        public Emotion CreateCopy()
        {
            Emotion copy = CreateInstance<Emotion>();
            copy.id = copy.GetID<Emotion>();
            copy.CopyData(this);
            return copy;
        }

        /// <summary>
        /// Copies all data of another emotion, excluding ID.
        /// </summary>
        /// <param name="other">The emotion to copy.</param>
        public void CopyData(Emotion other)
        {
            sprite = other.sprite;
            emotionName = other.emotionName;
            animationName = other.animationName;
            textSpeedMultiplier = other.textSpeedMultiplier;
            textColor = other.textColor;
            sound = other.sound;
        }

        /// <summary>
        /// Creates a new instance of a emotion (editor only).
        /// </summary>
        /// <returns>The created instance.</returns>
        public static Emotion Create()
        {
#if UNITY_EDITOR
            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }
#endif

            var obj = CreateInstance<Emotion>();
            var id = obj.GetID<Emotion>();
            obj.id = id;
            var name = "New Emotion";
            obj.emotionName = name;

#if UNITY_EDITOR
            obj.UpdateDatabaseRecord();
            var path = Path.Combine(DirectoryPath, $"{id}_{name}.asset");
            UnityEditor.AssetDatabase.CreateAsset(obj, path);
            obj.Save();
#endif

            return obj;
        }

        /// <summary>
        /// Updates the name of the asset (editor only).
        /// </summary>
        public void UpdateAssetName()
        {
#if UNITY_EDITOR
            emotionName = SanitizeName(emotionName);

            string name = $"{id}_{emotionName}";
            UnityEditor.EditorApplication.delayCall += () =>
            {
                UnityEditor.AssetDatabase.RenameAsset(GetFullPath(this), name);

                UnityEditor.EditorApplication.delayCall += () =>
                {
                    this.name = name;
                    Save();
                };
            };
#endif
        }

        public override void Save()
        {
#if UNITY_EDITOR
            base.Save();
            UpdateDatabaseRecord();
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Gets the full path of the emotion (editor only).
        /// </summary>
        /// <param name="emotion">The emotion.</param>
        /// <returns>The full path to the emotion.</returns>
        public static string GetFullPath(Emotion emotion)
        {
            return Path.Combine(DirectoryPath, $"{emotion.name}.asset");
        }
#endif

        protected override int GetID<T>(string pathInResources = null)
        {
            return base.GetID<T>(resourcesPath);
        }
    }
}