using Hedgehog.Actors;
using Hedgehog.Utils;
using UnityEditor;
using UnityEngine;

namespace Hedgehog.Editor
{
    static class HedgehogCreateMenu
    {
        private static void HandleCreateContext(MenuCommand menuCommand, GameObject createdObject)
        {
            GameObjectUtility.SetParentAndAlign(createdObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(createdObject, "Create " + createdObject.name);
            Selection.activeObject = createdObject;
        }

        private static void HandleClonePrefab(MenuCommand menuCommand, string path, string name)
        {
            var clonedPrefab = GameObject.Instantiate(
                AssetDatabase.LoadAssetAtPath<GameObject>(path));
            clonedPrefab.name = name;
            HandleCreateContext(menuCommand, clonedPrefab);
        }

        private const string DefaultActorPath = "Assets/Hedgehog/Prefabs/Actors/Default.prefab";
        private const string DefaultActorName = "Hedgehog";
        [MenuItem("GameObject/Hedgehog/Actors/Default", false, 10)]
        private static void CreateDefaultController(MenuCommand menuCommand)
        {
            HandleClonePrefab(menuCommand, DefaultActorPath, DefaultActorName);
        }

        private const string PathSwitcherPath = "Assets/Hedgehog/Prefabs/Special/Path Switcher.prefab";
        private const string PathSwitcherName = "Path Switcher";
        [MenuItem("GameObject/Hedgehog/Special/Path Switcher", false, 10)]
        private static void CreatePathSwitcher(MenuCommand menuCommand)
        {
            HandleClonePrefab(menuCommand, PathSwitcherPath, PathSwitcherName);
        }
    }
}
