//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using Esper.FeelSpeak.Graph;
using UnityEngine;
using UnityEngine.UI;

namespace Esper.FeelSpeak.UI.UGUI
{
    /// <summary>
    /// Feel Speak's choice timer display for uGUI.
    /// </summary>
    public class ChoiceTimerUGUI : ChoiceTimer
    {
        /// <summary>
        /// The GameObject that contains all the content.
        /// </summary>
        [Header("UI")]
        [SerializeField]
        protected GameObject content;

        /// <summary>
        /// The slider that visually displays the amount of time remaining.
        /// </summary>
        public Slider slider;

        public override bool IsOpen { get => content.activeSelf; }

        public override bool StartTimer(ChoiceNode choiceNode)
        {
            var result = base.StartTimer(choiceNode);

            if (result)
            {
                slider.minValue = 0;
                Refresh();
            }

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

        public override void Refresh()
        {
            if (choiceNode == null)
            {
                return;
            }

            slider.maxValue = choiceNode.timeout;
            slider.value = timeRemaining;
        }
    }
}