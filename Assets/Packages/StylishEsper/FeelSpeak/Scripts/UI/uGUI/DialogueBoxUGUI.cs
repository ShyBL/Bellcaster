//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using Esper.FeelSpeak.Graph;
using Esper.FeelSpeak.Graph.Data;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Esper.FeelSpeak.UI.UGUI
{
    /// <summary>
    /// Feel Speak's main dialogue box for uGUI.
    /// </summary>
    public class DialogueBoxUGUI : DialogueBox
    {
        /// <summary>
        /// The GameObject that contains all the content.
        /// </summary>
        [SerializeField]
        [Header("Dialogue UI")]
        protected GameObject content;

        /// <summary>
        /// The label that displays the dialogue text.
        /// </summary>
        [SerializeField]
        protected TextMeshProUGUI dialogueLabel;

        /// <summary>
        /// The scroll rect of the dialogue. This is used for overflow cases.
        /// </summary>
        [SerializeField]
        protected ScrollRect dialogueScroll;

        /// <summary>
        /// The total visible dialogue text characters.
        /// </summary>
        private int totalVisibleCharacters;

        public override bool IsOpen { get => content.activeSelf; }


        protected override void Awake()
        {
            base.Awake();
            dialogueLabel.text = string.Empty;
        }

        protected override void Apply(DialogueNode node)
        {
            base.Apply(node);

            switch (mode)
            {
                case DialogueMode.Instant:
                    dialogueLabel.text = fullText;
                    dialogueLabel.maxVisibleCharacters = int.MaxValue;
                    break;

                case DialogueMode.Typewriter:
                    StartTyping();
                    break;
            }
        }

        protected override void Apply(ChoiceNode node)
        {
            base.Apply(node);
        }

        public override bool FastForward()
        {
            var result = base.FastForward();

            if (result)
            {
                dialogueLabel.maxVisibleCharacters = int.MaxValue; 
            }

            AutomaticNextOnComplete();

            return result;
        }

        public override void Hide()
        {
            content.SetActive(false);
        }

        public override void Show()
        {
            content.SetActive(true);
        }

        public override void Close()
        {
            dialogueLabel.text = string.Empty;
            base.Close();
        }

        protected override void StartTyping()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            typingCoroutine = StartCoroutine(TypeText());
        }

        /// <summary>
        /// A coroutine that types out the dialogue text character by character.
        /// </summary>
        /// <returns>Yields for a small delay.</returns>
        protected IEnumerator TypeText()
        {
            float emotionMultiplier = 1f;
            var dialogue = activeDialogue;
            var dialogueNode = dialogue.currentNode as DialogueNode;
            var emotion = dialogueNode.emotion;

            if (emotion)
            {
                emotionMultiplier = emotion.textSpeedMultiplier;
            }

            // Calculate per-character delay (speed)
            var delay = 0.04f / FeelSpeak.Settings.textSpeed / emotionMultiplier;

            // Pre-process dialogue info for extra custom tags
            var dialogueInfo = new DialogueInfo(fullText);

            // Set the full text
            dialogueLabel.text = dialogueInfo.text;
            dialogueLabel.ForceMeshUpdate();

            totalVisibleCharacters = dialogueLabel.textInfo.characterCount;
            int counter = 0;

            // Get the text sound
            AudioClip sound = null;

            if (FeelSpeak.Settings.enableTextSounds)
            {
                if (activeDialogueSpeaker != null)
                {
                    sound = activeDialogueSpeaker.speaker.character.textSound;
                }

                if (!sound)
                {
                    sound = FeelSpeak.Settings.defaultTextSound;
                }
            }

            if (dialogueScroll)
            {
                dialogueScroll.verticalNormalizedPosition = 1;
            }

            float prev = 1f;
            float speedMultiplier = 1f;
            float pauseLength = 0f;
            bool instant = false;

            while (counter <= totalVisibleCharacters - 1)
            {
                // Count up
                counter++;
                int index = counter - 1;

                // Apply current pause
                pauseLength = dialogueInfo.data[index].pause;

                if (pauseLength > 0f)
                {
                    yield return new WaitForSeconds(pauseLength);
                }

                instant = dialogueInfo.data[index].instant;

                // Increase current visible characters
                dialogueLabel.maxVisibleCharacters = counter;

                // Automatically scroll down if using a scroll rect
                if (dialogueScroll)
                {
                    float curr = 1f - (dialogueLabel.textBounds.size.y / dialogueScroll.content.rect.height);

                    if (curr != prev)
                    {
                        dialogueScroll.verticalNormalizedPosition = curr;
                    }

                    prev = curr;
                }

                // Play sound
                if (!IsTextSoundDelayOn && sound)
                {
                    FeelSpeak.PlaySound(sound);

                    if (FeelSpeak.Settings.textSoundMinDelay > 0)
                    {
                        if (textSoundDelayCoroutine != null)
                        {
                            StopCoroutine(textSoundDelayCoroutine);
                        }

                        textSoundDelayCoroutine = StartCoroutine(TextSoundDelay());
                    }
                }

                // Apply current speed multiplier
                speedMultiplier = pauseLength = dialogueInfo.data[index].speed;

                if (speedMultiplier <= 0)
                {
                    speedMultiplier = 0.001f;
                }

                // Delay next character if instant not enabled
                if (!instant)
                {
                    yield return new WaitForSeconds(delay / speedMultiplier);
                }
            }

            typingCoroutine = null;

            AutomaticNextOnComplete();
        }
    }
}