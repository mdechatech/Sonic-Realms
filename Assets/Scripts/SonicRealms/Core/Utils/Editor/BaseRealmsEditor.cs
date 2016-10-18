using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SonicRealms.Core.Utils.Editor
{
    /// <summary>
    /// Allows easy creation of custom editors using special attributes such as <see cref="FoldoutAttribute"/>
    /// and overriden methods such as <see cref="DrawProperty"/>.
    /// </summary>
    [CanEditMultipleObjects]
    public abstract class BaseRealmsEditor : UnityEditor.Editor
    {
        protected static readonly IComparer<PropertyData> DefaultPropertyComparer = new IdentityPropertyComparer();
        protected static readonly IComparer<FoldoutData> DefaultFoldoutComparer = new IdentityFoldoutComparer();

        protected virtual IComparer<PropertyData> PropertyComparer { get { return DefaultPropertyComparer; } }
        protected virtual IComparer<FoldoutData> FoldoutComparer { get { return DefaultFoldoutComparer; } }

        protected List<KeyValuePair<FoldoutData, List<PropertyData>>> FoldoutProperties;
        protected List<PropertyData> UnmarkedProperties;

        protected virtual void OnEnable()
        {
            // Get comparers
            var foldoutComparer = FoldoutComparer;
            var propertyComparer = PropertyComparer;
            
            // Get and sort properties on the object
            List<PropertyData> unmarkedProperties;
            var foldoutProperties = GatherProperties(out unmarkedProperties);

            unmarkedProperties.Sort(propertyComparer);

            foreach (var pair in foldoutProperties)
            {
                pair.Key.PropertyCount = pair.Value.Count;

                pair.Value.Sort(propertyComparer);

                for (var i = 0; i < pair.Value.Count; ++i)
                {
                    var propertyData = pair.Value[i];

                    propertyData.FoldoutOrder = i;
                    propertyData.Foldout = pair.Key;
                }
            }

            var sortedFoldouts = foldoutProperties.Keys.ToArray();
            Array.Sort(sortedFoldouts, foldoutComparer);

            UnmarkedProperties = unmarkedProperties;
            FoldoutProperties = sortedFoldouts.Select(
                foldout => new KeyValuePair<FoldoutData, List<PropertyData>>(foldout, foldoutProperties[foldout]))
                .ToList();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw unmarked properties
            foreach (var property in UnmarkedProperties)
            {
                DoDrawProperty(property);
            }

            // Draw foldouted properties
            foreach (var pair in FoldoutProperties)
            {
                var editorPrefsKey = GetFoldoutEditorPrefsKey(pair.Key);

                var state = EditorPrefs.GetBool(editorPrefsKey, false);
                state = DrawFoldout(state, pair.Key);
                
                EditorPrefs.SetBool(editorPrefsKey, state);

                if (state)
                {
                    foreach (var property in pair.Value)
                    {
                        DoDrawProperty(property);

                        if (property.IsLastInFoldout)
                        {
                            DrawAtFoldoutBottom(pair.Key);
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual string GetFoldoutEditorPrefsKey(FoldoutData foldout)
        {
            return string.Format("{0}.Show{1}",
                serializedObject.targetObject.GetType().Name,
                foldout.Name);
        }

        protected void AddFoldout(string foldoutName)
        {
            AddFoldout(new FoldoutData(0, default(int?), 0, foldoutName));
        }

        protected void AddFoldout(FoldoutData foldout)
        {
            FoldoutProperties.Add(new KeyValuePair<FoldoutData, List<PropertyData>>(foldout, new List<PropertyData>()));
            FoldoutProperties.Sort((a, b) => FoldoutComparer.Compare(a.Key, b.Key));

            for (var i = 0; i < FoldoutProperties.Count; ++i)
                FoldoutProperties[i].Key.DeclarationOrder = i;
        }

        protected virtual bool DrawFoldout(bool state, FoldoutData foldout)
        {
            state = EditorGUILayout.Foldout(state, foldout.Name);

            if (state)
                DrawAtFoldoutTop(foldout);

            return state;
        }

        protected virtual void DrawAtFoldoutTop(FoldoutData foldout)
        {
            EditorGUILayout.Separator();
        }

        protected virtual void DrawAtFoldoutBottom(FoldoutData foldout)
        {
            EditorGUILayout.Separator();
        }

        protected virtual void DrawProperty(PropertyData property, GUIContent label)
        {
            EditorGUILayout.PropertyField(property.Property, label, true);
        }

        private void DoDrawProperty(PropertyData property)
        {
            var content = new GUIContent(property.Property.displayName);

            var tooltip = RealmsEditorUtility.GetAttribute<TooltipAttribute>(property.Property);
            if (tooltip != null)
                content.tooltip = tooltip.tooltip;

            DrawProperty(property, content);
        }

        protected Dictionary<FoldoutData, List<PropertyData>> GatherProperties(out List<PropertyData> unmarkedProperties)
        {
            var results = new Dictionary<FoldoutData, List<PropertyData>>();
            var unmarked = new List<PropertyData>();

            var it = serializedObject.GetIterator();
            it.Next(true);

            var foldoutDeclOrder = 0;
            var propertyDeclOrder = 0;

            while (it.NextVisible(false))
            {
                if (it.name == RealmsEditorUtility.ScriptPropertyName)
                    continue;

                var attr = RealmsEditorUtility.GetAttribute<FoldoutAttribute>(it);

                if (attr == null)
                {
                    unmarked.Add(new PropertyData(it.Copy(), null, 0, propertyDeclOrder++, null));
                    continue;
                }

                var foldoutName = attr.Name;

                var data = new FoldoutData(foldoutDeclOrder++, attr.Order, 0, foldoutName);

                (results.ContainsKey(data) ? results[data] : (results[data] = new List<PropertyData>()))
                    .Add(new PropertyData(it.Copy(), data, 0, propertyDeclOrder++, null));
            }

            unmarkedProperties = unmarked.ToList();
            return results.ToDictionary(pair => pair.Key, pair => pair.Value.ToList());
        }

        protected class IdentityPropertyComparer : IComparer<PropertyData>
        {
            public int Compare(PropertyData x, PropertyData y)
            {
                if (x.Order.HasValue && y.Order.HasValue)
                {
                    return x.Order.Value - y.Order.Value;
                }

                if (!x.Order.HasValue && y.Order.HasValue)
                {
                    return 1;
                }

                if (x.Order.HasValue && !y.Order.HasValue)
                {
                    return -1;
                }

                return x.DeclarationOrder - y.DeclarationOrder;
            }
        }

        protected class IdentityFoldoutComparer : IComparer<FoldoutData>
        {
            public int Compare(FoldoutData x, FoldoutData y)
            {
                if (x.Order.HasValue && y.Order.HasValue)
                {
                    return x.Order.Value - y.Order.Value;
                }

                if (!x.Order.HasValue && y.Order.HasValue)
                {
                    return 1;
                }

                if (x.Order.HasValue && !y.Order.HasValue)
                {
                    return -1;
                }

                return x.DeclarationOrder - y.DeclarationOrder;
            }
        }

        public class PropertyData
        {
            public SerializedProperty Property;
            public FoldoutData Foldout;
            public int FoldoutOrder;
            public int DeclarationOrder;
            public int? Order;

            public string Name { get { return Property.name; } }

            public bool IsFirstInFoldout
            {
                get { return Foldout != null && FoldoutOrder == 0; }
            }

            public bool IsLastInFoldout
            {
                get { return Foldout != null && FoldoutOrder == Foldout.PropertyCount - 1; }
            }

            public PropertyData(SerializedProperty property, FoldoutData foldout, int foldoutOrder, int declarationOrder,
                int? order)
            {
                Property = property;
                Foldout = foldout;
                FoldoutOrder = foldoutOrder;
                DeclarationOrder = declarationOrder;
                Order = order;
            }
        }

        public class FoldoutData : IEquatable<FoldoutData>
        {
            public int DeclarationOrder;
            public int? Order;
            public int PropertyCount;
            public string Name;

            public FoldoutData(int declarationOrder, int? order, int propertyCount, string name)
            {
                DeclarationOrder = declarationOrder;
                Order = order;
                PropertyCount = propertyCount;
                Name = name;
            }

            public bool Equals(FoldoutData other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return string.Equals(Name, other.Name);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((FoldoutData) obj);
            }

            public override int GetHashCode()
            {
                return (Name != null ? Name.GetHashCode() : 0);
            }

            public static bool operator ==(FoldoutData left, FoldoutData right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(FoldoutData left, FoldoutData right)
            {
                return !Equals(left, right);
            }
        }
    }
}
