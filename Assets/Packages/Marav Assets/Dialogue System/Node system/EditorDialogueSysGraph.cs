using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
namespace MaravStudios.DialogueSystem
{
    public class EditorDialogueSysGraph : EditorWindow
    {

        public ObjectField textAssetField;
        public DialogueSystemSetting character;
        EditorDialogueSysGraphView _graphView;
        [MenuItem("Window/Dialogue System/Dialogue Editor")]
        static void Open()
        {

            var window = GetWindow<EditorDialogueSysGraph>();
            string icono = "💬";
            window.titleContent = new GUIContent(text: icono + " " + "Dialogue Editor Graph");

        }
        public void CreateGUI()
        {



        }
        private void OnEnable()
        {
            string[] guid = AssetDatabase.FindAssets("t:DialogueSystemSetting");
            if (guid.Length > 0)
            {
                string patth = AssetDatabase.GUIDToAssetPath(guid[0]);
                character = AssetDatabase.LoadAssetAtPath<DialogueSystemSetting>(patth);
            }
            rootVisualElement.AddToClassList("rootVisualElement");
            _graphView = new EditorDialogueSysGraphView();

            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);

            // Cargar el archivo uss
            string path = "EditorDialogueSysStyle_cduioouisdj.uss";
            string[] guids = AssetDatabase.FindAssets("EditorDialogueSysStyle_cduioouisdj");
            if (guids.Length > 0)
            {
                path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
                rootVisualElement.styleSheets.Add(styleSheet);
            }

            GenerateToolbar();



        }

        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);

        }


        // //////////////////////////////////////////////   Tool bar  /////////////////////////////////////////////
        void GenerateToolbar()
        {
            var toolbar = new Toolbar();
            toolbar.AddToClassList("Toolbar");
            var div1 = new VisualElement();
            div1.AddToClassList("div");
            var div2 = new VisualElement();
            div2.AddToClassList("divCenter");
            var div3 = new VisualElement();
            div3.AddToClassList("divLeft");





            // Creamos los nodos
            var createNode = new DropdownField()
            {
                choices = new List<string> { "Start", "Dialogue", "Switch", "Trigger", "Animation", "Hide/Hide Right", "Hide/Hide Left" },
                value = "Create Node"
            };
            createNode.RegisterValueChangedCallback(evt =>
            {
                switch (evt.newValue)
                {
                    case "Start":
                        _graphView.CreateNode(NodeType.Start);
                        break;
                    case "Dialogue":
                        _graphView.CreateNode(NodeType.Dialogue);
                        break;
                    case "Switch":
                        _graphView.CreateNode(NodeType.Switch);
                        break;
                    case "Hide Right":
                        _graphView.CreateNode(NodeType.HideRight);
                        break;
                    case "Hide Left":
                        _graphView.CreateNode(NodeType.HideLeft);
                        break;
                    case "Trigger":
                        _graphView.CreateNode(NodeType.Trigger);
                        break;
                    case "Animation":
                        _graphView.CreateNode(NodeType.Animation);
                        break;
                }
                //createNode.value = "Create Node";
            });
            div1.Add(createNode);
            List<string> choices = new List<string>();
            choices.AddRange(character.languageList);
            choices.Add("Show All");
            var languageSelect = new DropdownField()
            {
                choices = choices,
                value = character.languageList[0]
            };
            languageSelect.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue == "Show All")
                    character.languageSelect = -1;
                else
                    character.languageSelect = character.GetIndexLanguageFromName(evt.newValue);
                _graphView.UpdateAll();
            });
            div1.Add(new Label() { text = "Language" });
            div1.Add(languageSelect);


            textAssetField = new ObjectField() { objectType = typeof(TextAsset) };

            textAssetField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue != null)
                {
                    TextAsset selectedAsset = evt.newValue as TextAsset;
                    _graphView.LoadDialogueFile(selectedAsset);
                    languageSelect.value = character.languageList[0];
                }
                else
                {
                    TextAsset selectedAsset = evt.previousValue as TextAsset;
                    _graphView.UnLoadDialogueFile();
                }
            });
            div2.Add(textAssetField);

            var saveButton = new Button(clickEvent: () =>
            {
                _graphView.SaveDialogueFile(textAssetField.value as TextAsset);
            });
            saveButton.text = "Save";
            div3.Add(saveButton);

            var loadButton = new Button(clickEvent: () =>
            {

                if (textAssetField.value != null)
                {
                    TextAsset selectedAsset = textAssetField.value as TextAsset;
                    _graphView.LoadDialogueFile(selectedAsset);
                }
            });
            loadButton.text = "Load";
            div3.Add(loadButton);
            var autosave = new Toggle()
            {
                label = "Auto Save",
                value = true
            };
            autosave.RegisterValueChangedCallback(evt =>
            {
                _graphView.AutoSave = evt.newValue;
            });
            div3.Add(autosave);
            _graphView.AutoSave = autosave.value;


            var setting = new Button(clickEvent: () =>
            {
                Selection.activeObject = character;
            })
            { text = "Setting" };
            div3.Add(setting);

            toolbar.Add(div1);
            toolbar.Add(div2);
            toolbar.Add(div3);

            rootVisualElement.Add(toolbar);
            VisualElement shadow = new VisualElement();
            shadow.AddToClassList("shadow");
            rootVisualElement.Add(shadow);
        }

    }
}