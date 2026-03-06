using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using System.IO;

namespace MaravStudios.DialogueSystem
{
    public class DialogueSystemTrigger : MonoBehaviour
    {
        public DialogueSystemDirector director;
        public UnityEvent trigger;
        public TextAsset dialogueFile;

        private void Start()
        {
            if (director == null)
                director = FindAnyObjectByType<DialogueSystemDirector>();

        }

        public void Z_TriggerTheDialogue()
        {
            director.Play(dialogueFile, this);
        }

    }

#if UNITY_EDITOR

    [CustomEditor(typeof(DialogueSystemTrigger))]
    [CanEditMultipleObjects]
    public class Editor_DialogueSystemTrigger : Editor
    {
        SerializedProperty director, trigger, dialogueFile;
        void OnEnable()
        {

            trigger = serializedObject.FindProperty("trigger");
            director = serializedObject.FindProperty("director");
            dialogueFile = serializedObject.FindProperty("dialogueFile");

        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            // las cosas que tiene que estar aplicadar y ocultas

            if (director.objectReferenceValue == null)
            {
                EditorGUILayout.PropertyField(director);
                director.objectReferenceValue = FindAnyObjectByType<DialogueSystemDirector>();
            }

            EditorGUILayout.PropertyField(dialogueFile);
            if (dialogueFile.objectReferenceValue == null)
            {
                EditorGUILayout.Space(10);
                if (GUILayout.Button("Create New dialogueFile"))
                {
                    dialogueFile.objectReferenceValue = NewDialogueFile();
                }
            }
            else
            {
                EditorGUILayout.Space(10);
                if (GUILayout.Button("Open Dialogue Editor"))
                {
                    EditorApplication.ExecuteMenuItem("Window/Dialogue System/Dialogue Editor");
                    EditorDialogueSysGraph dialogueEditor = EditorWindow.GetWindow(typeof(EditorDialogueSysGraph)) as EditorDialogueSysGraph;
                    dialogueEditor.textAssetField.value = dialogueFile.objectReferenceValue;
                }
            }

            EditorGUILayout.Space(10);
            // el trigger
            EditorGUILayout.PropertyField(trigger);
            //EditorGUILayout.Space(10);
            EditorGUILayout.SelectableLabel("To execute the dialog, use the function “Z_TriggerTheDialogue()”.\nOr you can also use the “DialogueSystemInteraction” component");
            //EditorGUILayout.Space(5);

            serializedObject.ApplyModifiedProperties();

            //EditorGUILayout.Space(40);
        }


        TextAsset NewDialogueFile()
        {
            // Abre el explorador de archivos para elegir la ubicación
            string ruta = EditorUtility.SaveFilePanel(
                "Save new dialogueFile",
                Application.dataPath,
                "new dialogueFile",
                "xml"
            );

            // Verifica si el usuario canceló la acción
            if (string.IsNullOrEmpty(ruta))
            {
                return null;
            }
            // Creamos el achivo
            try
            {
                File.WriteAllText(ruta, "");
                Debug.Log($"dialogueFile successfully created in: {ruta}");
            }
            catch (IOException e)
            {
                Debug.LogError($"Error creating the file dialogueFile: {e.Message}");
                return null;
            }
            // Verifica si el archivo está dentro de la carpeta "Assets"
            if (ruta.StartsWith(Application.dataPath))
            {
                string rutaRelativa = "Assets" + ruta.Substring(Application.dataPath.Length);
                AssetDatabase.ImportAsset(rutaRelativa); // Importa el archivo al proyecto
                TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(rutaRelativa);
                // creamos el la primera instruccion que tiene que ser salir de esto
                DialogueSystemInterpreter.InterpretationXL newData = new DialogueSystemInterpreter.InterpretationXL();
                newData.lines = new List<DialogueSystemInterpreter.InterpretationXL.DialogueLines>();
                newData.lines.Add(new DialogueSystemInterpreter.InterpretationXL.DialogueLines()
                {
                    action = NodeType.Start.ToString(),
                    variantSpriteId = "",
                    characterId = "",
                    GUID = Guid.NewGuid().ToString()
                });
                DialogueSystemInterpreter.Save(newData, textAsset);
                return textAsset;// Devuelve el TextAsset importado
            }
            else
            {
                Debug.LogWarning("El archivo creado no está en la carpeta del proyecto y no puede ser importado como TextAsset.");
                return null;
            }
        }
    }


#endif
}