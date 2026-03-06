//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using Esper.FeelSpeak.Database;
#if UNITY_EDITOR
using Esper.FeelSpeak.Editor;
using System.IO;
#endif
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Esper.FeelSpeak
{
    /// <summary>
    /// A character that can speak.
    /// </summary>
    public class Character : FeelSpeakObject
    {
        /// <summary>
        /// A sprite that represents the character.
        /// </summary>
        public Sprite sprite;

        /// <summary>
        /// The character name.
        /// </summary>
        public string characterName;

        /// <summary>
        /// The sound that plays for each dialogue character text.
        /// </summary>
        public AudioClip textSound;

        /// <summary>
        /// The text color of this character.
        /// </summary>
        public Color textColor = Color.white;

        /// <summary>
        /// A list of all character emotion data.
        /// </summary>
        public List<CharacterEmotion> characterEmotions = new();

        /// <summary>
        /// The database record.
        /// </summary>
        public CharacterRecord DatabaseRecord
        {
            get
            {
                return new CharacterRecord
                {
                    id = id,
                    objectName = name,
                    characterName = characterName
                };
            }
        }
        /// <summary>
        /// The path to all generated objects of this type relative to the resources folder.
        /// </summary>
        public static string resourcesPath = "FeelSpeakResources/Characters";

#if UNITY_EDITOR
        /// <summary>
        /// The directory of all generated objects of this type. Works in the editor only.
        /// </summary>
        public static string DirectoryPath { get => Path.Combine(AssetSearch.FolderOf<TextAsset>("FeelSpeakIdentifier"), "Resources", "FeelSpeakResources", "Characters"); }

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

            if (FeelSpeakDatabase.HasCharacterRecord(id))
            {
                FeelSpeakDatabase.DeleteCharacterRecord(DatabaseRecord);
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

            if (FeelSpeakDatabase.HasCharacterRecord(id))
            {
                FeelSpeakDatabase.UpdateCharacterRecord(DatabaseRecord);
            }
            else
            {
                FeelSpeakDatabase.InsertCharacterRecord(DatabaseRecord);
            }

#if UNITY_EDITOR
            if (disconnectOnComplete)
            {
                FeelSpeakDatabase.Disconnect();
            }
#endif
        }

        /// <summary>
        /// Creates a copy of this character.
        /// </summary>
        /// <returns>The copy.</returns>
        public Character CreateCopy()
        {
            Character copy = CreateInstance<Character>();
            copy.id = copy.GetID<Character>();
            copy.CopyData(this);
            return copy;
        }

        /// <summary>
        /// Copies all data of another character, excluding ID.
        /// </summary>
        /// <param name="other">The character to copy.</param>
        public void CopyData(Character other)
        {
            sprite = other.sprite;
            characterName = other.characterName;
            textSound = other.textSound;
            textColor = other.textColor;
            characterEmotions = new(other.characterEmotions);
        }

        /// <summary>
        /// Creates a new instance of a Character (editor only).
        /// </summary>
        /// <returns>The created instance.</returns>
        public static Character Create()
        {
#if UNITY_EDITOR
            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }
#endif

            var obj = CreateInstance<Character>();
            var id = obj.GetID<Character>();
            obj.id = id;
            var name = "New Character";
            obj.characterName = name;

            var emotions = FeelSpeak.GetAllEmotions().ToList();
            emotions = emotions.OrderBy(x => x.id).ToList();
            obj.RebuildCharacterEmotionsList(emotions);

#if UNITY_EDITOR
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
            characterName = SanitizeName(characterName);

            string name = $"{id}_{characterName}";
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
        /// Gets the full path of the character (editor only).
        /// </summary>
        /// <param name="character">The character.</param>
        /// <returns>The full path to the character.</returns>
        public static string GetFullPath(Character character)
        {
            return Path.Combine(DirectoryPath, $"{character.name}.asset");
        }
#endif

        protected override int GetID<T>(string pathInResources = null)
        {
            return base.GetID<T>(resourcesPath);
        }

        /// <summary>
        /// Gets the sprite of the character.
        /// </summary>
        /// <param name="emotion">The emotion.</param>
        /// <returns>The sprite of the character experiencing a specific emotion. If no emotion is passed, the neutral
        /// sprite is returned. Null is returned if the sprite was not found.</returns>
        public Sprite GetSprite(Emotion emotion = null)
        {
            if (emotion)
            {
                foreach (var item in characterEmotions)
                {
                    if (item.emotion.id == emotion.id)
                    {
                        return item.sprite;
                    }
                }
            }

            if (sprite)
            {
                return sprite;
            }

            return null;
        }

        /// <summary>
        /// Gets the emotion animation name of a specific emotion.
        /// </summary>
        /// <param name="emotion">The emotion.</param>
        /// <returns>The emotion animation name or an empty string if it doesn't exist.</returns>
        public string GetAnimationNameOverride(Emotion emotion = null)
        {
            if (emotion)
            {
                foreach (var item in characterEmotions)
                {
                    if (item.emotion.id == emotion.id)
                    {
                        return item.animationNameOverride;
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Rebuilds the character emotions list based on a given list of emotions.
        /// </summary>
        /// <param name="emotions">A list of emotions.</param>
        public void RebuildCharacterEmotionsList(List<Emotion> emotions)
        {
            if (emotions == null || emotions.Count == 0)
            {
                this.characterEmotions = new();
                return;
            }

            var characterEmotions = new List<CharacterEmotion>();

            foreach (var emotion in emotions)
            {
                characterEmotions.Add(new CharacterEmotion() { emotion = emotion });
            }

            for (int i = 0; i < characterEmotions.Count; i++)
            {
                var existing = this.characterEmotions.Get(characterEmotions[i].emotion.id);
                if (existing != null)
                {
                    characterEmotions[i] = existing.Value;
                }
            }

            this.characterEmotions = characterEmotions;
        }

        /// <summary>
        /// Data related to a character expressing an emotion.
        /// </summary>
        [System.Serializable]
        public struct CharacterEmotion
        {
            /// <summary>
            /// The emotion reference.
            /// </summary>
            public Emotion emotion;

            /// <summary>
            /// The sprite that represents the specific emotion for this character.
            /// </summary>
            public Sprite sprite;

            /// <summary>
            /// Overrides the default animation name. This will be used instead of the default animation name when this 
            /// character expresses the emotion during dialogue.
            /// </summary>
            public string animationNameOverride;
        }
    }
}