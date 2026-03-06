using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
namespace MaravStudios.DialogueSystem
{

    using static DialogueSystemInterpreter;
    public class DialogueSystemDirector : MonoBehaviour
    {

        public DialogueSystemSetting setting;
        //[Header("lines")]
        [HideInInspector]
        public string lineStep;
        [HideInInspector]
        public InterpretationXL lines;
        [HideInInspector]
        public DialogueSystemTrigger trigger;
        [HideInInspector]
        public bool writing;
        string textToShow;
        [Header("Stop Time")]
        public float timeShowLetters = 0.05f;
        public float timeShowComma = 0.1f;
        public float timeShowPoint = 0.2f;
        public int everyHowManyLettersSound = 2;
        [Header("Text Box Color")]
        public bool useCharacterColors = true;
        public Color TextColorDefaul = new Color(1, 1, 1, 1);
        public Color bgColorDefaul = new Color(0, 0, 0, 1);

        [Header("GUI")]
        [Tooltip("necessary")]

        public GameObject panel;
        [Space]
        public AudioSource audioSource;
        [Space]
        public GameObject panelText;
        public TextMeshProUGUI dialogueText;
        public Image backGroundDialogueText;

        [Space]
        public GameObject panelSwitch;
        public DialogueSystemButton switchButton;
        [HideInInspector]
        public List<DialogueSystemButton> switchButtonTemporal;


        public CharacterPositionLeft characterPositionLeft = new CharacterPositionLeft();
        public CharacterPositionRight characterPositionRight = new CharacterPositionRight();

        [System.Serializable]
        public class CharacterPositionLeft
        {
            public GameObject namePanel;
            public TextMeshProUGUI characterNameText;
            public Image characterImagen;
            public Animation character;
        }
        [System.Serializable]
        public class CharacterPositionRight
        {
            public GameObject namePanel;
            public TextMeshProUGUI characterNameText;
            public Image characterImagen;
            public Animation character;

        }
        public eventos events;
        [System.Serializable]
        public class eventos
        {
            [Header("Dialogue")]
            public UnityEvent startDialogue;
            public UnityEvent endDialogue;
            [Space]
            [Header("Character'sTurn")]
            public UnityEvent rightCharacterTurn;
            public UnityEvent leftCharacterTurn;
            [Space]
            [Header("Switch")]
            public UnityEvent turnOnSwitch;

        }

        private void Start()
        {

            setting.SetLanguageStartGame();
            // cosas desactivadas
            writing = false;
            trigger = null;
            panel.SafeSetActive(false);
            panelSwitch.SafeSetActive(false);
            switchButton.gameObject.SafeSetActive(false);
            characterPositionRight.namePanel.SafeSetActive(false);
            characterPositionRight.characterImagen?.gameObject.SafeSetActive(false);
            characterPositionLeft.namePanel.SafeSetActive(false);
            characterPositionLeft.characterImagen?.gameObject.SafeSetActive(false);
            // cosas activadas
            panelText.SafeSetActive(true);
            if (characterPositionLeft.character != null)
                characterPositionLeft.character.clip = setting.talkCharacterDefaul;

            if (characterPositionRight.character != null)
                characterPositionRight.character.clip = setting.talkCharacterDefaul;

            foreach (var item in setting.charactersAnimations)
            {
                if (characterPositionLeft.character != null)
                    characterPositionLeft.character.AddClip(item, item.name.Replace("DialogueSystemAnimation_", "").Trim().ToString());
                if (characterPositionRight.character != null)
                    characterPositionRight.character.AddClip(item, item.name.Replace("DialogueSystemAnimation_", "").Trim().ToString());
            }
        }
        public void Play(TextAsset dialogueFile_CSV, DialogueSystemTrigger yourself)
        {
            if (trigger != null)
            {
                return;
            }
            if (setting.languageSelect == -1) setting.languageSelect = 0;
            events.startDialogue.Invoke();
            trigger = yourself;
            panel.SafeSetActive(true);
            lines = new DialogueSystemInterpreter().Interpretar(dialogueFile_CSV);
            // para aniadir aleatoriedad a esto, puedes poner varios inicio, y estos se ejecutaran de forma aleatoria
            List<InterpretationXL.DialogueLines> inicios = new List<InterpretationXL.DialogueLines>();
            foreach (var item in lines.lines)
            {
                if (item.action.Trim() == NodeType.Start.ToString())
                {
                    inicios.Add(item);
                }
            }

            ShowStep(inicios[Random.Range(0, inicios.Count)]);

        }

