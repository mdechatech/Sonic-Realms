using SonicRealms.Core.Utils.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SonicRealms.UI.Editor
{
    [CustomEditor(typeof(UiStateManager))]
    public class UiStateHandlerEditor : BaseRealmsEditor
    {
        private UiStateManager _instance;

        private ReorderableList _transitionList;
        private SerializedProperty _transitionsProp;

        protected override void OnEnable()
        {
            base.OnEnable();

            _instance = (UiStateManager) target;

            _transitionsProp = serializedObject.FindProperty("_transitions");
            CreateTransitionList();

            AddFoldout("Debug");
        }

        protected override void DrawInFoldoutStart(FoldoutData foldout)
        {
            if (foldout.Name == "Debug")
            {
                DrawDebugFoldout();
            }
        }

        protected override void DrawProperty(PropertyData property, GUIContent label)
        {
            if (property.Name == "_transitions")
            {
                EditorGUILayout.Space();
                _transitionList.DoLayoutList();
            }
            else
            {
                base.DrawProperty(property, label);
            }
        }

        private void DrawDebugFoldout()
        {
            GUI.enabled = Application.isPlaying;

            EditorGUILayout.TextField("Current State", _instance.CurrentState);
            EditorGUILayout.TextField("Current Transition", _instance.IsInTransition
                ? string.Format("{0} -> {1}", _instance.TransitionFromState, _instance.TransitionToState)
                : "None");

            GUI.enabled = true;
        }

        private void CreateTransitionList()
        {
            _transitionList = new ReorderableList(serializedObject, _transitionsProp);

            _transitionList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Transitions");

            _transitionList.elementHeight = UiStateTransitionDrawer.PropertyHeight;

            _transitionList.drawElementCallback = (rect, index, active, focused) =>
            {
                EditorGUI.PropertyField(rect, _transitionsProp.GetArrayElementAtIndex(index));
            };
        }
    }
}
