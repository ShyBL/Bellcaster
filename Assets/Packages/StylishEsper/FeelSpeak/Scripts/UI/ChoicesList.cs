//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using Esper.FeelSpeak.Graph;
using Esper.FeelSpeak.Graph.Data;
using Esper.FeelSpeak.Overworld.Data;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Esper.FeelSpeak.UI
{
    /// <summary>
    /// Displays choices that the player can select for choice nodes.
    /// </summary>
    public abstract class ChoicesList : MonoBehaviour
    {
        /// <summary>
        /// If this object should not be destroyed when the scene is unloaded.
        /// </summary>
        public bool dontDestroyOnLoad = true;

        /// <summary>
        /// The choice node that choices are being displayed for.
        /// </summary>
        [NonSerialized]
        public ChoiceNode choiceNode;

        /// <summary>
        /// A callback for when the choices list is opened.
        /// </summary>
        [Header("Events"), Space(4)]
        public UnityEvent onOpen = new();

        /// <summary>
        /// A callback for when the choices list is closed.
        /// </summary>
        [Space(4)]
        public UnityEvent onClose = new();

        /// <summary>
        /// A callback for when a choice is selected. This accepts 1 argument: the choice (Choice).
        /// </summary>
        [Space(4)]
        public UnityEvent<Choice> onChoiceSelected = new();

        /// <summary>
        /// If the choices list is open.
        /// </summary>
        public abstract bool IsOpen { get; }

        /// <summary>
        /// The active instance.
        /// </summary>
        public static ChoicesList Instance { get; protected set; }

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
        /// Hides the choices.
        /// </summary>
        public abstract void Hide();

        /// <summary>
        /// Shows the choices.
        /// </summary>
        public abstract void Show();

        /// <summary>
        /// Makes the choice. This will briefly mark the choice made.
        /// </summary>
        /// <param name="choice">The choice made.</param>
        public abstract void MakeChoice(Choice choice);

        /// <summary>
        /// Removes all choices.
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Opens the choices list and displays choices based on the choice node.
        /// </summary>
        /// <param name="choiceNode">The choice node.</param>
        public virtual void Open(ChoiceNode choiceNode)
        {
            this.choiceNode = choiceNode;
            Show();
            onOpen.Invoke();
        }

        /// <summary>
        /// Closes the choices list.
        /// </summary>
        public virtual void Close()
        {
            Hide();
            onClose.Invoke();
        }

        /// <summary>
        /// Refreshes the choices list.
        /// </summary>
        public abstract void RefreshList();

        /// <summary>
        /// Handles a choice option selection.
        /// </summary>
        /// <param name="choice">The selected choice data.</param>
        public void OnOptionSelected(Choice choice)
        {
            onChoiceSelected?.Invoke(choice);
            MakeChoice(choice);
        }
    }
}