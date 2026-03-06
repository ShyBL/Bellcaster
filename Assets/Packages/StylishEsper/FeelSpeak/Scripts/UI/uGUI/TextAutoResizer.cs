//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using UnityEngine;
using TMPro;

namespace Esper.FeelSpeak.UI.UGUI
{
    /// <summary>
    /// Automatically resizes TextMeshProUGUI.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextAutoResizer : MonoBehaviour
    {
        private TextMeshProUGUI tmpText;
        private RectTransform rectTransform;

        private void Awake()
        {
            tmpText = GetComponent<TextMeshProUGUI>();
            rectTransform = GetComponent<RectTransform>();

            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
        }

        private void Start()
        {
            ResizeToFitText();
        }

        private void OnDestroy()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        }

        private void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        }

        private void OnEnable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
        }

        private void OnApplicationQuit()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        }

        private void OnTextChanged(Object changedObject)
        {
            if (changedObject == tmpText)
            {
                ResizeToFitText();
            }
        }

        private void ResizeToFitText()
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, tmpText.preferredHeight);
        }
    }
}