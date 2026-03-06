//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Esper.FeelSpeak.UI.UGUI
{
    /// <summary>
    /// Adds link detection to a TMP component.
    /// </summary>
    public class TMPLinkHandler : MonoBehaviour, IPointerClickHandler
    {
        private TMP_Text textMeshPro;

        private void Awake()
        {
            textMeshPro = GetComponent<TMP_Text>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMeshPro, eventData.position, eventData.pressEventCamera);

            if (linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo[linkIndex];
                string linkId = linkInfo.GetLinkID();
                Application.OpenURL(linkId);
            }
        }
    }
}