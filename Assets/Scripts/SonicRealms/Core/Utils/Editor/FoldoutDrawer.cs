using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SonicRealms.Core.Utils.Editor
{
    public class FoldoutDrawer
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
                if(it.name == RealmsEditorUtility.ScriptPropertyName) continue;

                var attr = RealmsEditorUtility.GetAttribute<FoldoutAttribute>(it);
                var type = attr == null ? defaultFoldoutName : attr.Name;

                (results.ContainsKey(type) ? results[type] : (results[type] = new List<SerializedProperty>()))
                    .Add(it.Copy());
            }

            return results;
        }

        public static void DoFoldoutsLayout(SerializedObject serializedObject, string noFoldoutName = "Misc")
        {
            var data = GatherProperties(serializedObject, noFoldoutName);

            List<SerializedProperty> noFoldout;
            if (data.TryGetValue(noFoldoutName, out noFoldout))
            {
                data.Remove(noFoldoutName);
                RealmsEditorUtility.DrawProperties(noFoldout.ToArray());
            }

            DoFoldoutsLayout(data);
        }

        public static void DoFoldoutsLayout(Dictionary<string, List<SerializedProperty>> data)
        {
            var first = data.Values.FirstOrDefault();
            if (first == null || first.Count == 0) return;

            var serializedObject = first.First().serializedObject;

            var objectName = serializedObject.targetObject.GetType().Name;

            foreach (var foldoutName in data.Keys)
            {
                var entryName = objectName + ".Show" + foldoutName;

                var state = EditorPrefs.GetBool(entryName, false);
                state = EditorGUILayout.Foldout(state, foldoutName);
                if (state)
                {
                    RealmsEditorUtility.DrawProperties(data[foldoutName].ToArray());
                }

                EditorPrefs.SetBool(entryName, state);
            }
        }
    }
}
