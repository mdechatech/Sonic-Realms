using System;
using System.Linq;
using SonicRealms.Core.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace SonicRealms.Core.Moves.Editor
{
    [CustomEditor(typeof(MoveManager))]
    public class MoveManagerEditor : UnityEditor.Editor
    {
        protected bool ShowEventsFoldout
        {
            get { return EditorPrefs.GetBool("MoveManagerEditor.ShowEventsFoldout", false); }
            set { EditorPrefs.SetBool("MoveManagerEditor.ShowEventsFoldout", value); }
        }

        protected bool ShowDebugFoldout
        {
            get { return EditorPrefs.GetBool("MoveManagerEditor.ShowDebugFoldout", false); }
            set { EditorPrefs.SetBool("MoveManagerEditor.ShowDebugFoldout", value); }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var moveManager = target as MoveManager;

            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                "Controller", "Moves");
            
            ShowEventsFoldout = EditorGUILayout.Foldout(ShowEventsFoldout, "Events");
            if (ShowEventsFoldout)
            {
                HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                    "OnPerform", "OnEnd", "OnAvailable", "OnUnavailable", "OnInterrupted");
            }

            GUI.enabled = Application.isPlaying;
            if ((ShowDebugFoldout = EditorGUILayout.Foldout(ShowDebugFoldout, "Debug")) && Application.isPlaying)
            {
                foreach (var layer in moveManager.Layers.Keys)
                {
                    EditorGUILayout.LabelField(Enum.GetName(typeof (MoveLayer), layer),
                        new GUIStyle(EditorStyles.boldLabel) {alignment = TextAnchor.UpperCenter});

                    var active = moveManager.Moves.Where(move => move.Layer == layer &&
                                                                 move.CurrentState == Move.State.Active);
                    var available = moveManager.Moves.Where(move => move.Layer == layer &&
                                                                    move.CurrentState == Move.State.Available);

                    // !WARNING! Very messy one-liner ahead. Quickly whipped it up to see move states from the manager.
                    if (active.Any() || available.Any())
                    {
                        EditorGUILayout.TextArea(

                        (active.Any() ?
                        ("<color=#006400>" +
                        string.Join("\n", active.Select(move => move.GetType().Name +
                                                new string(' ', Mathf.Max(1, 23 - move.GetType().Name.Length)) +
                                                "\tActive").ToArray()) +
                        "</color>" + (available.Any() ? "\n" : "")) : "") +

                        (available.Any() ?
                        ("<color=#646400>" +
                        string.Join("\n", available.Select(move => move.GetType().Name +
                                                new string(' ', Mathf.Max(1, 20 - move.GetType().Name.Length)) +
                                                "\tAvailable")
                                .ToArray()) +
                        "</color>") : ""),

                        new GUIStyle { alignment = TextAnchor.UpperCenter });
                    }
                }
            }
            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
