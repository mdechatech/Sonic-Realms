using System.Linq;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Utils;
using SonicRealms.Core.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace SonicRealms.Level.Objects.Editor
{
    [CustomEditor(typeof(Spring))]
    [CanEditMultipleObjects]
    public class SpringEditor : UnityEditor.Editor
    {
        private static readonly Color32 OnFillColor = new Color32(153, 255, 204, 64);
        private static readonly Color32 OnOutlineColor = new Color32(153, 255, 204, 64);
        private static readonly Color32 OffFillColor = new Color32(255, 102, 153, 64);
        private static readonly Color32 OffOutlineColor = new Color32(255, 102, 153, 64);

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "HitTrigger");

            if(targets.Count() <= 1)
                HedgehogEditorGUIUtility.DrawProperties(serializedObject, "BouncySides");

            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "Power", "AccurateBounce", "LockControl");

            if (targets.Count() == 1)
            {
                var lockControlProp = serializedObject.FindProperty("LockControl");
                if (lockControlProp.boolValue)
                {
                    HedgehogEditorGUIUtility.DrawProperties(serializedObject, "LockDuration");
                }
            }

            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "KeepOnGround");

            serializedObject.ApplyModifiedProperties();
        }

        public void OnSceneGUI()
        {
            DrawHandle(target as Spring);
        }

        private void DrawHandle(Spring spring)
        {
            if (spring == null) return;
            var box = spring.GetComponent<BoxCollider2D>();

            float originalRotation = box.transform.eulerAngles.z;

            box.transform.eulerAngles = new Vector3(
                box.transform.eulerAngles.x, box.transform.eulerAngles.y, 0.0f);

            Handles.DrawSolidRectangleWithOutline(new[]
            {
                new Vector3(box.bounds.min.x, box.bounds.max.y),
                box.bounds.min,
                new Vector3(box.bounds.max.x, box.bounds.min.y),
                box.bounds.max,
            }.Select(vector3 =>
                (Vector3) DMath.RotateBy(vector3, originalRotation*Mathf.Deg2Rad, box.transform.position))
                .ToArray(),
                OnFillColor, OnOutlineColor);

            var hitBounds = box.bounds;
            var offsetX = box.bounds.size.x*0.2f;
            var offsetY = box.bounds.size.y*0.2f;
            if (offsetX < offsetY) offsetY = offsetX;
            else offsetX = offsetY;

            if ((spring.BouncySides & ControllerSide.Right) > 0)
                hitBounds.max = new Vector2(hitBounds.max.x - offsetX, hitBounds.max.y);
            if ((spring.BouncySides & ControllerSide.Left) > 0)
                hitBounds.min = new Vector2(hitBounds.min.x + offsetX, hitBounds.min.y);
            if ((spring.BouncySides & ControllerSide.Top) > 0)
                hitBounds.max = new Vector2(hitBounds.max.x, hitBounds.max.y - offsetY);
            if ((spring.BouncySides & ControllerSide.Bottom) > 0)
                hitBounds.min = new Vector2(hitBounds.min.x, hitBounds.min.y + offsetY);

            Handles.DrawSolidRectangleWithOutline(new[]
            {
                new Vector3(hitBounds.min.x, hitBounds.max.y),
                hitBounds.min,
                new Vector3(hitBounds.max.x, hitBounds.min.y),
                hitBounds.max,
            }.Select(vector3 =>
                (Vector3)DMath.RotateBy(vector3, originalRotation * Mathf.Deg2Rad, box.transform.position))
                .ToArray(),
                OffFillColor, OffOutlineColor);

            box.transform.eulerAngles = new Vector3(
                box.transform.eulerAngles.x, box.transform.eulerAngles.y, originalRotation);
        }
    }
}
