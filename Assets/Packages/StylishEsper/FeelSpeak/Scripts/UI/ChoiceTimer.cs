//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using Esper.FeelSpeak.Graph;
using Esper.FeelSpeak.UI.UGUI;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Esper.FeelSpeak.UI
{
    /// <summary>
    /// Displays a visual timer that represents the remaining time to make a choice.
    /// </summary>
    public abstract class ChoiceTimer : MonoBehaviour
    {
        /// <summary>
        /// If this object should not be destroyed when the scene is unloaded.
        /// </summary>
        public bool dontDestroyOnLoad = true;

        /// <summary>
        /// The choice node that the timer is being displayed for.
        /// </summary>
        [NonSerialized]
        public ChoiceNode choiceNode;

        /// <summary>
        /// A callback for when the choice timer has started.
        /// </summary>
        [Header("Events"), Space(4)]
        public UnityEvent onStart = new();

        /// <summary>
        /// A callback for when the choice timer has completed.
        /// </summary>
        [Space(4)]
        public UnityEvent onComplete = new();

        /// <summary>
        /// The amount of time remaining.
        /// </summary>
        [NonSerialized]
        public float timeRemaining;

        /// <summary>
        /// If the choice timer is open.
        /// </summary>
        public abstract bool IsOpen { get; }

        /// <summary>
        /// The active instance.
        /// </summary>
        public static ChoiceTimer Instance { get; protected set; }

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

        protected virtual void Update()
        {
            if (choiceNode == null)
            {
                return;
            }

            timeRemaining -= Time.deltaTime;
            Refresh();

            if (timeRemaining <= 0)
            {
                SelectTimeoutOption();
                Close();
            }
        }

        /// <summary>
        /// Automatically selects the timeout option based on the choice node settings.
        /// </summary>
        protected virtual void SelectTimeoutOption()
        {
            if (choiceNode == null)
            {
                return;
            }

            var timeoutChoice = choiceNode.GetTimeoutChoice();

            if (!timeoutChoice.HasValue)
            {
                FeelSpeakLogger.LogWarning("Feel Speak: failed to automatically select timeout option. Timeout choice returned null.");
                return;
            }

            if (!ChoicesList.Instance)
            {
                if (!DialogueBox.Instance)
                {
                    FeelSpeakLogger.LogWarning("Feel Speak: failed to automatically select timeout option. There's no DialogueBox instance in the scene.");
                    return;
                }

                DialogueBox.Instance.Next(timeoutChoice.Value.index, true);
            }
            else
            {
                ChoicesList.Instance.OnOptionSelected(timeoutChoice.Value);
            }
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public virtual void Stop()
        {
            enabled = false;
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        public virtual void Continue()
        {
            enabled = true;
        }

        /// <summary>
        /// Hides the timer.
        /// </summary>
        public abstract void Hide();

        /// <summary>
        /// Shows the timer.
        /// </summary>
        public abstract void Show();

        /// <summary>
        /// Opens the choice timer based on the choice node.
        /// </summary>
        /// <param name="choiceNode">The choice node.</param>
        /// <returns>True if the timer was successfully opened. Otherwise, false.</returns>
        public virtual bool StartTimer(ChoiceNode choiceNode)
        {
            if (!choiceNode.useTimeout)
            {
                FeelSpeakLogger.LogWarning("Feel Speak: cannot open the choice timer for a choice node with timeout disabled.");
                return false;
            }

            this.choiceNode = choiceNode;
            timeRemaining = choiceNode.timeout;
            Show();
            Continue();
            onStart.Invoke();
            return true;
        }

        /// <summary>
        /// Closes the choice timer.
        /// </summary>
        public virtual void Close()
        {
            Hide();
            Stop();
            onComplete.Invoke();
        }

        /// <summary>
        /// Refreshes the timer UI.
        /// </summary>
        public abstract void Refresh();
    }
}