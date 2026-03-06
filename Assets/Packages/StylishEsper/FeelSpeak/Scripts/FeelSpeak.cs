//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using Esper.FeelSpeak.Database;
using Esper.FeelSpeak.Graph;
using Esper.FeelSpeak.Overworld;
using Esper.FeelSpeak.Settings;
using Esper.FeelSpeak.UI;
using Esper.FeelSpeak.UI.UGUI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Esper.FeelSpeak
{
    /// <summary>
    /// Simplified access to Feel Speak's settings and database. Provides some other runtime functionality as well.
    /// </summary>
    public static class FeelSpeak
    {
        /// <summary>
        /// The path to settings relative to the Resources folder.
        /// </summary>
        private static string settingsPath = "FeelSpeakResources/Settings/FeelSpeakSettings";

        /// <summary>
        /// Feel Speak's settings.
        /// </summary>
        private static FeelSpeakSettings settings;

        /// <summary>
        /// The active player speaker. This is required for some built-in functionalities such as 
        /// interacting with NPCs.
        /// </summary>
        public static Speaker activePlayerSpeaker;

        /// <summary>
        /// The main camera. This is required for some built-in functionalities such as UI billboarding.
        /// </summary>
        public static Camera mainCamera;

        /// <summary>
        /// The audio source that will play Feel Speak's sounds that are related to FX or UI.
        /// </summary>
        public static AudioSource fxAudioSource;

        /// <summary>
        /// The default voice line audio source. This is used for all voice lines when an audio source for a specific
        /// character was not foud or the character speaking was not set.
        /// </summary>
        public static AudioSource defaultVoiceLineAudioSource;

        /// <summary>
        /// Feel Speak's settings.
        /// </summary>
        public static FeelSpeakSettings Settings
        {
            get
            {
                if (!settings)
                {
                    settings = Resources.Load<FeelSpeakSettings>(settingsPath);
                }

                return settings;
            }
        }

        /// <summary>
        /// If the dialogue box is actively displaying dialogue.
        /// </summary>
        public static bool HasActiveDialogue
        {
            get
            {
                if (!DialogueBox.Instance)
                {
                    return false;
                }

                return DialogueBox.Instance.IsDialogueRunning;
            }
        }

        /// <summary>
        /// A list of speakers currently in range of the player.
        /// </summary>
        public static List<Speaker> speakersInRange = new();

        /// <summary>
        /// A callback for when the player has interacted with a speaker. This accepts 1 argument: the speaker interacted 
        /// with (Speaker).
        /// </summary>
        public static UnityEvent<Speaker> onInteracted = new();

        /// <summary>
        /// A callback for when any dialogue graph begins. This accepts 1 argument: the dialogue graph (DialogueGraph).
        /// </summary>
        public static UnityEvent<DialogueGraph> onDialogueStarted = new();

        /// <summary>
        /// A callback for when any dialogue graph ends. This accepts 1 argument: the dialogue graph (DialogueGraph).
        /// </summary>
        public static UnityEvent<DialogueGraph> onDialogueEnded = new();

        /// <summary>
        /// If Feel Speak has been initialized.
        /// </summary>
        public static bool IsInitialized { get => FeelSpeakDatabase.IsConnected; }

        /// <summary>
        /// Initializes Feel Speak by connecting to the database. This is required to get it to work properly at runtime.
        /// </summary>
        /// <param name="activePlayerSpeaker">The active player speaker.</param>
        /// <param name="mainCamera">The main camera.</param>
        /// <param name="fxAudioSource">The audio source that will play Feel Speak's sounds that are related to FX or UI.</param>
        public static void Initialize(Speaker activePlayerSpeaker = null, Camera mainCamera = null, AudioSource fxAudioSource = null)
        {
            FeelSpeakDatabase.Initialize();

            if (activePlayerSpeaker)
            {
                FeelSpeak.activePlayerSpeaker = activePlayerSpeaker;
            }

            if (mainCamera)
            {
                FeelSpeak.mainCamera = mainCamera;
            }

            if (fxAudioSource)
            {
                FeelSpeak.fxAudioSource = fxAudioSource;
            }
        }

        /// <summary>
        /// Terminates the Feel Speak database connection. Initialize will need to be called again before working with 
        /// Feel Speak's API.
        /// </summary>
        public static void Terminate()
        {
#if UNITY_EDITOR
            if (Application.isPlaying && !UnityEditor.EditorWindow.HasOpenInstances<Editor.FeelSpeakEditorWindow>() && !UnityEditor.EditorWindow.HasOpenInstances<Editor.DialogueGraphEditorWindow>())
            {
                FeelSpeakDatabase.Disconnect();
            }
#else
            FeelSpeakDatabase.Disconnect();
#endif
            activePlayerSpeaker = null;
            mainCamera = null;
            fxAudioSource = null;
            speakersInRange.Clear();
            FreeMemory();
        }

        /// <summary>
        /// Frees up memory by unloading unused assets.
        /// </summary>
        public static void FreeMemory()
        {
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// Gets a list of all speakers in the active scene.
        /// </summary>
        /// <returns>A list of all speakers.</returns>
        public static Speaker[] FindAllSpeakers(FindObjectsInactive findObjectsInactive = FindObjectsInactive.Exclude)
        {
            var speakers = Object.FindObjectsByType<Speaker>(findObjectsInactive, FindObjectsSortMode.None);
            return speakers;
        }

        /// <summary>
        /// Finds the active dialoge speaker.
        /// </summary>
        /// <returns>The active dialogue speaker. This will return null if there isn't an active speaker.</returns>
        public static DialogueSpeaker FindActiveDialogueSpeaker()
        {
            if (!DialogueBox.Instance || DialogueBox.Instance.activeDialogueSpeaker == null)
            {
                return null;
            }

            return DialogueBox.Instance.activeDialogueSpeaker;
        }

        /// <summary>
        /// Finds a speaker in the active scene.
        /// </summary>
        /// <param name="characterId">The character ID of the speaker to find.</param>
        /// <returns>The speaker or null if one was not found.</returns>
        public static Speaker FindSpeaker(int characterId)
        {
            var speakers = FindAllSpeakers();

            foreach (Speaker speaker in speakers)
            {
                if (speaker.character.id == characterId)
                {
                    return speaker;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds a speaker in the active scene.
        /// </summary>
        /// <param name="characterName">The name of the speaker to find.</param>
        /// <returns>The speaker or null if one was not found.</returns>
        public static Speaker FindSpeaker(string characterName)
        {
            var speakers = FindAllSpeakers();

            foreach (Speaker speaker in speakers)
            {
                if (speaker.character.characterName.ToLower() == characterName.ToLower())
                {
                    return speaker;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds a speaker in the active scene.
        /// </summary>
        /// <param name="character">The character of the speaker to find.</param>
        /// <returns>The speaker or null if one was not found.</returns>
        public static Speaker FindSpeaker(Character character)
        {
            return FindSpeaker(character.id);
        }

        /// <summary>
        /// Interacts with the closest speaker that is in range of the player.
        /// </summary>
        public static void InteractWithClosestSpeaker()
        {
            if (!activePlayerSpeaker)
            {
                FeelSpeakLogger.LogWarning("Feel Speak: no active player speaker set! Set the activePlayerSpeaker otherwise " +
                    "Feel Speak won't know who the player character is.");
                return;
            }

            if (speakersInRange.Count == 0)
            {
                FeelSpeakLogger.Log("Feel Speak: no speakers in range of the player.");
                return;
            }

            Speaker closest = speakersInRange.
                OrderBy(x => Vector3.Distance(activePlayerSpeaker.transform.position, x.transform.position)).First();

            closest.Interact();
        }

        /// <summary>
        /// Triggers dialogue.
        /// </summary>
        /// <param name="dialogueGraph">The dialogue graph to use.</param>
        public static void TriggerDialogue(DialogueGraph dialogueGraph)
        {
            if (!dialogueGraph)
            {
                FeelSpeakLogger.LogWarning("Feel Speak: cannot trigger dialogue because the dialogue graph is null!");
                return;
            }

            if (!DialogueBox.Instance)
            {
                FeelSpeakLogger.LogWarning("Feel Speak: there is no dialogue box instance in the scene!");
                return;
            }

            DialogueBox.Instance.StartDialogue(dialogueGraph);
        }

        /// <summary>
        /// Plays a sound with Feel Speak's audio source.
        /// </summary>
        /// <param name="audioClip">The audio clip to play.</param>
        public static void PlaySound(AudioClip audioClip)
        {
            if (!fxAudioSource || !audioClip)
            {
                return;
            }

            fxAudioSource.PlayOneShot(audioClip);
        }

        /// <summary>
        /// Gets all dialogue graphs.
        /// </summary>
        /// <returns>A list of all dialogue graphs.</returns>
        public static DialogueGraph[] GetAllDialogueGraphs()
        {
            var dialogues = Resources.LoadAll<DialogueGraph>(DialogueGraph.resourcesPath);
            return dialogues;
        }

        /// <summary>
        /// Gets a dialogue graph by it's ID.
        /// </summary>
        /// <param name="record">The dialogue record.</param>
        /// <returns>A dialogue graph with the record ID or null if it does not exist.</returns>
        public static DialogueGraph GetDialogueGraph(DialogueRecord record)
        {
            var dialogue = Resources.Load<DialogueGraph>($"{DialogueGraph.resourcesPath}/{record.objectName}");

            if (!dialogue)
            {
                FeelSpeakLogger.LogWarning($"Feel Speak: Dialogue with the ID '{record.id}' doesn't exist.");
            }

            return dialogue;
        }

        /// <summary>
        /// Gets a dialogue graph by it's ID.
        /// </summary>
        /// <param name="id">The dialogue graph ID.</param>
        /// <returns>A dialogue graph with the ID or null if it does not exist.</returns>
        public static DialogueGraph GetDialogueGraph(int id)
        {
            var record = FeelSpeakDatabase.GetDialogueRecord(id);

            if (record == null)
            {
                FeelSpeakLogger.LogWarning($"Feel Speak: Dialogue record with the ID '{id}' doesn't exist.");
                return null;
            }

            return GetDialogueGraph(record);
        }

        /// <summary>
        /// Gets a dialogue graph by it's name.
        /// </summary>
        /// <param name="name">The name of the dialogue graph.</param>
        /// <returns>A dialogue graph with the name or null if it does not exist.</returns>
        public static DialogueGraph GetDialogueGraph(string name)
        {
            var record = FeelSpeakDatabase.GetDialogueRecord(name);

            if (record == null)
            {
                FeelSpeakLogger.LogWarning($"Feel Speak: Dialogue record with the name '{name}' doesn't exist.");
                return null;
            }

            return GetDialogueGraph(record);
        }

        /// <summary>
        /// Gets all characters.
        /// </summary>
        /// <returns>A list of all characters.</returns>
        public static Character[] GetAllCharacters()
        {
            var characters = Resources.LoadAll<Character>(Character.resourcesPath);
            return characters;
        }

        /// <summary>
        /// Gets a character by it's record.
        /// </summary>
        /// <param name="record">The character record.</param>
        /// <returns>A character with the record ID or null if it does not exist.</returns>
        public static Character GetCharacter(CharacterRecord record)
        {
            var character = Resources.Load<Character>($"{Character.resourcesPath}/{record.objectName}");

            if (!character)
            {
                FeelSpeakLogger.LogWarning($"Feel Speak: Character with the ID '{record.id}' doesn't exist.");
            }

            return character;
        }

        /// <summary>
        /// Gets a character by it's ID.
        /// </summary>
        /// <param name="id">The character ID.</param>
        /// <returns>A character with the ID or null if it does not exist.</returns>
        public static Character GetCharacter(int id)
        {
            var record = FeelSpeakDatabase.GetCharacterRecord(id);

            if (record == null)
            {
                FeelSpeakLogger.LogWarning($"Feel Speak: Character record with the ID '{id}' doesn't exist.");
                return null;
            }

            return GetCharacter(record);
        }

        /// <summary>
        /// Gets a character by it's name.
        /// </summary>
        /// <param name="name">The name of the character.</param>
        /// <returns>A character with the name or null if it does not exist.</returns>
        public static Character GetCharacter(string name)
        {
            var record = FeelSpeakDatabase.GetCharacterRecord(name);

            if (record == null)
            {
                FeelSpeakLogger.LogWarning($"Feel Speak: Character record with the name '{name}' doesn't exist.");
                return null;
            }

            return GetCharacter(name);
        }

        /// <summary>
        /// Gets all emotions.
        /// </summary>
        /// <returns>A list of all emotions.</returns>
        public static Emotion[] GetAllEmotions()
        {
            var emotions = Resources.LoadAll<Emotion>(Emotion.resourcesPath);
            return emotions;
        }

        /// <summary>
        /// Gets an emotion by it's ID.
        /// </summary>
        /// <param name="record">The record ID.</param>
        /// <returns>An emotion with the record ID or null if it does not exist.</returns>
        public static Emotion GetEmotion(EmotionRecord record)
        {
            var emotion = Resources.Load<Emotion>($"{Emotion.resourcesPath}/{record.objectName}");

            if (!emotion)
            {
                FeelSpeakLogger.LogWarning($"Feel Speak: Emotion with the ID '{record.id}' doesn't exist.");
            }

            return emotion;
        }

        /// <summary>
        /// Gets an emotion by it's ID.
        /// </summary>
        /// <param name="id">The emotion ID.</param>
        /// <returns>An emotion with the ID or null if it does not exist.</returns>
        public static Emotion GetEmotion(int id)
        {
            var record = FeelSpeakDatabase.GetEmotionRecord(id);

            if (record == null)
            {
                FeelSpeakLogger.LogWarning($"Feel Speak: Emotion record with the id '{id}' doesn't exist.");
                return null;
            }

            return GetEmotion(record);
        }

        /// <summary>
        /// Gets an emotion by it's name.
        /// </summary>
        /// <param name="name">The name of the emotion.</param>
        /// <returns>An emotion with the name or null if it does not exist.</returns>
        public static Emotion GetEmotion(string name)
        {
            var record = FeelSpeakDatabase.GetEmotionRecord(name);

            if (record == null)
            {
                FeelSpeakLogger.LogWarning($"Feel Speak: Emotion record with the name '{name}' doesn't exist.");
                return null;
            }

            return GetEmotion(record);
        }
    }
}