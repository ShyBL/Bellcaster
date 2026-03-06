using System.Collections.Generic;
using System;
using System.Xml;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml.Serialization;

namespace MaravStudios.DialogueSystem
{
    public enum NodeType
    {
        Dialogue, Switch, Trigger, HideRight, HideLeft, Start, Animation
    }

    public class DialogueSystemNodePortColor
    {
        static public Color32 start = new Color32(3, 201, 136, 255);
        static public Color32 Dialogue = new Color32(92, 213, 237, 255);
        static public Color32 Switch = new Color32(185, 100, 245, 255);
        static public Color32 Animations = new Color32(177, 35, 35, 255);
        static public Color32 others = new Color32(255, 255, 255, 255);

    }
    public class DialogueSystemLogError
    {
        static public void errorLectura()
        {
            Debug.LogError(
                "Read error: check the CSV, at some point there is a line break where it should not be or a comma(,) is missing." +
                "\n\n" +
                "Error de lectura: revisa el CSV, en algún momento hay un salto de línea donde no debería o falta alguna coma(,)." +
                "\n\n" +
                "Lesefehler: Überprüfen Sie die CSV, irgendwo ist ein Zeilenumbruch, wo er nicht sein sollte, oder es fehlt ein Komma(,)." +
                "\n\n" +
                "Erreur de lecture : vérifiez le CSV, il y a un saut de ligne là où il ne devrait pas y en avoir ou une virgule (,) est manquante." +
                "\n\n");
        }
    }

    public class DialogueSystemInterpreter
    {
        public InterpretationXL Interpretar(TextAsset xmlFile)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(InterpretationXL));
            using (StringReader reader = new StringReader(xmlFile.text))
            {
                InterpretationXL data = (InterpretationXL)serializer.Deserialize(reader);

                return data;
            }
        }

        static public void Save(InterpretationXL newData, TextAsset xmlFile)
        {


            string filePath = AssetDatabase.GetAssetPath(xmlFile);

            XmlSerializer serializer = new XmlSerializer(typeof(InterpretationXL));
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, newData);
            }


            AssetDatabase.Refresh();
            xmlFile = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
        }


        // Este guarda todas las variables de la interpretacion en una lista facil de usar por el Director


        [Serializable]
        public class InterpretationXL
        {
            // Datos generales del dialogo
            [XmlElement("DialogueLines")]
            public List<DialogueLines> lines = new List<DialogueLines>();
            [XmlElement("lenguageSelectOnColumns")]
            public int lenguageSelectOnColumns;

            // viables de la lineas de texto
            [Serializable]
            public class DialogueLines
            {
                [XmlElement("GUID")]
                public string GUID;
                [XmlElement("output_GUID")]
                public string output_GUID;
                [XmlArray("Switch_outputs_GUIDs")]
                [XmlArrayItem("Switch_output")]
                public List<Switch_outputs_GUID> Switch_outputs_GUIDs = new List<Switch_outputs_GUID>();
                [XmlElement("action")]
                public string action;
                [XmlElement("characterId")]
                public string characterId;
                [XmlElement("position")]
                public Vector2 position;
                [XmlElement("variantSpriteId")]
                public string variantSpriteId;
                [XmlElement("animation")]
                public string animation;
                [XmlElement("orientation")]
                public string orientation;
                [XmlArray("translations")]
                [XmlArrayItem("translation")]
                public List<Translation> translations = new List<Translation>();

                [Serializable]
                public class Translation
                {
                    [XmlElement("id")]
                    public string id;

                    [XmlElement("text")]
                    public string text;

                }
                [Serializable]
                public class Switch_outputs_GUID
                {
                    [XmlElement("GUID")]
                    public string GUID;

                    [XmlArray("translations")]
                    [XmlArrayItem("translation")]
                    public List<Translation> translations = new List<Translation>();
                }
                public Translation Traduccion(string Identificador, List<Translation> t)
                {
                    foreach (var item in t)
                    {
                        if (Identificador == item.id)
                        {
                            return item;
                        }
                    }
                    Translation newT = new Translation()
                    {
                        id = Identificador,
                        text = ""
                    };
                    t.Add(newT);
                    return newT;
                }
            }
            public string GetOutputGUID(string i)
            {
                foreach (var item in lines)
                {
                    if (item.GUID == i)
                    {// identificamos quien soyo 
                        return item.output_GUID;
                    }
                }
                return null;
            }
            public DialogueLines NowStep(string i)
            {
                foreach (var item in lines)
                {
                    if (item.GUID == i)
                    {
                        return item;
                    }
                }
                return null;
            }
        }


    }



#if UNITY_EDITOR

    [CustomEditor(typeof(DialogueSystemInteraction))]
    [CanEditMultipleObjects]
    public class Editor_DialogueSystemInteraction : Editor
    {
        DialogueSystemInteraction dialogueSystemInteraction;

        SerializedProperty dialogueSystemTrigger, signal, Interaction, dimensions, boxSize, layerMask;
        SerializedProperty activateWhenEntering, executeButton, executeTag;
        void OnEnable()
        {

            dialogueSystemTrigger = serializedObject.FindProperty("dialogueSystemTrigger");
            signal = serializedObject.FindProperty("signal");
            Interaction = serializedObject.FindProperty("Interaction");
            dimensions = serializedObject.FindProperty("dimensions");
            boxSize = serializedObject.FindProperty("boxSize");
            layerMask = serializedObject.FindProperty("layerMask");

            activateWhenEntering = serializedObject.FindProperty("activateWhenEntering");
            executeButton = serializedObject.FindProperty("executeButton");
            executeTag = serializedObject.FindProperty("executeTag");

        }
        public override void OnInspectorGUI()
        {
            dialogueSystemInteraction = (DialogueSystemInteraction)target;


            serializedObject.Update();


            if (dialogueSystemTrigger.objectReferenceValue == null)
            {
                EditorGUILayout.PropertyField(dialogueSystemTrigger);
                serializedObject.ApplyModifiedProperties();
                return;
            }
            EditorGUILayout.PropertyField(dialogueSystemTrigger);
            EditorGUILayout.PropertyField(signal);
            EditorGUILayout.PropertyField(activateWhenEntering);
            EditorGUILayout.PropertyField(executeButton);
            EditorGUILayout.PropertyField(executeTag);

            EditorGUILayout.PropertyField(dimensions);

            switch (dialogueSystemInteraction.dimensions)
            {
                case DialogueSystemInteraction.dimenciones._2D:
                    Vector2 box = EditorGUILayout.Vector2Field("Box Size", boxSize.vector3Value);

                    boxSize.vector3Value = box;

                    break;
                case DialogueSystemInteraction.dimenciones._3D:
                    Vector3 box3 = EditorGUILayout.Vector3Field("Box Size", boxSize.vector3Value);
                    boxSize.vector3Value = box3;
                    break;
                default:
                    break;
            }

            EditorGUILayout.PropertyField(layerMask);


            EditorGUILayout.PropertyField(Interaction);

            serializedObject.ApplyModifiedProperties();

        }


    }






#endif
}