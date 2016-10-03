using System;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace UnityToolbag
{
    [CustomEditor(typeof(SortingLayerExposed))]
    public class SortingLayerExposedEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Get the renderer from the target object
            var renderer = (target as SortingLayerExposed).gameObject.GetComponent<Renderer>();

            // If there is no renderer, we can't do anything
            if (!renderer) {
                EditorGUILayout.HelpBox("SortingLayerExposed must be added to a game object that has a renderer.", MessageType.Error);
                return;
            }

            var sortingLayerNames = SortingLayer.layers.Select(l => l.name).ToArray();

            // Look up the layer name using the current layer ID
            string oldName = SortingLayer.IDToName(renderer.sortingLayerID);

            // Use the name to look up our array index into the names list
            int oldLayerIndex = Array.IndexOf(sortingLayerNames, oldName);

            // Show the popup for the names
            int newLayerIndex = EditorGUILayout.Popup("Sorting Layer", oldLayerIndex, sortingLayerNames);

            // If the index changes, look up the ID for the new index to store as the new ID
            if (newLayerIndex != oldLayerIndex) {
                Undo.RecordObject(renderer, "Edit Sorting Layer");
                renderer.sortingLayerID = SortingLayer.NameToID(sortingLayerNames[newLayerIndex]);
                EditorUtility.SetDirty(renderer);
            }

            // Expose the manual sorting order
            int newSortingLayerOrder = EditorGUILayout.IntField("Sorting Layer Order", renderer.sortingOrder);
            if (newSortingLayerOrder != renderer.sortingOrder) {
                Undo.RecordObject(renderer, "Edit Sorting Order");
                renderer.sortingOrder = newSortingLayerOrder;
                EditorUtility.SetDirty(renderer);
            }
        }
    }
}
