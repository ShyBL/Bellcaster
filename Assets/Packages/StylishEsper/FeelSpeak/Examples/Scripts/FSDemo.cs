//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Esper.FeelSpeak.Examples
{
    /// <summary>
    /// Feel Speak's demo helper script.
    /// </summary>
    public class FSDemo : MonoBehaviour
    {
        /// <summary>
        /// If pinky has already been spoken to at least once.
        /// </summary>
        public bool alreadySpokenToPinky;

        private void Start()
        {
            // Get the starting dialogue and trigger it
            var startDialogue = FeelSpeak.GetDialogueGraph("Start");
            startDialogue.Trigger();
        }

        /// <summary>
        /// Checks if the current scene is the 3D demo scene.
        /// </summary>
        /// <returns>True if the active scene name is FS3D.</returns>
        public bool Is3dDemo()
        {
            return SceneManager.GetActiveScene().name == "FS3D";
        }

        /// <summary>
        /// Hides the talk buttons. This is for the 2D demo.
        /// </summary>
        public void HideButtons()
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }

        /// <summary>
        /// Shows the talk buttons. THis is for the 2D demo.
        /// </summary>
        public void ShowButtons()
        {
            transform.GetChild(0).gameObject.SetActive(true);
        }

        /// <summary>
        /// Triggers Pinky's dialogue.
        /// </summary>
        public void TalkToPinky()
        {
            var pinky = FeelSpeak.FindSpeaker("pinky");
            pinky.TriggerDialogue();
        }

        /// <summary>
        /// Triggers Goldy's dialogue.
        /// </summary>
        public void TalkToGoldy()
        {
            var goldy = FeelSpeak.FindSpeaker("goldy");
            goldy.TriggerDialogue();
        }
    }
}