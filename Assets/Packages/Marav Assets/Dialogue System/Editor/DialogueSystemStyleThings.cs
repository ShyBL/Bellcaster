using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
namespace MaravStudios.DialogueSystem
{
    public static class DialogueSystemMenuItems
    {
        [MenuItem("GameObject/UI/Dialogue System Canvas", false, 10)]
        public static void DialogueCanvasInstance()
        {
            // Obtener la ruta del script actual
            string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(ScriptableObject.CreateInstance<DialogueSystemSetting>()));
            string scriptDirectory = Path.GetDirectoryName(scriptPath);

            // Construir la ruta relativa al prefab Dialogue System GUI.prefab
            string prefabPath = Path.Combine(scriptDirectory, "../Dialogue Canvas.prefab");

            // Normalizar la ruta para que Unity la entienda
            prefabPath = Path.GetFullPath(prefabPath).Replace("\\", "/");
            prefabPath = prefabPath.Replace(Application.dataPath, "Assets");



            // Carga el prefab desde la ruta
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab != null)
            {
                // Instancia el prefab en la jerarquía
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                Undo.RegisterCreatedObjectUndo(instance, "Dialogue Canvas");
                instance.name = prefab.name; // Opcional: Configurar el nombre del objeto creado.
            }
            else
            {
                Debug.LogError($"Prefab no encontrado en la ruta: {prefabPath}");
            }
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                // Crear un nuevo EventSystem si no existe
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
            }
        }


        [MenuItem("Window/Dialogue System/Setting")]
        public static void DialogueSetting()
        {
            string[] guid = AssetDatabase.FindAssets("t:DialogueSystemSetting");
            if (guid.Length > 0)
            {
                string patth = AssetDatabase.GUIDToAssetPath(guid[0]);
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<DialogueSystemSetting>(patth);
            }
        }
    }


}
