using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SonicRealms.Core.Utils.Editor
{
    //[CustomPropertyDrawer(typeof (FoldoutAttribute), true)]
    public class FoldoutDrawer : PropertyDrawer
    {
        public const float FoldoutHeight = 16f;
        public const float SpaceHeight = 2f;

        public static Dictionary<string, List<SerializedProperty>> GatherProperties(
            SerializedObject serializedObject, string defaultFoldoutName = "Misc")
        {
            var results = new Dictionary<string, List<SerializedProperty>>();

            var it = serializedObject.GetIterator(); it.Next(true);
            while (it.NextVisible(false))
            {
                if(it.name == HedgehogEditorGUIUtility.ScriptPropertyName) continue;

                var attr = HedgehogEditorGUIUtility.GetAttribute<FoldoutAttribute>(it);
                var type = attr == null ? defaultFoldoutName : attr.Name;

                (results.ContainsKey(type) ? results[type] : (results[type] = new List<SerializedProperty>()))
                    .Add(it.Copy());
            }

            foreach (var field in serializedObject.targetObject.GetType().GetFields(
                BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var property = serializedObject.FindProperty(field.Name);
                if (property == null) continue;
                if (HedgehogEditorGUIUtility.GetAttribute<HideInInspector>(field) != null) continue;

                foreach(var propertyLists in results.Values)
                    if (propertyLists.Contains(property)) continue;

                var attr = HedgehogEditorGUIUtility.GetAttribute<FoldoutAttribute>(field);
                var type = attr == null ? defaultFoldoutName : attr.Name;

                (results.ContainsKey(type) ? results[type] : (results[type] = new List<SerializedProperty>()))
                    .Add(property);
            }

            return results;
        }

        public static void DoFoldoutsLayout(SerializedObject serializedObject, string noFoldoutName = "Misc")
        {
            serializedObject.Update();
            var data = GatherProperties(serializedObject, noFoldoutName);

            List<SerializedProperty> noFoldout;
            if (data.TryGetValue(noFoldoutName, out noFoldout))
            {
                data.Remove(noFoldoutName);
                HedgehogEditorGUIUtility.DrawProperties(noFoldout.ToArray());
                serializedObject.ApplyModifiedProperties();
            }

            DoFoldoutsLayout(data);
        }

        public static void DoFoldoutsLayout(Dictionary<string, List<SerializedProperty>> data)
        {
            var first = data.Values.FirstOrDefault();
            if (first == null || first.Count == 0) return;

            var serializedObject = first.First().serializedObject;
            serializedObject.Update();

            var objectName = serializedObject.targetObject.GetType().Name;

            foreach (var foldoutName in data.Keys)
            {
                var entryName = objectName + ".Show" + foldoutName;

                var state = EditorPrefs.GetBool(entryName, false);
                state = EditorGUILayout.Foldout(state, foldoutName);
                if (state)
                {
                    HedgehogEditorGUIUtility.DrawProperties(data[foldoutName].ToArray());
                }

                EditorPrefs.SetBool(entryName, state);
            }

            serializedObject.ApplyModifiedProperties();
        }

        // Seems impossible to use the default property drawer from a custom one, causing
        // the drawer to break on events and other things
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var entryName = GetEntryName(property);
            var state = EditorPrefs.GetBool(entryName);

            var height = base.GetPropertyHeight(property, label);
            var name = (attribute as FoldoutAttribute).Name;
            var first = IsFirst(property, name);
            if (first)
            {
                state = EditorGUI.Foldout(new Rect(new Vector2(position.position.x, position.y),
                    new Vector2(position.width, FoldoutHeight)), state, name);
                EditorPrefs.SetBool(entryName, state);

                if (state)
                {
                    EditorGUI.PropertyField(new Rect(new Vector2(position.position.x, position.y + height + SpaceHeight),
                        new Vector2(position.width, height)), property, label, true);
                }
            }
            else if(state)
            {
                EditorGUI.PropertyField(new Rect(position.position, new Vector2(position.width, height)), property,
                    label, true);
            }
        }

        private string GetEntryName(SerializedProperty property)
        {
            return property.serializedObject.targetObject.GetType().Name + ".Show" + 
                    (attribute as FoldoutAttribute).Name;
        }

        private bool IsFirst(SerializedProperty property, string foldoutName)
        {
            var it = property.serializedObject.GetIterator(); it.Next(true);

            while (it.NextVisible(true))
            {
                if (it.name == property.name)
                    break;

                var attr = HedgehogEditorGUIUtility.GetAttribute<FoldoutAttribute>(it);
                if (attr != null && attr.Name == foldoutName) return false;
            }

            return true;
        }

        

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (EditorPrefs.GetBool(GetEntryName(property), false))
            {
                if (IsFirst(property, (attribute as FoldoutAttribute).Name))
                    return FoldoutHeight + SpaceHeight + base.GetPropertyHeight(property, label);
                else
                    return FoldoutHeight;
            }
            else
            {
                if (IsFirst(property, (attribute as FoldoutAttribute).Name))
                    return FoldoutHeight;
                else
                    // Counteract the spacing unity automatically puts between property. Can't believe this works LOL
                    return -SpaceHeight;
            }
        }
    }
}
