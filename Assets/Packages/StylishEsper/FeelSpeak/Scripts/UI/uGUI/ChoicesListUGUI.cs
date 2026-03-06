//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using Esper.FeelSpeak.Graph;
using Esper.FeelSpeak.Graph.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Esper.FeelSpeak.UI.UGUI
{
    /// <summary>
    /// Feel Speak's choices list display for uGUI.
    /// </summary>
    public class ChoicesListUGUI : ChoicesList
    {
        /// <summary>
        /// The GameObject that contains all the content.
        /// </summary>
        [Header("UI")]
        [SerializeField]
        protected GameObject content;

        /// <summary>
        /// The choice prefab instantiated for each possible choice.
        /// </summary>
        [SerializeField]
        public ChoiceUGUI choicePrefab;

        /// <summary>
        /// The parent transform of all instantiated choices.
        /// </summary>
        [SerializeField]
        public RectTransform choiceContainer;

        /// <summary>
        /// The choice container layout group.
        /// </summary>
        protected VerticalLayoutGroup layoutGroup;

        /// <summary>
        /// The list of instantiated choices.
        /// </summary>
        protected List<ChoiceUGUI> loadedChoices = new();

        public override bool IsOpen { get => content.activeSelf; }

        protected override void Awake()
        {
            base.Awake();
            layoutGroup = choiceContainer.GetComponent<VerticalLayoutGroup>();
        }

        public override void Open(ChoiceNode choiceNode)
        {
            base.Open(choiceNode);
            RefreshList();
        }

        public override void Hide()
        {
            content.SetActive(false);
        }

        public override void Show()
        {
            content.SetActive(true);
        }

        public override void MakeChoice(Choice choice)
        {
            StartCoroutine(MakeChoiceCoroutine(choice));
        }

        /// <summary>
        /// Confirms a choice made after briefly marking it.
        /// </summary>
        /// <param name="choice">The choice made.</param>
        /// <returns>Yields for a small amount of time.</returns>
        protected IEnumerator MakeChoiceCoroutine(Choice choice)
        {
            if (ChoiceTimer.Instance && ChoiceTimer.Instance.IsOpen)
            {
                ChoiceTimer.Instance.Stop();
            }

            for (int i = 0; i < loadedChoices.Count; i++)
            {
                var c = loadedChoices[i];

                if (i != choice.index)
                {
                    var colors = c.button.colors;
                    colors.disabledColor = colors.normalColor;
                    c.button.colors = colors;
                }

                c.button.interactable = false;
            }

            yield return new WaitForSeconds(FeelSpeak.Settings.choiceDisplayLength);

            if (!DialogueBox.Instance)
            {
                FeelSpeakLogger.LogWarning("Feel Speak: selected choice did nothing! Unable to find the dialogue box instance in the scene!");
                yield break;
            }

            Close();
            Clear();

            if (ChoiceTimer.Instance && ChoiceTimer.Instance.IsOpen)
            {
                ChoiceTimer.Instance.Close();
            }

            DialogueBox.Instance.Next(choice.index, true);
        }

        public override void Clear()
        {
            foreach (var choice in loadedChoices)
            {
                Destroy(choice.gameObject);
            }

            loadedChoices.Clear();
        }

        public override void RefreshList()
        {
            if (choiceNode == null)
            {
                FeelSpeakLogger.LogWarning("Feel Speak: cannot refresh choices list when the choice node and speaker reference are null!");
                return;
            }

            Clear();

            if (choiceNode != null)
            {
                for (int i = 0; i < choiceNode.choices.Count; i++)
                {
                    var choice = choiceNode.choices[i];
                    var ui = Instantiate(choicePrefab, choiceContainer);
                    ui.SetChoice(choice);
                    loadedChoices.Add(ui);
                }
            }

            if (layoutGroup)
            {
                var parent = choiceContainer.parent;

                if (parent)
                {
                    var parentRt = parent as RectTransform;
                    float parentHeight = parentRt.rect.height;
                    float preferredHeight = layoutGroup.preferredHeight;
                    choiceContainer.sizeDelta = new Vector2(choiceContainer.sizeDelta.x, Mathf.Max(parentHeight, preferredHeight));
                }
            }
        }
    }
}