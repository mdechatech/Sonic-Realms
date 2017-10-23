using SonicRealms.Core.Internal;
using SonicRealms.Core.Utils;
using SonicRealms.Core.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace SonicRealms.Level.Objects.Editor
{
    [CustomEditor(typeof(Corkscrew))]
    public class CorkscrewEditor : BaseRealmsEditor
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
            var arrowtipLength = tangentLength*0.3f;

            for (var i = 1; i < pointCount; i += 3)
            {
                var position = instance.GetPosition(i/(float) (pointCount - 1));
                var angle = instance.GetSurfaceAngle(i/(float) (pointCount - 1));

                var unit = SrMath.UnitVector(angle*Mathf.Deg2Rad);

                var arrowStart = position - unit*tangentLength;
                var arrowEnd = position + unit*tangentLength;

                Handles.DrawLine(arrowStart, arrowEnd);

                Handles.DrawLine(arrowEnd, arrowEnd - SrMath.UnitVector((angle - 45)*Mathf.Deg2Rad)*arrowtipLength);
                Handles.DrawLine(arrowEnd, arrowEnd - SrMath.UnitVector((angle + 45)*Mathf.Deg2Rad)*arrowtipLength);
            }
        }
    }
}
