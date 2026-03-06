//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Esper.FeelSpeak.Editor
{
#if !UNITY_2022
    [UxmlElement]
    public partial class ContextualDropdown : Button
    {
#elif UNITY_2022
    public class ContextualDropdown : Button
    {
        public new class UxmlFactory : UxmlFactory<ContextualDropdown, UxmlTraits> { }
#endif

        protected UnityEngine.Object boundObject;

        public Action<string, FieldInfo> onFieldSelected;
        public Action<string, PropertyInfo> onPropertySelected;
        public Action<string, MethodInfo> onMethodSelected;

        public bool boolReturnTypesOnly;

        protected Type[] supportedTypes = { typeof(int), typeof(float), typeof(bool), typeof(string), typeof(Vector2), 
            typeof(Vector3), typeof(Vector4), typeof(Quaternion), typeof(Color), typeof(Vector2Int), typeof(Vector3Int), 
            typeof(UnityEngine.Object) };

        public ContextualDropdown()
        {
            clicked += ShowDropdown;
        }

        public void Bind(UnityEngine.Object obj)
        {
            boundObject = obj;
        }

        public void Unbind()
        {
            boundObject = null;
        }

        private void ShowDropdown()
        {
            if (!boundObject)
            {
                return;
            }

            var menu = new GenericMenu();

            if (boundObject is GameObject go)
            {
                AddComponentMembersToMenu(menu, go, go.GetType().Name);

                var components = go.GetComponents<Component>();

                foreach (var component in components)
                {
                    AddComponentMembersToMenu(menu, component, component.GetType().Name);
                }
            }
            else if (boundObject is ScriptableObject so)
            {
                AddComponentMembersToMenu(menu, so, so.GetType().Name);
            }
            else
            {
                Debug.LogWarning("Contextual Dropdown: unsupported object type.");
                return;
            }

            Vector2 menuPosition = worldBound.position + new Vector2(0, layout.height);
            menu.DropDown(new Rect(menuPosition, Vector2.zero));
        }

        private void AddComponentMembersToMenu(GenericMenu menu, object component, string typeName)
        {
            var type = component.GetType();

            if (!boolReturnTypesOnly)
            {
                // Add Fields (any type, but only if supported)
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var field in fields)
                {
                    if (!IsMemberSupported(field))
                        continue;

                    string path = $"{typeName}/{field.Name}";
                    menu.AddItem(new GUIContent(path), false, () => OnFieldSelected(field));
                }

                // Add Properties (any type, skip indexers, only if supported)
                var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(p => p.GetIndexParameters().Length == 0);

                foreach (var property in properties)
                {
                    if (!IsMemberSupported(property))
                        continue;

                    string path = $"{typeName}/{property.Name}";
                    menu.AddItem(new GUIContent(path), false, () => OnPropertySelected(property));
                }

                // Add Methods (only if supported)
                var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m =>
                        m.DeclaringType != typeof(MonoBehaviour) &&
                        m.DeclaringType != typeof(ScriptableObject) &&
                        !m.IsSpecialName &&
                        IsMemberSupported(m)
                    );

                foreach (var method in methods)
                {
                    string parameterList = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                    string path = $"{typeName}/{method.Name}({parameterList})";
                    menu.AddItem(new GUIContent(path), false, () => OnMethodSelected(method));
                }
            }
            else
            {
                // Bool Fields Only (with nullable support)
                var boolFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(f => UnwrapNullable(f.FieldType) == typeof(bool));

                foreach (var member in boolFields)
                {
                    string path = $"{typeName}/{member.Name}";
                    menu.AddItem(new GUIContent(path), false, () => OnFieldSelected(member));
                }

                // Bool Properties Only (with nullable support)
                var boolProperties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(p => UnwrapNullable(p.PropertyType) == typeof(bool) && p.GetIndexParameters().Length == 0);

                foreach (var member in boolProperties)
                {
                    string path = $"{typeName}/{member.Name}";
                    menu.AddItem(new GUIContent(path), false, () => OnPropertySelected(member));
                }

                // Bool Methods Only
                var boolMethods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m => UnwrapNullable(m.ReturnType) == typeof(bool) && IsMemberSupported(m));

                foreach (var method in boolMethods)
                {
                    string parameterList = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                    string path = $"{typeName}/{method.Name}({parameterList})";
                    menu.AddItem(new GUIContent(path), false, () => OnMethodSelected(method));
                }
            }
        }

        private bool IsMemberSupported(FieldInfo field)
        {
            var fieldType = UnwrapNullable(field.FieldType);

            if (supportedTypes.Contains(fieldType))
                return true;

            if (fieldType.IsEnum)
                return true;

            if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
                return true;

            return false;
        }

        private bool IsMemberSupported(PropertyInfo property)
        {
            var propertyType = UnwrapNullable(property.PropertyType);

            if (supportedTypes.Contains(propertyType))
                return true;

            if (propertyType.IsEnum)
                return true;

            if (typeof(UnityEngine.Object).IsAssignableFrom(propertyType))
                return true;

            return false;
        }

        private bool IsMemberSupported(MethodInfo method)
        {
            foreach (var param in method.GetParameters())
            {
                var paramType = UnwrapNullable(param.ParameterType);

                if (supportedTypes.Contains(paramType))
                    continue;

                if (paramType.IsEnum)
                    continue;

                if (typeof(UnityEngine.Object).IsAssignableFrom(paramType))
                    continue;

                return false;
            }

            return true;
        }


        private Type UnwrapNullable(Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        private void OnFieldSelected(FieldInfo info)
        {
            text = $"{info.DeclaringType.Name}/{info.Name}";
            onFieldSelected?.Invoke(info.DeclaringType.AssemblyQualifiedName, info);
        }

        private void OnPropertySelected(PropertyInfo info)
        {
            text = $"{info.DeclaringType.Name}/{info.Name}";
            onPropertySelected?.Invoke(info.DeclaringType.AssemblyQualifiedName, info);
        }

        private void OnMethodSelected(MethodInfo info)
        {
            text = $"{info.DeclaringType.Name}/{info.Name}";
            onMethodSelected?.Invoke(info.DeclaringType.AssemblyQualifiedName, info);
        }
    }
}
#endif