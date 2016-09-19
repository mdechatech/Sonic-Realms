using UnityEngine;

namespace SonicRealms.Core.Actors
{
    /// <summary>
    /// Holds all of the sensors for HedgehogController.
    /// </summary>
    public class HedgehogSensors : MonoBehaviour
    {
        /// <summary>
        /// This box collider will be modified based on the shape of the sensors.
        /// </summary>
        [Tooltip("This box collider will be modified based on the shape of the sensors.")]
        public BoxCollider2D BoxCollider;

        // For collisions with ceilings
        [Header("Ceiling Collision")]
        public Transform TopLeft;
        public Transform TopLeftStart;
        public Transform TopCenter;
        public Transform TopRight;
        public Transform TopRightStart;

        // For collisions with walls
        [Header("Wall Collision")]
        public Transform CenterLeft;
        public Transform Center;
        public Transform CenterRight;

        // For collisions with the floor when in the air
        [Header("Airborne Floor Collision")]
        public Transform BottomLeft;
        public Transform BottomCenter;
        public Transform BottomRight;

        // For collisions with the floor when on the ground
        [Header("Grounded Floor Collision")]
        public Transform LedgeClimbLeft;
        public Transform LedgeClimbRight;
        public Transform LedgeDropLeft;
        public Transform LedgeDropRight;

        // For collisions with objects that have the special Solid Object component
        [Header("Solid Box Collision")]
        public Transform SolidLeft;
        public Transform SolidCenter;
        public Transform SolidRight;

        #region Sensor Existence Utilites
        public bool HasCeilingSensors
        {
            get { return TopLeft && TopLeftStart && TopCenter && TopRight && TopRightStart; }
        }

        public bool HasSideSensors
        {
            get { return CenterLeft && Center && CenterRight; }
        }

        public bool HasBottomSensors
        {
            get { return BottomLeft && BottomCenter && BottomRight; }
        }

        public bool HasLedgeSensors
        {
            get { return LedgeClimbLeft && LedgeClimbRight && LedgeDropLeft && LedgeDropRight; }
        }

        public bool HasSolidSensors
        {
            get { return SolidLeft && SolidCenter && SolidRight; }
        }
        #endregion
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
                TopLeftStart.localPosition = new Vector3(TopCenter.localPosition.x - value/2.0f,
                    TopLeftStart.localPosition.y,
                    TopLeftStart.localPosition.z);
                TopRight.localPosition = new Vector3(TopCenter.localPosition.x + value/2.0f, TopRight.localPosition.y,
                    TopRight.localPosition.z);
                TopRightStart.localPosition = new Vector3(TopCenter.localPosition.x + value/2.0f,
                    TopRightStart.localPosition.y,
                    TopRightStart.localPosition.z);
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
        /// Y offset of the solid box sensors from the center sensors, always negative.
        /// </summary>
        public float SolidOffset
        {
            get { return SolidCenter.localPosition.y - Center.localPosition.y; }
            set
            {
                SolidLeft.localPosition = new Vector3(SolidLeft.localPosition.x, Center.localPosition.y + value,
                    SolidLeft.localPosition.z);
                SolidCenter.localPosition = new Vector3(SolidCenter.localPosition.x, Center.localPosition.y + value,
                    SolidCenter.localPosition.z);
                SolidRight.localPosition = new Vector3(SolidRight.localPosition.x, Center.localPosition.y + value,
                    SolidRight.localPosition.z);
            }
        }

        /// <summary>
        /// Width of the line formed by the solid box sensors.
        /// </summary>
        public float SolidWidth
        {
            get { return SolidRight.localPosition.x - SolidLeft.localPosition.x; }
            set
            {
                SolidLeft.localPosition = new Vector3(SolidCenter.localPosition.x - value/2.0f,
                    SolidLeft.localPosition.y, SolidLeft.localPosition.z);
                SolidRight.localPosition = new Vector3(SolidCenter.localPosition.x + value/2.0f,
                    SolidRight.localPosition.y, SolidRight.localPosition.z);
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
        #region Sensor Type Utilites
        public Transform Get(SensorType sensor)
        {
            switch (sensor)
            {
                case SensorType.BottomLeft:
                    return BottomLeft;

                case SensorType.BottomRight:
                    return BottomRight;

                case SensorType.CenterLeft:
                    return CenterLeft;

                case SensorType.CenterRight:
                    return CenterRight;

                case SensorType.SolidLeft:
                    return SolidLeft;

                case SensorType.SolidRight:
                    return SolidRight;

                case SensorType.TopLeft:
                    return TopLeft;

                case SensorType.TopRight:
                    return TopRight;

                default:
                    return null;
            }
        }

        public Transform Get(GroundSensorType sensor)
        {
            switch (sensor)
            {
                case GroundSensorType.Left:
                    return BottomLeft;

                case GroundSensorType.Right:
                    return BottomRight;

                default:
                    return null;
            }
        }
        #endregion

        public void FixedUpdate()
        {
            if (BoxCollider == null || !BoxCollider) return;
            UpdateCollider();
        }

        public void UpdateCollider()
        {
            var center = new Vector3(
                (CenterLeft.position.x + CenterRight.position.x)/2.0f, 
                (TopCenter.position.y + BottomCenter.position.y)/2.0f,
                Center.position.z
                );
            var size = new Vector2(
                Vector2.Distance(CenterLeft.position, CenterRight.position),
                Vector2.Distance(BottomCenter.position, TopCenter.position)
                );
            BoxCollider.transform.position = center;
            BoxCollider.transform.eulerAngles = Center.transform.eulerAngles;
            BoxCollider.size = size;
        }
    }
}
