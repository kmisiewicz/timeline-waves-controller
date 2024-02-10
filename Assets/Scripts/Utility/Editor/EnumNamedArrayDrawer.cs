using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Chroma.Utility
{
    // https://stackoverflow.com/questions/55583071/see-enumerated-indices-of-array-in-unity-inspector
    // https://stackoverflow.com/questions/24892935/custom-property-drawers-for-generic-classes-c-sharp-unity

    [CustomPropertyDrawer(typeof(EnumNamedArray), true)]
    public class EnumNamedArrayDrawer : PropertyDrawer
    {
        bool showElements = false;

        const int INDENT_WIDTH = 15;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var names = property.FindPropertyRelative("Names");
            var values = property.FindPropertyRelative("Values");

            showElements = EditorGUI.Foldout(new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight),
                showElements, new GUIContent(label.text + $" ({values.arraySize})", property.tooltip), true);

            GUIContent buttonContent = new GUIContent("Refresh", "Rebuild array if enum was changed while keeping compatible values");
            if (GUI.Button(new Rect(position.x + EditorGUIUtility.labelWidth + 2, position.y,
                Mathf.Min(position.width - EditorGUIUtility.labelWidth - 2, EditorStyles.label.CalcSize(buttonContent).x + 20), 
                EditorGUIUtility.singleLineHeight), buttonContent))
            {
                System.Object obj = property.serializedObject.targetObject;
                var type = obj.GetType();
                var fieldInfo = type.GetField(property.name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (fieldInfo == null)
                {
                    Debug.LogError($"Could not find field: {property.name}.");
                    return;
                }
                obj = fieldInfo.GetValue(obj);
                type = fieldInfo.FieldType;
                fieldInfo.SetValue(property.serializedObject.targetObject, Activator.CreateInstance(type, obj));
            }

            if (showElements)
            {
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.indentLevel++;

                float singleHeight = values.arraySize > 0 ? EditorGUI.GetPropertyHeight(values.GetArrayElementAtIndex(0)) : 0;

                for (var i = 0; i < values.arraySize; i++)
                {
                    var name = names.GetArrayElementAtIndex(i);
                    var value = values.GetArrayElementAtIndex(i);

                    if (singleHeight < EditorGUIUtility.singleLineHeight + 1)
                    {
                        var indentedRect = EditorGUI.IndentedRect(position);
                        indentedRect.x -= INDENT_WIDTH * EditorGUI.indentLevel;
                        indentedRect = EditorGUI.PrefixLabel(indentedRect, new GUIContent(SplitCamelCase(name.stringValue)));

                        indentedRect.x -= INDENT_WIDTH * EditorGUI.indentLevel;
                        indentedRect.width += INDENT_WIDTH * (EditorGUI.indentLevel * 2);
                        EditorGUI.PropertyField(indentedRect, value, GUIContent.none, true);
                    }
                    else
                    {
                        EditorGUI.PropertyField(position, value, new GUIContent(SplitCamelCase(name.stringValue)), true);
                    }
                    position.y += singleHeight + EditorGUIUtility.standardVerticalSpacing;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var values = property.FindPropertyRelative("Values");
            float elementHeight = values.arraySize > 0 ? EditorGUI.GetPropertyHeight(values.GetArrayElementAtIndex(0)) + EditorGUIUtility.standardVerticalSpacing : 0;
            return showElements ? (values.arraySize) * elementHeight + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing : EditorGUIUtility.singleLineHeight;
        }

        private string SplitCamelCase(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
        }
    }
}
