using UnityEngine;
using TMPro;
namespace MaravStudios.DialogueSystem
{
    public class DialogueSystemButton : MonoBehaviour
    {

        public DialogueSystemDirector director;
        public string GUID;
        public TextMeshProUGUI textMesh;

        [HideInInspector]
        public void newButton(string text, string id)
        {
            textMesh.text = text;
            GUID = id;
        }
        public void Z_Ejecute()
        {

            director.ShowStep(director.lines.NowStep(GUID));
            director.panelSwitch.SetActive(false);
            foreach (var item in director.switchButtonTemporal)
            {
                Destroy(item.gameObject);
            }
            director.switchButtonTemporal.Clear();

        }

    }
}