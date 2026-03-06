//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using Esper.FeelSpeak.Overworld;
using UnityEngine;

namespace Esper.FeelSpeak
{
    /// <summary>
    /// Initializes Feel Speak.
    /// </summary>
    [DefaultExecutionOrder(-1)]
    public class FeelSpeakInitializer : MonoBehaviour
    {
        /// <summary>
        /// The speaker component of the player character.
        /// </summary>
        [SerializeField]
        protected Speaker playerSpeaker;

        /// <summary>
        /// The main camera.
        /// </summary>
        [SerializeField] 
        protected Camera mainCamera;

        /// <summary>
        /// The audio source that will play Feel Speak's sounds that are related to FX or UI.
        /// </summary>
        [SerializeField]
        protected AudioSource fxAudioSource;

        /// <summary>
        /// If the connection to Feel Speak's database should be terminated on quit.
        /// </summary>
        [SerializeField]
        protected bool terminateConnectionOnQuit;

        private void Awake()
        {
            FeelSpeak.Initialize(playerSpeaker, mainCamera, fxAudioSource);
        }

        private void OnApplicationQuit()
        {
            if (terminateConnectionOnQuit)
            {
                FeelSpeak.Terminate();
            }
        }
    }
}