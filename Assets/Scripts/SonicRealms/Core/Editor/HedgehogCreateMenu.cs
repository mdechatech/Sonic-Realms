using UnityEditor;
using UnityEngine;

namespace SonicRealms.Core.Editor
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

        private const string PathSwitcherPath = "Assets/Hedgehog/Prefabs/Level/Areas/Path Switcher.prefab";
        private const string PathSwitcherName = "Path Switcher";
        [MenuItem("GameObject/Hedgehog/Areas/Path Switcher", false, 10)]
        private static void CreatePathSwitcher(MenuCommand menuCommand)
        {
            HandleClonePrefab(menuCommand, PathSwitcherPath, PathSwitcherName);
        }

        private const string CurrentPath = "Assets/Hedgehog/Prefabs/Level/Areas/Current.prefab";
        private const string CurrentName = "Current";
        [MenuItem("GameObject/Hedgehog/Areas/Current", false, 10)]
        private static void CreateCurrent(MenuCommand menuCommand)
        {
            HandleClonePrefab(menuCommand, CurrentPath, CurrentName);
        }

        private const string WaterPath = "Assets/Hedgehog/Prefabs/Level/Areas/Water.prefab";
        private const string WaterName = "Water";
        [MenuItem("GameObject/Hedgehog/Areas/Water", false, 10)]
        private static void CreateWater(MenuCommand menuCommand)
        {
            HandleClonePrefab(menuCommand, WaterPath, WaterName);
        }

        private const string BoostPadPath = "Assets/Hedgehog/Prefabs/Level/Objects/Boost Pad.prefab";
        private const string BoostPadName = "Boost Pad";
        [MenuItem("GameObject/Hedgehog/Objects/Boost Pad", false, 10)]
        private static void CreateBoostPad(MenuCommand menuCommand)
        {
            HandleClonePrefab(menuCommand, BoostPadPath, BoostPadName);
        }

        private const string BumperPath = "Assets/Hedgehog/Prefabs/Level/Objects/Bumper.prefab";
        private const string BumperName = "Bumper";
        [MenuItem("GameObject/Hedgehog/Objects/Bumper", false, 10)]
        private static void CreateBumper(MenuCommand menuCommand)
        {
            HandleClonePrefab(menuCommand, BumperPath, BumperName);
        }

        private const string SpringPath = "Assets/Hedgehog/Prefabs/Level/Objects/Spring.prefab";
        private const string SpringName = "Spring";
        [MenuItem("GameObject/Hedgehog/Objects/Spring", false, 10)]
        private static void CreateSpring(MenuCommand menuCommand)
        {
            HandleClonePrefab(menuCommand, SpringPath, SpringName);
        }

        private const string AcceleratorPath = "Assets/Hedgehog/Prefabs/Level/Platforms/Accelerator.prefab";
        private const string AcceleratorName = "Accelerator";
        [MenuItem("GameObject/Hedgehog/Platforms/Accelerator", false, 10)]
        private static void CreateAccelerator(MenuCommand menuCommand)
        {
            HandleClonePrefab(menuCommand, AcceleratorPath, AcceleratorName);
        }

        private const string ConveyorBeltPath = "Assets/Hedgehog/Prefabs/Level/Platforms/Conveyor Belt.prefab";
        private const string ConveyorBeltName = "Conveyor Belt";
        [MenuItem("GameObject/Hedgehog/Platforms/Conveyor Belt", false, 10)]
        private static void CreateConveyorBelt(MenuCommand menuCommand)
        {
            HandleClonePrefab(menuCommand, ConveyorBeltPath, ConveyorBeltName);
        }

        private const string GravityMagnetPath = "Assets/Hedgehog/Prefabs/Level/Platforms/Gravity Magnet.prefab";
        private const string GravityMagnetName = "Gravity Magnet";
        [MenuItem("GameObject/Hedgehog/Platforms/Gravity Magnet", false, 10)]
        private static void CreateGravityMagnet(MenuCommand menuCommand)
        {
            HandleClonePrefab(menuCommand, GravityMagnetPath, GravityMagnetName);
        }

        private const string IcePath = "Assets/Hedgehog/Prefabs/Level/Platforms/Ice.prefab";
        private const string IceName = "Ice";
        [MenuItem("GameObject/Hedgehog/Platforms/Ice", false, 10)]
        private static void CreateIce(MenuCommand menuCommand)
        {
            HandleClonePrefab(menuCommand, IcePath, IceName);
        }

        private const string LedgePath = "Assets/Hedgehog/Prefabs/Level/Platforms/Ledge.prefab";
        private const string LedgeName = "Ledge";
        [MenuItem("GameObject/Hedgehog/Platforms/Ledge", false, 10)]
        private static void CreateLedge(MenuCommand menuCommand)
        {
            HandleClonePrefab(menuCommand, LedgePath, LedgeName);
        }

        private const string PathPlatformPath = "Assets/Hedgehog/Prefabs/Level/Platforms/Path Platform.prefab";
        private const string PathPlatformName = "Path Platform";
        [MenuItem("GameObject/Hedgehog/Platforms/Path Platform", false, 10)]
        private static void CreatePathPlatform(MenuCommand menuCommand)
        {
            HandleClonePrefab(menuCommand, PathPlatformPath, PathPlatformName);
        }

        private const string SwingPlatformPath = "Assets/Hedgehog/Prefabs/Level/Platforms/Swing Platform.prefab";
        private const string SwingPlatformName = "Swing Platform";
        [MenuItem("GameObject/Hedgehog/Platforms/Swing Platform", false, 10)]
        private static void CreateSwingPlatform(MenuCommand menuCommand)
        {
            HandleClonePrefab(menuCommand, SwingPlatformPath, SwingPlatformName);
        }

        private const string WeightedPlatformPath = "Assets/Hedgehog/Prefabs/Level/Platforms/Weighted Platform.prefab";
        private const string WeightedPlatformName = "Weighted Platform";
        [MenuItem("GameObject/Hedgehog/Platforms/Weighted Platform", false, 10)]
        private static void CreateWeightedPlatform(MenuCommand menuCommand)
        {
            HandleClonePrefab(menuCommand, WeightedPlatformPath, WeightedPlatformName);
        }
    }
}
