//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using UnityEngine;

namespace Esper.FeelSpeak.UI
{
    /// <summary>
    /// An object that faces the camera.
    /// </summary>
    public class Billboarded : MonoBehaviour
    {
        /// <summary>
        /// If billboarding is enabled.
        /// </summary>
        public bool isBillboardEnabled = true;

        protected virtual void Update()
        {
            if (!isBillboardEnabled || !FeelSpeak.mainCamera)
            {
                return;
            }

            transform.rotation = FeelSpeak.mainCamera.transform.rotation;
        }
    }
}