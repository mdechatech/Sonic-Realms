using UnityEngine;

namespace Hedgehog.Core.Actors
{
    /// <summary>
    /// Holds all of the sensors for HedgehogController.
    /// </summary>
    public class HedgehogSensors : MonoBehaviour
    {
        public float DefaultSideOffset = 0.01f;

        // These sensors are used for hit detection with ceilings.
        public Transform TopLeft;
        public Transform TopCenter;
        public Transform TopRight;

        // These sensors are used for hit detection with walls.
        public Transform CenterLeft;
        public Transform Center;
        public Transform CenterRight;

        /// These sensors are used for hit detection with the floor when in the air.
        public Transform BottomLeft;
        public Transform BottomCenter;
        public Transform BottomRight;

        // These sensors are used for hit detection with the floor when on the ground.
        public Transform LedgeClimbLeft;
        public Transform LedgeClimbRight;
        public Transform LedgeDropLeft;
        public Transform LedgeDropRight;
        #region Sensor Line Utilities
        /// <summary>
        /// Width of the line formed by the top sensors.
        /// </summary>
        public float TopWidth
        {
            get { return TopRight.localPosition.x - TopLeft.localPosition.x; }
            set
            {
                TopLeft.localPosition = new Vector3(TopCenter.localPosition.x - value/2.0f, TopLeft.localPosition.y,
                    TopLeft.localPosition.z);
                TopRight.localPosition = new Vector3(TopCenter.localPosition.x + value/2.0f, TopRight.localPosition.y,
                    TopRight.localPosition.z);
            }
        }

        /// <summary>
        /// Y offset of the top sensors from the center sensors.
        /// </summary>
        public float TopOffset
        {
            get { return TopCenter.localPosition.y - Center.localPosition.y; }
            set
            {
                TopLeft.localPosition = new Vector3(TopLeft.localPosition.x, Center.localPosition.y + value,
                    TopLeft.localPosition.z);
                TopCenter.localPosition = new Vector3(TopCenter.localPosition.x, Center.localPosition.y + value,
                    TopCenter.localPosition.z);
                TopRight.localPosition = new Vector3(TopRight.localPosition.x, Center.localPosition.y + value,
                    TopRight.localPosition.z);
            }
        }

        /// <summary>
        /// Width of the line formed by the center sensors.
        /// </summary>
        public float CenterWidth
        {
            get { return CenterRight.localPosition.x - CenterLeft.localPosition.x; }
            set
            {
                CenterLeft.localPosition = new Vector3(Center.localPosition.x - value/2.0f, CenterLeft.localPosition.y,
                    CenterLeft.localPosition.z);
                CenterRight.localPosition = new Vector3(Center.localPosition.x + value/2.0f, CenterRight.localPosition.y,
                    CenterRight.localPosition.z);
            }
        }

        /// <summary>
        /// Width of the line formed by the bottom sensors.
        /// </summary>
        public float BottomWidth
        {
            get { return BottomRight.localPosition.x - BottomLeft.localPosition.x; }
            set
            {
                BottomLeft.localPosition = new Vector3(BottomCenter.localPosition.x - value/2.0f,
                    BottomLeft.localPosition.y, BottomLeft.localPosition.z);
                BottomRight.localPosition = new Vector3(BottomCenter.localPosition.x + value/2.0f,
                    BottomRight.localPosition.y, BottomRight.localPosition.z);
            }
        }

        /// <summary>
        /// Y offset of the bottom sensors from the center sensors, always negative.
        /// </summary>
        public float BottomOffset
        {
            get { return BottomCenter.localPosition.y - Center.localPosition.y; }
            set
            {
                BottomLeft.localPosition = new Vector3(BottomLeft.localPosition.x, Center.localPosition.y + value,
                    BottomLeft.localPosition.z);
                BottomCenter.localPosition = new Vector3(BottomCenter.localPosition.x, Center.localPosition.y + value,
                    BottomCenter.localPosition.z);
                BottomRight.localPosition = new Vector3(BottomRight.localPosition.x, Center.localPosition.y + value,
                    BottomRight.localPosition.z);
            }
        }

        /// <summary>
        /// Width of the line formed by ledge climb and drop sensors.
        /// </summary>
        public float LedgeWidth
        {
            get { return LedgeClimbRight.localPosition.x - LedgeClimbLeft.localPosition.x; }
            set
            {
                var change = (value - LedgeWidth)/2.0f;
                LedgeClimbLeft.localPosition -= Vector3.right*change;
                LedgeClimbRight.localPosition += Vector3.right*change;
                LedgeDropLeft.localPosition -= Vector3.right*change;
                LedgeDropRight.localPosition += Vector3.right*change;
            }
        }
        #endregion
    }
}
