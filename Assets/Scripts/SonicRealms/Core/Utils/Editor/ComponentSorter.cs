using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SonicRealms.Core.Utils.Editor
{
    [InitializeOnLoad]
    public class ComponentSorter
    {
        private const string AutoSortComponentsMenu = "Realms/Auto Sort Components";

        protected static bool AutoSortComponents
        {
            get { return EditorPrefs.GetBool("AutoSortComponents", true); }
            set { EditorPrefs.SetBool("AutoSortComponents", value); }
        }

        static ComponentSorter()
        {
            EditorApplication.delayCall += () =>
            {
                Menu.SetChecked(AutoSortComponentsMenu, AutoSortComponents);

                EditorScheduler.Repeat(CheckSort, 2);
            };
        }

        public static void CheckSort()
        {
            if (!AutoSortComponents)
                return;
            
            if (Selection.activeTransform == null || Selection.transforms.Length > 1)
                return;

            SortComponents(Selection.activeTransform.gameObject);
        }

        public static void SortComponents(GameObject gameObject)
        {
            var components = gameObject.GetComponents<Component>();

            if (components.Length <= 2)
                return;

            // We'll use gnome sort because it's inplace, covers sorted lists in O(n), limits itself
            // to swapping adjacent items in the list, and only savages use bubble sort
            var index = 1;
            var t = 0;
            while (index < components.Length && ++t < 200)
            {
                var current = components[index - 1];
                var currentComparable = current as IComparable<Component>;

                var next = components[index];
                var nextComparable = next as IComparable<Component>;

                int compareResult;

                if (nextComparable != null)
                    compareResult = -nextComparable.CompareTo(current);
                else if (currentComparable != null)
                    compareResult = currentComparable.CompareTo(next);
                else
                    compareResult = 0;

                if (index == 1 || compareResult <= 0)
                {
                    ++index;
                }
                else
                {
                    Swap(components, index, index - 1);
                    ComponentUtility.MoveComponentUp(next);
                    --index;
                }

                if (t == 199)
                {
                    Debug.Log(string.Format("Auto Sort Components hit a snag comparing {0} to {1}. The option " +
                                            "will now disable itself.", current.GetType().Name, next.GetType().Name));
                    AutoSortComponents = false;
                    Menu.SetChecked(AutoSortComponentsMenu, false);
                }
            }
        }

        private static void Swap(Component[] components, int a, int b)
        {
            var temp = components[a];
            components[a] = components[b];
            components[b] = temp;
        }

        [MenuItem(AutoSortComponentsMenu, false, 51)]
        public static void AutoSortComponentsMenuItem()
        {
            AutoSortComponents = !AutoSortComponents;
            Menu.SetChecked(AutoSortComponentsMenu, AutoSortComponents);
        }
    }
}
