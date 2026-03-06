using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.Audio;

namespace MaravStudios.DialogueSystem
{
    [Tooltip("There should only be one Setting within the project")]
    [CreateAssetMenu(fileName = "Setting", menuName = "DialogueSystem/Setting (only one in the project)")]
    public class DialogueSystemSetting : ScriptableObject
    {
        [Header("Animations")]
        public AnimationClip talkCharacterDefaul;
        public AnimationClip[] charactersAnimations;

        [Header("Sound of speaking default")]
        [Tooltip("Use a Audio Random Container")]
        [NoAudioClip]
        public AudioResource soundSpeakDefault;
        // Lenguajes
        [Header("Language")]
        public int languageSelect = 0;
        public List<string> languageList;

        // esto es para hacer facil el tomar el lenguaje seleccionado
        static string LanguageSaveKey = "LanguageSaveKey_d54svnjweusld2skshihnsh";
        public void SetLanguage(int i)
        {
            languageSelect = i;
            PlayerPrefs.SetInt(LanguageSaveKey, i);
        }
        public void SetLanguageStartGame()
        {
            languageSelect = PlayerPrefs.GetInt(LanguageSaveKey, 0);
        }
        public string GetlanguageSelectID()
        {
            if (languageList.Count < languageSelect || languageList.Count == 0)
            {
                Debug.LogError("Invalid selected language");
                return languageList[0];
            }
            else
            {
                return languageList[languageSelect];
            }
        }

        [Space]

        [Header("Characters")]
        public List<Character> characterList;

        // Todas las variables del personaje
        [Serializable]
        public class Character
        {
            public string _name;
            [Space]
            [SpriteThumbnail(80)]
            public Sprite spriteDefaul;
            [Space]
            public Position positionDefaul;
            public enum Position
            {
                right, left
            }
            public List<variantSprite> variantsSprite = new List<variantSprite>();
            [Space]
            public Color TextColor = new Color(1, 1, 1, 1);
            public Color bgColor = new Color(1, 1, 1, 1);
            [Space]
            [Tooltip("Use a Audio Random Container")]
            [NoAudioClip]
            public AudioResource soundSpeak;
            [Space]
            [TextArea]
            public string description;

            public Sprite GetvariantsSprite(string id)
            {
                // Debug.Log("|"+ id.Trim() + "|");  spriteDefaul
                if (id == null) return spriteDefaul;
                if (id.Trim() == "" || id.Trim() == "sprite Defaul")
                {
                    // Debug.Log("spriteDefaul");
                    return spriteDefaul;
                }

                foreach (var item in variantsSprite)
                {
                    if (item._name.Trim() == id.Trim())
                    {
                        return item.sprite;
                    }
                }
                //Debug.LogError("variants Sprite invalid");
                return spriteDefaul;

            }

        }
        // para seleccionar un character facilmente
        public Character GetCharacterFromId(string id)
        {
            Character character = new Character();
            if (id == null)
            {
                return character;
            }
            if (id == "")
            {
                return character;
            }

            foreach (var item in characterList)
            {
                if (item._name == id.Trim())
                {

                    return item;
                }
            }
            // Debug.LogError("invalid id character");
            return character;
        }

        public int GetIndexLanguageFromName(string _name)
        {
            int language = 0;
            for (int i = 0; i < languageList.Count; i++)
            {
                if (languageList[i] == _name)
                {
                    language = i;

                    break;
                }
            }
            return language;
        }


        // Variaciones de sprites
        [Serializable]
        public class variantSprite
        {

            public string _name;
            [SpriteThumbnail(80)]
            public Sprite sprite;

        }

    }

    // Define el atributo personalizado
    public class SpriteThumbnailAttribute : PropertyAttribute
    {
        public float size;

        public SpriteThumbnailAttribute(float size = 64f)
        {
            this.size = size;
        }
    }
    public class NoAudioClipAttribute : PropertyAttribute { }
    // Implementa el PropertyDrawer para mostrar la miniatura
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SpriteThumbnailAttribute))]
    public class SpriteThumbnailDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Obt�n el tama�o del atributo
            SpriteThumbnailAttribute thumbnail = (SpriteThumbnailAttribute)attribute;
            float thumbnailSize = thumbnail.size;

            // Define la posici�n para el campo y la miniatura
            Rect fieldRect = new Rect(position.x, position.y, position.width - thumbnailSize - 5, EditorGUIUtility.singleLineHeight);
            Rect thumbnailRect = new Rect(position.x + position.width - thumbnailSize, position.y, thumbnailSize, thumbnailSize);

            // Dibuja el campo para asignar el sprite
            EditorGUI.PropertyField(fieldRect, property, label);

            // Dibuja la miniatura si el valor no es nulo y es un Sprite
            if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue is Sprite sprite)
            {
                Texture2D texture = sprite.texture;
                if (texture != null)
                {
                    EditorGUI.DrawPreviewTexture(thumbnailRect, texture);
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Ajusta la altura para que incluya la miniatura
            SpriteThumbnailAttribute thumbnail = (SpriteThumbnailAttribute)attribute;
            return Mathf.Max(EditorGUIUtility.singleLineHeight, thumbnail.size);
        }
    }


    [CustomPropertyDrawer(typeof(NoAudioClipAttribute))]
    public class NoAudioClipDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Determina si se debe mostrar el HelpBox (cuando se asigna un AudioClip)
            bool showHelpBox = property.objectReferenceValue != null && property.objectReferenceValue is AudioClip;

            // Define las alturas para cada elemento
            float helpBoxHeight = EditorGUIUtility.singleLineHeight * 2;
            float fieldHeight = EditorGUIUtility.singleLineHeight;
            float spacing = 2f;

            // Calcula el rectángulo para el HelpBox y para el ObjectField
            Rect helpBoxRect = new Rect(position.x, position.y, position.width, helpBoxHeight);
            Rect fieldRect = new Rect(position.x, position.y + (showHelpBox ? helpBoxHeight + spacing : 0), position.width, fieldHeight);

            // Dibuja el HelpBox por encima, si es necesario
            if (showHelpBox)
            {
                EditorGUI.HelpBox(helpBoxRect, "❌ You cannot assign an AudioClip here. Use an AudioRandomContainer.", MessageType.Error);
            }

            // Dibuja el ObjectField debajo del HelpBox (o en la posición original si no se muestra el HelpBox)
            EditorGUI.BeginProperty(fieldRect, label, property);
            UnityEngine.Object audioObj = EditorGUI.ObjectField(fieldRect, label, property.objectReferenceValue, typeof(AudioResource), false);
            property.objectReferenceValue = audioObj;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Se suma la altura del HelpBox solo si se debe mostrar
            bool showHelpBox = property.objectReferenceValue != null && property.objectReferenceValue is AudioClip;
            float height = EditorGUIUtility.singleLineHeight;
            if (showHelpBox)
            {
                height += EditorGUIUtility.singleLineHeight * 2 + 2; // 2 es el espacio adicional
            }
            return height;
        }
    }

#endif
}