        public void ShowStep(InterpretationXL.DialogueLines line)
        {
            if (line == null)
            {
                End(); return;
            }
            lineStep = line.GUID.ToString();
            DialogueSystemSetting.Character ch = setting.GetCharacterFromId(line.characterId);

            // //////////////////////////////////////////    Acciones   /////////////////////////////////////

            // decisiones
            if (line.action.Contains(NodeType.Start.ToString()))
            {
                NextStep();
                return;
            }
            else if (line.action.Contains(NodeType.Switch.ToString()))
            {
                panelSwitch.SafeSetActive(true);
                events.turnOnSwitch.Invoke();
                foreach (var item in line.Switch_outputs_GUIDs)
                {
                    var newB = Instantiate(switchButton.gameObject, panelSwitch.transform);
                    DialogueSystemButton bb = newB.GetComponent<DialogueSystemButton>();
                    bb.GUID = item.GUID;
                    bb.textMesh.text = line.Traduccion(setting.GetlanguageSelectID(), item.translations).text;
                    newB.SafeSetActive(true);
                    switchButtonTemporal.Add(bb);
                }
                return;

            }
            // ocultar personajes
            else if (line.action.Contains(NodeType.HideLeft.ToString()))
            {
                characterPositionLeft.namePanel?.SafeSetActive(false);
                characterPositionLeft.characterImagen?.gameObject.SafeSetActive(false);
            }
            else if (line.action.Contains(NodeType.HideRight.ToString()))
            {
                characterPositionRight.namePanel?.SafeSetActive(false);
                characterPositionRight.characterImagen?.gameObject.SafeSetActive(false);
            }


            // animacion
            else if (line.action.Contains(NodeType.Animation.ToString()))
            {

                if (line.orientation == "Right")
                {
                    if (characterPositionRight.character != null)
                    {
                        characterPositionRight.character.Stop();
                        if (line.animation == null || line.animation == "Defaul")
                            characterPositionRight.character.PlayQueued(setting.talkCharacterDefaul.name);
                        else
                            characterPositionRight.character.PlayQueued(line.animation);
                    }

                }
                else
                {
                    if (characterPositionLeft.character != null)
                    {
                        characterPositionLeft.character.Stop();
                        if (line.animation == null || line.animation == "Defaul")
                            characterPositionLeft.character.PlayQueued(setting.talkCharacterDefaul.name);
                        else
                            characterPositionLeft.character.PlayQueued(line.animation);
                    }
                }
                dialogueText.text = "";
                try
                {
                    Invoke("NextStep", characterPositionLeft.character.GetClip(line.animation).averageDuration);
                }
                catch (System.Exception)
                {
                    Invoke("NextStep", 0.3f);
                }
            }

            // Ejecutar accion espesifica
            else if (line.action.Contains(NodeType.Trigger.ToString()))
            {
                trigger.trigger.Invoke();
            }

            // ///////////////////////////////////////////   Seleccion de la posicion del personaje    /////////////////////////////////////////
            else if (line.action.Contains(NodeType.Dialogue.ToString()))
            {  // si no es ninguna de las anteriors, significa que puede hacer estas funciones, porque es un nodo de dialogo 
                switch (ch.positionDefaul)
                {
                    case DialogueSystemSetting.Character.Position.right:
                        events.rightCharacterTurn.Invoke();
                        characterPositionRight.namePanel.SafeSetActive(true);
                        characterPositionRight.characterImagen?.gameObject.SafeSetActive(true);
                        if (characterPositionRight.characterNameText != null)
                            characterPositionRight.characterNameText.text = ch._name;
                        if (characterPositionRight.characterImagen != null)
                            characterPositionRight.characterImagen.sprite = ch.GetvariantsSprite(line.variantSpriteId);
                        if (characterPositionRight.character != null)
                        {
                            characterPositionRight.character.Stop();
                            if (line.animation == null || line.animation == "Defaul")
                                characterPositionRight.character.PlayQueued(setting.talkCharacterDefaul.name);
                            else
                                characterPositionRight.character.PlayQueued(line.animation);
                        }
                        break;
                    case DialogueSystemSetting.Character.Position.left:
                        events.leftCharacterTurn.Invoke();
                        characterPositionLeft.namePanel.SafeSetActive(true);
                        characterPositionLeft.characterImagen?.gameObject.SafeSetActive(true);
                        if (characterPositionLeft.characterNameText != null)
                            characterPositionLeft.characterNameText.text = ch._name;
                        if (characterPositionLeft.characterImagen != null)
                            characterPositionLeft.characterImagen.sprite = ch.GetvariantsSprite(line.variantSpriteId);
                        if (characterPositionLeft.character != null)
                        {
                            characterPositionLeft.character.Stop();
                            if (line.animation == null || line.animation == "Defaul")
                            {
                                characterPositionLeft.character.PlayQueued(setting.talkCharacterDefaul.name);
                            }
                            else
                                characterPositionLeft.character.PlayQueued(line.animation);
                        }

                        break;
                    default:
                        Debug.LogError("Este error es imposible");
                        break;
                }
            }


            // //////////////////////////////////////    texto    ///////////////////////////////////
            textToShow = line.Traduccion(setting.GetlanguageSelectID(), line.translations).text;
            if ((textToShow == "" || textToShow == null) && line.action != NodeType.Animation.ToString())
            {
                NextStep();
                return;
            }

            // texto

            StopAllCoroutines();
            StartCoroutine(WriteNextChar(textToShow, ch));
            if (useCharacterColors)
            {
                if (ch.bgColor == ch.TextColor)
                {
                    dialogueText.color = TextColorDefaul;
                    backGroundDialogueText.color = bgColorDefaul;

                }
                else
                {
                    dialogueText.color = ch.TextColor;
                    backGroundDialogueText.color = ch.bgColor;
                }

            }
        }
        int ehmls;
        IEnumerator WriteNextChar(string line, DialogueSystemSetting.Character ch)
        {
            ehmls = 0;
            writing = true;
            dialogueText.text = "";
            int charIndex = 0;
            while (charIndex < line.Length)
            {
                dialogueText.text += line[charIndex];

                switch (line[charIndex])
                {
                    case ',':
                        yield return new WaitForSeconds(timeShowComma);
                        break;
                    case '.':
                        yield return new WaitForSeconds(timeShowPoint);
                        break;
                    default:
                        yield return new WaitForSeconds(timeShowLetters);
                        if (ehmls <= 0)
                        {
                            if (ch.soundSpeak != null)
                            {
                                audioSource.resource = ch.soundSpeak;
                                audioSource.Play();
                            }
                            else if (setting.soundSpeakDefault != null)
                            {
                                audioSource.resource = setting.soundSpeakDefault;
                                audioSource.Play();
                            }
                            ehmls = everyHowManyLettersSound;
                        }
                        else
                        {
                            ehmls--;
                        }

                        break;
                }
                charIndex++;
            }
            writing = false;
        }



