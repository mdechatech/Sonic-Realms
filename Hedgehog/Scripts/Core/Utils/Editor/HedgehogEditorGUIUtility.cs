using System.Collections.Generic;
using System.Linq;
using Hedgehog.Core.Actors;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;

namespace Hedgehog.Core.Utils.Editor
{
    public class HedgehogEditorGUIUtility
    {
        /// <summary>
        /// Just a quick way to draw property fields.
        /// </summary>
        /// <param name="serializedObject"></param>
        public static void DrawProperties(SerializedObject serializedObject)
        {
            DrawExcluding(serializedObject);
        }

        /// <summary>
        /// Just a quick way to draw property fields.
        /// </summary>
        /// <param name="serializedObject"></param>
        /// <param name="properties"></param>
        public static void DrawProperties(SerializedObject serializedObject, params string[] properties)
        {
            foreach (var property in properties)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(property), true);
            }
        }

        /// <summary>
        /// Just a quick way to draw property fields.
        /// </summary>
        /// <param name="serializedObject"></param>
        /// <param name="excludingProperties"></param>
        public static void DrawExcluding(SerializedObject serializedObject, params string[] excludingProperties)
        {
            var it = serializedObject.GetIterator(); it.Next(true);
            while(it.NextVisible(false))
            {
                if (!excludingProperties.Contains(it.name))
                {
                    EditorGUILayout.PropertyField(it, true);
                }
            }
        }

        // Source: http://answers.unity3d.com/questions/42996/how-to-create-layermask-field-in-a-custom-editorwi.html
        public static LayerMask LayerMaskField(string label, LayerMask layerMask)
        {
            List<string> layers = new List<string>();
            List<int> layerNumbers = new List<int>();

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (layerName != "")
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
            }
            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                    maskWithoutEmpty |= (1 << i);
            }
            maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers.ToArray());
            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= (1 << layerNumbers[i]);
            }
            layerMask.value = mask;
            return layerMask;
        }

        /// <summary>
        /// There is no return value - any changes to this field in the editor are immediately applied to the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="label"></param>
        /// <param name="serializedObject"></param>
        /// <param name="elements"></param>
        public static void ReorderableListField(string label, SerializedObject serializedObject,
            SerializedProperty elements)
        {
            var list = new ReorderableList(serializedObject, elements, false, false, false, false);

            //list.drawHeaderCallback += rect => GUI.Label(rect, label);
            list.drawElementCallback += (rect, index, active, focused) =>
            {
                rect.height = 16;
                rect.y += 2;
                EditorGUI.PropertyField(rect,
                    list.serializedProperty.GetArrayElementAtIndex(index),
                    GUIContent.none);
            };

            // Issue: The remove button is always disabled for unknown reasons.
            // Workaround: Lets make our own!

            list.onAddCallback += reorderableList => ReorderableList.defaultBehaviours.DoAddButton(list);
            list.onRemoveCallback += reorderableList => elements.DeleteArrayElementAtIndex(list.count - 1);
            list.onCanRemoveCallback += reorderableList => list.count > 0;

            serializedObject.Update();

            var headerStyle = EditorStyles.miniLabel;
            headerStyle.alignment = TextAnchor.MiddleCenter;

            var buttonStyle = EditorStyles.miniButton;
            var disabledButtonStyle = new GUIStyle(buttonStyle) {normal = buttonStyle.active};

            EditorGUILayout.LabelField(label, headerStyle);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add", buttonStyle))
            {
                list.onAddCallback(list);
            }

            if (GUILayout.Button("Remove Last", list.onCanRemoveCallback(list) ? buttonStyle : disabledButtonStyle))
            {
                if (list.onCanRemoveCallback(list)) list.onRemoveCallback(list);
            }
            EditorGUILayout.EndHorizontal();

            list.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        public static void UnityEventField(UnityEvent unityEvent, SerializedProperty serializedEvent)
        {
            var lastRect = GUILayoutUtility.GetLastRect();
            lastRect.y += 20;
            
            EditorGUI.PropertyField(lastRect, serializedEvent, true);

            GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none,
                GUILayout.Height(90 +
                                 Mathf.Max(0, unityEvent.GetPersistentEventCount() - 1) * 42.5f));
        }
    }
}
