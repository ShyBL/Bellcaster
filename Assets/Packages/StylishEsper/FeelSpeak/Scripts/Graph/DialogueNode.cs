//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using UnityEngine;

namespace Esper.FeelSpeak.Graph
{
    /// <summary>
    /// A node that contains dialogue data.
    /// </summary>
    [System.Serializable]
    public class DialogueNode : Node
    {
        /// <summary>
        /// The character saying the dialogue.
        /// </summary>
        public Character character;

        /// <summary>
        /// The emotion the character is feeling. This applies the emotion settings to adjust the behaviour of how
        /// dialogue is displayed. Leave this empty to use the default dialogue settings.
        /// </summary>
        public Emotion emotion;

        /// <summary>
        /// If enabled, the emotion animation of the speaker will not be invoked.
        /// </summary>
        public bool suppressEmotionAnimation;

        /// <summary>
        /// If enabled, the emotion sound will not play for this dialogue.
        /// </summary>
        public bool suppressEmotionSound;

        /// <summary>
        /// If the next node should be triggered right away when this dialogue is complete.
        /// </summary>
        public bool automaticNextOnComplete;

        /// <summary>
        /// The length of time automatic next on complete is delayed.
        /// </summary>
        public float automaticNextDelay;

        /// <summary>
        /// The dialogue text.
        /// </summary>
        public string dialogue;

        /// <summary>
        /// The voice line that represents the dialogue.
        /// </summary>
        public AudioClip voiceLine;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id">The node ID.</param>
        /// <param name="position">The position in the graph.</param>
        public DialogueNode(int id, Vector2 position) : base(id, position)
        {
            automaticNextOnComplete = FeelSpeak.Settings.defaultAutomaticNextOnComplete;
            automaticNextDelay = FeelSpeak.Settings.defaultAutomaticNextDelay;
        }

        public override bool IsComplete()
        {
            if (!FeelSpeak.Settings)
            {
                return true;
            }

            bool dialoguesComplete = true;
            bool voiceLinesComplete = true;

            dialoguesComplete = !string.IsNullOrEmpty(dialogue);

            if (FeelSpeak.Settings.enableVoiceLines)
            {
                voiceLinesComplete = voiceLine;
            }

            return dialoguesComplete && voiceLinesComplete;
        }

        /// <summary>
        /// Gets the dialogue text. This includes object tag interpolation. "???" is used if a character or emotion that is 
        /// attempted to be referenced doesn't exist.
        /// </summary>
        /// <returns>The interpolated dialogue.</returns>
        public string GetInterpolatedDialogue()
        {
            return FeelSpeakUtility.GetInterpolatedText(dialogue, character, emotion);
        }

        /// <summary>
        /// Creates a copy of this node.
        /// </summary>
        /// <returns>The copy.</returns>
        public DialogueNode CreateCopy()
        {
            var copy = new DialogueNode(id, position);
            copy.graphId = graphId;
            copy.character = character;
            copy.emotion = emotion;
            copy.suppressEmotionAnimation = suppressEmotionAnimation;
            copy.automaticNextOnComplete = automaticNextOnComplete;
            copy.automaticNextDelay = automaticNextDelay;
            copy.dialogue = dialogue;
            copy.voiceLine = voiceLine;
            return copy;
        }

        public override void InvokeEvent()
        {
            Dialogue.onDialogueNodeTriggered.Invoke(this);
        }
    }
}