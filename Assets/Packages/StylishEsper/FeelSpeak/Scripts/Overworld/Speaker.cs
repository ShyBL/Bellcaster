//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using Esper.FeelSpeak.Graph;
using System;
using UnityEngine;
using Esper.FeelSpeak.UI.UGUI;
using UnityEngine.Events;

namespace Esper.FeelSpeak.Overworld
{
    /// <summary>
    /// Represents a character in the game world that can speak.
    /// </summary>
    public class Speaker : MonoBehaviour
    {
        /// <summary>
        /// The character.
        /// </summary>
        public Character character;

        /// <summary>
        /// The dialogue that is triggered when interacting with this speaker.
        /// </summary>
        public DialogueGraph dialogue;

        /// <summary>
        /// The audio source for the character's voice.
        /// </summary>
        public AudioSource voiceAudioSource;

        /// <summary>
        /// An object that is enabled when the player is in range of the speaker.
        /// </summary>
        public GameObject inRangeIndicator;

        /// <summary>
        /// The animator component.
        /// </summary>
        protected Animator animator;

        /// <summary>
        /// The action to invoke when Interact() is called. 
        /// </summary>
        protected Action interactionAction;

        /// <summary>
        /// A callback for when the player has interacted with this speaker.
        /// </summary>
        public UnityEvent onInteracted = new();

        protected virtual void Awake()
        {
            animator = GetComponent<Animator>();

            if (!animator)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (inRangeIndicator)
            {
                inRangeIndicator.SetActive(false);
            }

            interactionAction = TriggerDialogue;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform == FeelSpeak.activePlayerSpeaker.transform)
            {
                FeelSpeak.speakersInRange.Add(this);

                if (inRangeIndicator)
                {
                    inRangeIndicator.SetActive(true);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.transform == FeelSpeak.activePlayerSpeaker.transform)
            {
                FeelSpeak.speakersInRange.Add(this);

                if (inRangeIndicator)
                {
                    inRangeIndicator.SetActive(true);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.transform == FeelSpeak.activePlayerSpeaker.transform)
            {
                FeelSpeak.speakersInRange.Remove(this);

                if (inRangeIndicator)
                {
                    inRangeIndicator.SetActive(false);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.transform == FeelSpeak.activePlayerSpeaker.transform)
            {
                FeelSpeak.speakersInRange.Remove(this);

                if (inRangeIndicator)
                {
                    inRangeIndicator.SetActive(false);
                }
            }
        }

        private void OnDestroy()
        {
            if (FeelSpeak.speakersInRange.Contains(this))
            {
                FeelSpeak.speakersInRange.Remove(this);
            }
        }

        /// <summary>
        /// Plays an animation.
        /// </summary>
        /// <param name="animationName">The name of the animation.</param>
        /// <param name="layer">The layer index.</param>
        public void PlayAnimation(string animationName, int layer = -1)
        {
            if (!animator)
            {
                return;
            }

            animator.Play(animationName, layer, 0);
        }

        /// <summary>
        /// Stops the currently playing voice line.
        /// </summary>
        public void StopVoiceLine()
        {
            if (voiceAudioSource && voiceAudioSource.isPlaying)
            {
                voiceAudioSource.Stop();
            }
        }

        /// <summary>
        /// Plays a voice line audio clip.
        /// </summary>
        /// <param name="audioClip">The audio clip.</param>
        public void PlayVoiceLine(AudioClip audioClip)
        {
            if (!audioClip)
            {
                return;
            }

            if (!voiceAudioSource)
            {
                FeelSpeakLogger.LogWarning($"Feel Speak: cannot play voice line for {name} because it has no voice audio source set.");
                return;
            }

            StopVoiceLine();
            voiceAudioSource.clip = audioClip;
            voiceAudioSource.Play();
        }

        /// <summary>
        /// Starts dialogue with this speaker.
        /// </summary>
        public void TriggerDialogue()
        {
            if (!DialogueBox.Instance)
            {
                FeelSpeakLogger.LogWarning("Feel Speak: there is no dialogue box instance in the scene!");
                return;
            }

            DialogueBox.Instance.StartDialogue(this);
        }

        /// <summary>
        /// Interacts with the speaker (invokes interaction action).
        /// </summary>
        public void Interact()
        {
            if (interactionAction != null)
            {
                interactionAction.Invoke();
                onInteracted.Invoke();
                FeelSpeak.onInteracted.Invoke(this);
            }
        }
    }
}