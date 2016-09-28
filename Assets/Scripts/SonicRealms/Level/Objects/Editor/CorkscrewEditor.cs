using SonicRealms.Core.Utils;
using SonicRealms.Core.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace SonicRealms.Level.Objects.Editor
{
    [CustomEditor(typeof(Corkscrew))]
    public class CorkscrewEditor : BaseFoldoutEditor
    {
        protected void OnSceneGUI()
        {
            var instance = (Corkscrew) target;

            const float pointsPerUnit = 25;
            var pointCount = Mathf.Max(2, Mathf.CeilToInt(instance.Length*pointsPerUnit/instance.Period));

            var points = new Vector3[pointCount];

            points[0] = instance.StartPosition;
            points[pointCount - 1] = instance.EndPosition;

            for (var i = 1; i <= pointCount - 2; ++i)
            {
                points[i] = instance.GetPosition(i/(float)(pointCount - 1));
            }

            Handles.color = Color.white;

            for (var i = 1; i < pointCount; ++i)
            {
                Handles.DrawLine(points[i - 1], points[i]);
            }

            Handles.color = Color.cyan;

            var tangentLength = 0.3f*Mathf.Min(1, instance.Period*0.5f);

            for (var i = 1; i < pointCount; i += 3)
            {
                var position = instance.GetPosition(i/(float) (pointCount - 1));
                var angle = instance.GetSurfaceAngle(i/(float) (pointCount - 1));

                var unit = DMath.UnitVector(angle*Mathf.Deg2Rad);

                Handles.DrawLine(position - unit*tangentLength, position + unit*tangentLength);
            }
        }
    }
}