        public void NextStep()
        {
            if (panelSwitch.activeInHierarchy) return;
            if (writing)
            {
                StopAllCoroutines();
                dialogueText.text = textToShow;
                writing = false;
                return;
            }
            string a = lines.GetOutputGUID(lineStep);
            if (a == null)
                End();
            else
                ShowStep(lines.NowStep(a));

        }

        public void End()
        {
            StopAllCoroutines();
            trigger = null;
            events.endDialogue.Invoke();
            panel.SafeSetActive(false);
            lines = null;
            lineStep = null;
            Start();
        }
    }
    public static class GameObjectExtensions
    {
        public static void SafeSetActive(this GameObject gameObject, bool value)
        {
            if (gameObject != null)
            {
                gameObject.SetActive(value);
            }
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(DialogueSystemDirector))]
    [CanEditMultipleObjects]
    public class Editor_DialogueSystemDirector : Editor
    {
        SerializedProperty setting;
        void OnEnable()
        {

            setting = serializedObject.FindProperty("setting");

        }
        public override void OnInspectorGUI()
        {

            serializedObject.Update();


            if (setting.objectReferenceValue == null)
            {
                EditorGUILayout.PropertyField(setting);

                serializedObject.ApplyModifiedProperties();
                return;
            }

            SerializedProperty property = serializedObject.GetIterator();
            property.NextVisible(true);  // Saltar la propiedad "script" inicial

            while (property.NextVisible(false))
            {
                EditorGUILayout.PropertyField(property, true);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif

}