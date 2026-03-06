//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using Esper.FeelSpeak.Graph;
using Esper.FeelSpeak.Graph.Data;
using Esper.FeelSpeak.Overworld;
using Esper.FeelSpeak.Overworld.Data;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Esper.FeelSpeak.UI.UGUI
{
    /// <summary>
    /// Feel Speak's main dialogue box base class.
    /// </summary>
    public abstract class DialogueBox : MonoBehaviour
    {
        /// <summary>
        /// The dialogue mode.
        /// </summary>
        public DialogueMode mode = DialogueMode.Typewriter;

        /// <summary>
        /// If this object should not be destroyed when the scene is unloaded.
        /// </summary>
        public bool dontDestroyOnLoad = true;

        [Header("Speaker UI")]
        /// <summary>
        /// The speaker being talked to.
        /// </summary>
        protected Speaker targetSpeaker;

        /// <summary>
        /// The current dialogue speaker.
        /// </summary>
        public DialogueSpeaker dialogueSpeaker;

        /// <summary>
        /// The current active dialogue speaker.
        /// </summary>
        [HideInInspector]
        public DialogueSpeaker activeDialogueSpeaker;

        /// <summary>
        /// The last speaker that was replaced.
        /// </summary>
        protected DialogueSpeaker lastReplaced;

        /// <summary>
        /// The last active dialogue graph.
        /// </summary>
        protected DialogueGraph lastGraph;

        /// <summary>
        /// The active dialogue.
        /// </summary>
        protected Dialogue activeDialogue;

        /// <summary>
        /// A callback for when the dialogue box is opened.
        /// </summary>
        [Header("Events"), Space(4)]
        public UnityEvent onOpen = new();

        /// <summary>
        /// A callback for when the dialogue box is closed.
        /// </summary>
        [Space(4)]
        public UnityEvent onClose = new();

        /// <summary>
        /// The active typing coroutine.
        /// </summary>
        protected Coroutine typingCoroutine;

        /// <summary>
        /// The active text sound delay coroutine.
        /// </summary>
        protected Coroutine textSoundDelayCoroutine;

        /// <summary>
        /// The active delay coroutine.
        /// </summary>
        protected Coroutine delayCoroutine;

        /// <summary>
        /// The current full text that is displayed or is being typed out.
        /// </summary>
        protected string fullText;

        /// <summary>
        /// The active dialogue info.
        /// </summary>
        protected DialogueInfo dialogueInfo;

        /// <summary>
        /// If dialogue continuation is currently delayed due to a delay node.
        /// </summary>
        protected bool IsDelayOn { get => delayCoroutine != null; }

        /// <summary>
        /// If the text sound delay is currently enabled.
        /// </summary>
        protected bool IsTextSoundDelayOn { get => textSoundDelayCoroutine != null; }

        /// <summary>
        /// If the primary speaker is currently active.
        /// </summary>
        public bool IsPrimaryActive { get => dialogueSpeaker == activeDialogueSpeaker; }

        /// <summary>
        /// If the text is currently being typed out.
        /// </summary>
        public bool IsTypingOut { get => typingCoroutine != null; }

        /// <summary>
        /// If the dialogue box is currently open.
        /// </summary>
        public abstract bool IsOpen { get; }

        /// <summary>
        /// If the dialogue box is actively displaying dialogue.
        /// </summary>
        public bool IsDialogueRunning { get => (IsOpen || IsDelayOn) && activeDialogue != null; }

        /// <summary>
        /// The active instance.
        /// </summary>
        public static DialogueBox Instance { get; protected set; }

        protected virtual void Awake()
        {
            if (dontDestroyOnLoad)
            {
                if (Instance)
                {
                    Destroy(gameObject);
                    return;
                }
                else
                {
                    DontDestroyOnLoad(gameObject);
                }
            }

            Instance = this;
            Hide();
        }

        /// <summary>
        /// Gets a present speaker by name. A present speaker is a speaker that is either the current primary or secondary
        /// speaker.
        /// </summary>
        /// <param name="characterName">The name of the character.</param>
        /// <returns>The present speaker (DialogueSpeaker) or null if there is no present speaker with the specific name.</returns>
        public DialogueSpeaker GetPresentSpeaker(string characterName)
        {
            if (dialogueSpeaker && dialogueSpeaker.speaker && dialogueSpeaker.speaker.character.characterName.ToLower() == characterName.ToLower())
            {
                return dialogueSpeaker;
            }

            return null;
        }

        /// <summary>
        /// Opens the dialogue box and starts dialogue.
        /// </summary>
        /// <param name="speaker">The target speaker.</param>
        public virtual void StartDialogue(Speaker speaker)
        {
            if (IsDialogueRunning)
            {
                return;
            }

            if (!speaker)
            {
                FeelSpeakLogger.LogWarning("Feel Speak: cannot start dialogue with null speaker.");
                return;
            }

            targetSpeaker = speaker;

            if (PrepareDialogue(speaker.dialogue))
            {
                StartPreparedDialogue();
            }
        }

        /// <summary>
        /// Opens the dialogue box and starts dialogue.
        /// </summary>
        /// <param name="dialogueGraph">The dialogue graph.</param>
        public virtual void StartDialogue(DialogueGraph dialogueGraph)
        {
            if (IsDialogueRunning)
            {
                return;
            }

            if (PrepareDialogue(dialogueGraph))
            {
                StartPreparedDialogue();
            }
        }

        /// <summary>
        /// Opens the dialogue box and starts dialogue. Use this after manually preparing dialogue.
        /// </summary>
        public virtual void StartDialogue()
        {
            if (IsDialogueRunning)
            {
                return;
            }

            StartPreparedDialogue();
        }

        /// <summary>
        /// Opens the dialogue box and starts dialogue.
        /// </summary>
        protected virtual void StartPreparedDialogue()
        {
            if (dialogueSpeaker)
            {
                dialogueSpeaker.SetData(null);
            }

            Open();
            Next(null);

            if (activeDialogue != null)
            {
                activeDialogue.dialogueGraph.callOnceOnDialogueStarted.Invoke();
                activeDialogue.dialogueGraph.callOnceOnDialogueStarted.RemoveAllListeners();
                FeelSpeak.onDialogueStarted.Invoke(activeDialogue.dialogueGraph);
            }
        }

        /// <summary>
        /// Ends the dialogue and closes the dialogue box.
        /// </summary>
        /// <returns>True if dialogue was successfully ended. Otherwise, false.</returns>
        public virtual bool EndDialogue()
        {
            if (delayCoroutine != null)
            {
                StopCoroutine(delayCoroutine);
            }

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            if (textSoundDelayCoroutine != null)
            {
                StopCoroutine(textSoundDelayCoroutine);
            }

            targetSpeaker = null;
            delayCoroutine = null;
            typingCoroutine = null;
            textSoundDelayCoroutine = null;
            activeDialogue = null;
            activeDialogueSpeaker = null;

            Close();

            if (lastGraph)
            {
                lastGraph.callOnceOnDialogueEnded.Invoke();
                lastGraph.callOnceOnDialogueEnded.RemoveAllListeners();
                FeelSpeak.onDialogueEnded.Invoke(lastGraph);
            }

            lastGraph = null;

            return true;
        }

        /// <summary>
        /// Move on to the next dialogue by invoking a dialogue option.
        /// </summary>
        /// <param name="dialogueOption">The dialogue option.</param>
        public virtual void SelectDialogue(DialogueOption dialogueOption)
        {
            dialogueOption.onSelected?.Invoke();
        }

        /// <summary>
        /// Move on to the next dialogue (or any other node). This may end dialogue and close the dalogue box if there is no 
        /// next node.
        /// </summary>
        /// <param name="choiceIndex">The choice index. This is only relevant if the current node is a choice node.</param>
        /// <param name="withoutFastForward">If fast forwarding should be ignored.</param>
        public virtual void Next(int? choiceIndex, bool withoutFastForward = false)
        {
            var dialogue = activeDialogue;

            if (IsDelayOn || dialogue == null)
            {
                return;
            }

            lastGraph = dialogue.dialogueGraph;

            if (!IsOpen)
            {
                Show();
            }

            if (IsTypingOut && !choiceIndex.HasValue && !withoutFastForward)
            {
                FastForward();
                return;
            }

            StopActiveVoiceLine();

            var next = dialogue.Next(choiceIndex);

            if (next is DialogueNode dialogueNode)
            {
                Apply(dialogueNode);

                var n = dialogue.ReadNext(null);

                if (n is ChoiceNode)
                {
                    Next(null, true);
                }
            }
            else if (next is ChoiceNode choiceNode)
            {
                Apply(choiceNode);
            }
            else if (next is DelayNode delayNode)
            {
                Apply(delayNode);
            }
            else if (next == null)
            {
                EndDialogue();
            }
        }

        /// <summary>
        /// Move on to the next dialogue (or any other node). This may end dialogue and close the dalogue box if there is no 
        /// next node. If the current node is a choice node, nothing will happen.
        /// </summary>
        public virtual void Next()
        {
            if (IsTypingOut)
            {
                FastForward();
                return;
            }

            if (activeDialogue != null && activeDialogue.currentNode is ChoiceNode)
            {
                return;
            }

            CancelInvoke(nameof(Next));
            Next(null);
        }

        /// <summary>
        /// Applies the properties of a node.
        /// </summary>
        /// <param name="node">The node.</param>
        protected virtual void Apply(DialogueNode node)
        {
            if (!node.character)
            {
                activeDialogueSpeaker = null;

                if (dialogueSpeaker)
                {
                    dialogueSpeaker.SetInactive();
                }
            }
            else
            {
                if (dialogueSpeaker)
                {
                    activeDialogueSpeaker = dialogueSpeaker;
                    dialogueSpeaker.SetData(node);
                }
            }

            fullText = node.GetInterpolatedDialogue();
            var emotion = node.emotion;

            if (emotion)
            {
                var sound = emotion.sound;

                if (activeDialogueSpeaker != null)
                {
                    if (!node.suppressEmotionAnimation)
                    {
                        var animName = emotion.animationName;
                        var overrideAnimName = activeDialogueSpeaker.speaker.character.GetAnimationNameOverride(emotion);

                        if (!string.IsNullOrEmpty(overrideAnimName))
                        {
                            animName = overrideAnimName;
                        }

                        activeDialogueSpeaker.speaker.PlayAnimation(animName);
                    }
                }

                if (!node.suppressEmotionSound)
                {
                    FeelSpeak.PlaySound(sound);
                }
            }

            if (FeelSpeak.Settings.enableVoiceLines)
            {
                var defaultSource = FeelSpeak.defaultVoiceLineAudioSource;
                var voiceLine = node.voiceLine;

                if (voiceLine)
                {
                    if (activeDialogueSpeaker != null)
                    {
                        activeDialogueSpeaker.speaker.PlayVoiceLine(voiceLine);
                    }
                    else if (defaultSource)
                    {
                        if (defaultSource.isPlaying)
                        {
                            defaultSource.Stop();
                        }

                        defaultSource.clip = voiceLine;
                        defaultSource.Play();
                    }
                }
            }
        }

        /// <summary>
        /// Applies the properties of a node.
        /// </summary>
        /// <param name="node">The node.</param>
        protected virtual void Apply(ChoiceNode node)
        {
            if (!ChoicesList.Instance)
            {
                Next(0, true);
                FeelSpeakLogger.LogWarning("Feel Speak: the first choice of a choice node was automatically selected because there is no active choices list in the scene!");
                return;
            }

            if (node.useTimeout)
            {
                if (!ChoiceTimer.Instance)
                {
                    FeelSpeakLogger.LogWarning("Feel Speak: unable to start the choice timer for a choice node because there is no ChoiceTimer instance in the scene.");
                }

                ChoiceTimer.Instance.StartTimer(node);
            }

            ChoicesList.Instance.Open(node);
        }

        /// <summary>
        /// Applies the properties of a node.
        /// </summary>
        /// <param name="node">The node.</param>
        protected virtual void Apply(DelayNode node)
        {
            if (node.hideDialogueBox)
            {
                Hide();
            }

            if (delayCoroutine != null)
            {
                StopCoroutine(delayCoroutine);
            }

            delayCoroutine = StartCoroutine(Delay(node.delay));
        }

        /// <summary>
        /// Fast forwards the dialogue if possible.
        /// </summary>
        /// <returns>True if successfully fast forwarded. Otherwise, false.</returns>
        public virtual bool FastForward()
        {
            if (FeelSpeak.Settings.enableFastForward && typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Triggers Next if the current dialogue node's automaticNextOnComplete value is true.
        /// </summary>
        protected virtual void AutomaticNextOnComplete()
        {
            if (activeDialogue != null && activeDialogue.currentNode is DialogueNode node)
            {
                if (node.automaticNextOnComplete)
                {
                    Invoke(nameof(Next), node.automaticNextDelay);
                }
            }
        }

        /// <summary>
        /// Stops the voice line currently playing.
        /// </summary>
        public virtual void StopActiveVoiceLine()
        {
            if (FeelSpeak.Settings.enableVoiceLines)
            {
                var defaultSource = FeelSpeak.defaultVoiceLineAudioSource;

                if (defaultSource && defaultSource.isPlaying)
                {
                    defaultSource.Stop();
                }
                else if (activeDialogueSpeaker != null)
                {
                    activeDialogueSpeaker.speaker.StopVoiceLine();
                }
            }
        }

        /// <summary>
        /// Hides the dialogue box.
        /// </summary>
        public abstract void Hide();

        /// <summary>
        /// Shows the dialogue box.
        /// </summary>
        public abstract void Show();

        /// <summary>
        /// Opens the dialogue box.
        /// </summary>
        public virtual void Open()
        {
            Show();
            onOpen.Invoke();
        }

        /// <summary>
        /// Closes the dialogue box.
        /// </summary>
        public virtual void Close()
        {
            if (dialogueSpeaker)
            {
                dialogueSpeaker.SetData(null);
            }

            Hide();
            onClose.Invoke();
        }

        /// <summary>
        /// Starts typing out the text.
        /// </summary>
        protected abstract void StartTyping();

        /// <summary>
        /// Sets the primary speaker as active.
        /// </summary>
        /// <param name="activeColor">The color of the active sprite.</param>
        /// <param name="inactiveColor">The color of the inactive sprite.</param>
        public virtual void SetPrimaryActive(Color? activeColor = null, Color? inactiveColor = null)
        {
            if (dialogueSpeaker)
            {
                activeDialogueSpeaker = dialogueSpeaker;
                dialogueSpeaker.SetActive(activeColor);
            }

            UpdateSpeakerElements();
        }

        /// <summary>
        /// Updates the speaker UI elements.
        /// </summary>
        public virtual void UpdateSpeakerElements()
        {
            if (dialogueSpeaker)
            {
                dialogueSpeaker.UpdateElements();
            }
        }

        /// <summary>
        /// Prepares dialogue synchronously.
        /// </summary>
        /// <param name="dialogueGraph">The dialogue graph.</param>
        public bool PrepareDialogue(DialogueGraph dialogueGraph)
        {
            activeDialogue = new Dialogue(dialogueGraph);
            return activeDialogue.Prepare();
        }

        /// <summary>
        /// Prepares dialogue asynchronously.
        /// </summary>
        /// <param name="dialogueGraph">The dialogue graph.</param>
        /// <returns>The task.</returns>
        public async Task<bool> PrepareDialogueAsync(DialogueGraph dialogueGraph)
        {
            activeDialogue = new Dialogue(dialogueGraph);
            return await activeDialogue.PrepareAsync();
        }

        /// <summary>
        /// Delays text sounds.
        /// </summary>
        /// <returns>Yields for the min text sound delay length.</returns>
        protected IEnumerator TextSoundDelay()
        {
            yield return new WaitForSeconds(FeelSpeak.Settings.textSoundMinDelay);
            textSoundDelayCoroutine = null;
        }

        /// <summary>
        /// Executes a delay.
        /// </summary>
        /// <param name="length">The delay length.</param>
        /// <returns>Yields for the delay length.</returns>
        protected IEnumerator Delay(float length)
        {
            yield return new WaitForSeconds(length);
            delayCoroutine = null;
            Next();
        }

        /// <summary>
        /// Supported dialogue modes.
        /// </summary>
        public enum DialogueMode
        {
            /// <summary>
            /// Instantly show text. Text sounds will not be used.
            /// </summary>
            Instant,

            /// <summary>
            /// Type out each character one by one. Text sounds are used for this effect.
            /// </summary>
            Typewriter
        }
    }
}