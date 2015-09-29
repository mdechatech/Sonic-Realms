using System.Linq;
using Hedgehog.Core.Actors;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hedgehog.Core.Utils
{
    /// <summary>
    /// Contains values that correspond to physics variables in HedgehogController.
    /// </summary>
    public class HedgehogPhysicsValues
    {
        /// <summary>
        /// What fields to exclude when copying values to and from controllers.
        /// </summary>
        public static readonly string[] ExcludedFields = new[]
        {
            "ExcludedFields",
            "TargetOrthographicSize",
        };

        public float? TargetOrthographicSize;

        public float TopSpeed;
        public float MaxSpeed;
        public float JumpSpeed;
        public float ReleaseJumpSpeed;
        public float GroundAcceleration;
        public float GroundDeceleration;
        public float GroundBrake;
        public float AirAcceleration;
        public float AirDragHorizontalSpeed;
        public float AirDragVerticalSpeed;
        public float AirDragCoefficient;
        public float AirGravity;
        public float GravityDirection;
        public float SlopeGravity;
        public float LedgeClimbHeight;
        public float LedgeDropHeight;
        public float AntiTunnelingSpeed;
        public float SlopeGravityBeginAngle;
        public float DetachSpeed;
        public float HorizontalLockTime;
        public float ForceJumpAngleDifference;
        public float MaxSurfaceAngleDifference;
        public float HorizontalWallmodeAngleWeight;
        public float MinWallmodeSwitchSpeed;
        public float MinOverlapAngle;
        public float MinFlatOverlapRange;
        public float MinFlatAttachAngle;
        public float MaxVerticalDetachAngle;

        public HedgehogPhysicsValues() {}
        public HedgehogPhysicsValues(HedgehogPhysicsValues copy)
        {
            var fields = typeof(HedgehogPhysicsValues).GetFields();
            foreach (var field in fields)
            {
                field.SetValue(this, field.GetValue(copy));
            }
        }

        public static HedgehogPhysicsValues operator *
            (HedgehogPhysicsValues multiplicand, float multiplier)
        {
            var product = new HedgehogPhysicsValues(multiplicand);

            // Multiply all speeds by multiplier
            if(product.TargetOrthographicSize != null)
                product.TargetOrthographicSize *= multiplier;

            product.TopSpeed *= multiplier;
            product.MaxSpeed *= multiplier;
            product.JumpSpeed *= multiplier;
            product.ReleaseJumpSpeed *= multiplier;
            product.GroundAcceleration *= multiplier;
            product.GroundDeceleration *= multiplier;
            product.GroundBrake *= multiplier;
            product.SlopeGravity *= multiplier;
            product.AirAcceleration *= multiplier;
            product.AirDragHorizontalSpeed *= multiplier;
            product.AirDragVerticalSpeed *= multiplier;
            product.AirGravity *= multiplier;
            product.DetachSpeed *= multiplier;

            return product;
        }

        /// <summary>
        /// Sets the specified conroller's values.
        /// </summary>
        /// <param name="controller">The specified controller.</param>
        public void Apply(HedgehogController controller)
        {
            var hedgehogType = typeof (HedgehogController);
            var physicsType = typeof (HedgehogPhysicsValues);

            var fields = physicsType.GetFields();
            foreach (var field in fields)
            {
                if (!ExcludedFields.Contains(field.Name))
                {
                    var controllerField = hedgehogType.GetField(field.Name);
                    if(controllerField != null)
                        controllerField.SetValue(controller, field.GetValue(this));
                    else
                        Debug.LogError("Controller has no property " + field.Name + ".");
                }
            }
        }

        /// <summary>
        /// Gets physics constants from the specified controller as an object which can be saved and applied
        /// to other controllers.
        /// </summary>
        /// <param name="controller">The specified controller.</param>
        /// <returns></returns>
        public static HedgehogPhysicsValues Capture(HedgehogController controller)
        {
            var result = new HedgehogPhysicsValues();

            var hedgehogType = typeof (HedgehogController);
            var physicsType = typeof (HedgehogPhysicsValues);

            var fields = physicsType.GetFields();
            foreach (var field in fields)
            {
                if (!ExcludedFields.Contains(field.Name))
                {
                    var controllerField = hedgehogType.GetField(field.Name);
                    if(controllerField != null)
                        field.SetValue(result, controllerField.GetValue(controller));
                    else
                        Debug.LogError("Controller has no property " + field.Name + ".");
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Various helpers for HedgehogController.
    /// </summary>
    public static class HedgehogUtility
    {
        public const float MegadrivePlayerCameraRatio = 0.0892857f;
        public const float MegadriveOrthographicSize = 1.12f;
        public const float SegaCDOrthographicSize = 1.12f;
        #region Physics Presets
        public static readonly HedgehogPhysicsValues BasePhysicsValues = 
            new HedgehogPhysicsValues
            {
                TargetOrthographicSize = MegadriveOrthographicSize,

                TopSpeed = 3.6f,
                MaxSpeed = 12.0f,
                JumpSpeed = 3.9f,
                ReleaseJumpSpeed = 2.4f,
                GroundAcceleration = 1.6875f,
                GroundDeceleration = 1.6875f,
                GroundBrake = 18.0f,
                AirAcceleration = 7.875f,
                AirDragHorizontalSpeed = 0.075f,
                AirDragVerticalSpeed = 2.4f,
                AirDragCoefficient = 0.1488343f,
                AirGravity = 7.875f,
                GravityDirection = 270.0f,
                SlopeGravity = 4.5f,
                AntiTunnelingSpeed = 5.0f,
                DetachSpeed = 1.5f,
                HorizontalLockTime = 0.5f,
                SlopeGravityBeginAngle = 10.0f,
                ForceJumpAngleDifference = 30.0f,
                LedgeClimbHeight = 0.25f,
                LedgeDropHeight = 0.25f,
                MinWallmodeSwitchSpeed = 0.5f,
                HorizontalWallmodeAngleWeight = 0.0f,
                MaxSurfaceAngleDifference = 70.0f,
                MinOverlapAngle = -40.0f,
                MinFlatOverlapRange = 30.0f,
                MinFlatAttachAngle = 5.0f,
                MaxVerticalDetachAngle = 2.5f
            };

        public static readonly HedgehogPhysicsValues SonicPhysicsValues = 
            new HedgehogPhysicsValues(BasePhysicsValues)
            {
                TargetOrthographicSize = MegadriveOrthographicSize,

                TopSpeed = 3.6f,
                JumpSpeed = 3.9f,
                ReleaseJumpSpeed = 2.4f,
                GroundAcceleration = 1.6875f,
                GroundDeceleration = 1.6875f,
                GroundBrake = 18.0f,
                AirAcceleration = 7.875f,
                AirDragCoefficient = 0.1488343f,
                AirGravity = 7.875f,
                SlopeGravity = 4.5f,
            };

        public static readonly HedgehogPhysicsValues TailsPhysicsValues =
            new HedgehogPhysicsValues(SonicPhysicsValues);

        public static readonly HedgehogPhysicsValues KnucklesPhysicsValues =
            new HedgehogPhysicsValues(SonicPhysicsValues)
            {
                JumpSpeed = 3.6f,
            };
        #endregion

        public const string GeneratedSensorsName = "__Generated Sensors__";

        public static float FOV2OrthographicSize(float fieldOfView)
        {
            // This is a logarithmic regression using some values I experimented with!
            // It becomes inaccurate for FOVs over 100.
            // I have no idea how to correctly find FOV projection sizes.
            return 33.6778f*Mathf.Log(1.26728f);
        }

        /// <summary>
        /// Gets physics constants from the specified controller as an object which can be saved and applied
        /// to other controllers.
        /// </summary>
        /// <param name="controller">The specified controller.</param>
        /// <returns></returns>
        public static HedgehogPhysicsValues CapturePhysicsValues(HedgehogController controller)
        {
            return HedgehogPhysicsValues.Capture(controller);
        }

        /// <summary>
        /// Looks inside the specified controller for the object holding sensors generated through the editor.
        /// </summary>
        /// <param name="controller">The specified controller.</param>
        /// <returns></returns>
        public static HedgehogSensors FindGeneratedSensors(Transform controller)
        {
            foreach (var child in controller.transform)
            {
                var transform = child as Transform;
                if (transform.name == GeneratedSensorsName)
                {
                    return transform.GetComponent<HedgehogSensors>();
                }
            }

            return null;
        }

        /// <summary>
        /// Generates sensors for the specified controller given its size.
        /// </summary>
        /// <param name="controller">The specified controller.</param>
        /// <param name="bounds">The size and position of the controller.</param>
        /// <param name="isLocal">Whether the bounds object is in local space.</param>
        public static void GenerateSensors(HedgehogController controller, Bounds bounds,
            bool isLocal = false)
        {
            if(controller.Sensors != null)
                Object.DestroyImmediate(controller.Sensors.gameObject);

            var sensorsObject = new GameObject { name = GeneratedSensorsName };
            var sensors = sensorsObject.AddComponent<HedgehogSensors>();
            controller.Sensors = sensors;
            
            sensorsObject.transform.SetParent(controller.transform);
            if (isLocal) sensorsObject.transform.localPosition = bounds.center;
            else sensorsObject.transform.position = bounds.center;

            var topLeft = new GameObject {name = "Top Left"};
            var topCenter = new GameObject {name = "Top Mid"};
            var topRight = new GameObject {name = "Top Right"};

            var centerLeft = new GameObject {name = "Center Left"};
            var center = new GameObject {name = "Center"};
            var centerRight = new GameObject {name = "Center Right"};

            var bottomLeft = new GameObject {name = "Bottom Left"};
            var botttomCenter = new GameObject {name = "Bottom Center"};
            var bottomRight = new GameObject {name = "Bottom Right"};
            
            var ledgeDropLeft = new GameObject {name = "Ledge Drop Left"};
            var ledgeDropRight = new GameObject {name = "Ledge Drop Right"};
            var ledgeClimbLeft = new GameObject {name = "Ledge Climb Left"};
            var ledgeClimbRight = new GameObject {name = "Ledge Climb Right"};

            topLeft.transform.SetParent(sensorsObject.transform);
            controller.Sensors.TopLeft = topLeft.transform;

            topCenter.transform.SetParent(sensorsObject.transform);
            controller.Sensors.TopCenter = topCenter.transform;

            topRight.transform.SetParent(sensorsObject.transform);
            controller.Sensors.TopRight = topRight.transform;

            centerLeft.transform.SetParent(sensorsObject.transform);
            controller.Sensors.CenterLeft = centerLeft.transform;

            center.transform.SetParent(sensorsObject.transform);
            controller.Sensors.Center = center.transform;

            centerRight.transform.SetParent(sensorsObject.transform);
            controller.Sensors.CenterRight = centerRight.transform;

            bottomLeft.transform.SetParent(sensorsObject.transform);
            controller.Sensors.BottomLeft = bottomLeft.transform;

            botttomCenter.transform.SetParent(sensorsObject.transform);
            controller.Sensors.BottomCenter = botttomCenter.transform;

            bottomRight.transform.SetParent(sensorsObject.transform);
            controller.Sensors.BottomRight = bottomRight.transform;

            ledgeDropLeft.transform.SetParent(sensorsObject.transform);
            controller.Sensors.LedgeDropLeft = ledgeDropLeft.transform;

            ledgeDropRight.transform.SetParent(sensorsObject.transform);
            controller.Sensors.LedgeDropRight = ledgeDropRight.transform;

            ledgeClimbLeft.transform.SetParent(sensorsObject.transform);
            controller.Sensors.LedgeClimbLeft = ledgeClimbLeft.transform;

            ledgeClimbRight.transform.SetParent(sensorsObject.transform);
            controller.Sensors.LedgeClimbRight = ledgeClimbRight.transform;

            if (isLocal)
            {
                topLeft.transform.localPosition = new Vector3(bounds.min.x, bounds.max.y);
                topCenter.transform.localPosition = new Vector3(bounds.center.x, bounds.max.y);
                topRight.transform.localPosition = bounds.max;
                centerLeft.transform.localPosition = new Vector3(bounds.min.x - 0.01f, bounds.center.y);
                center.transform.localPosition = bounds.center;
                centerRight.transform.localPosition = new Vector3(bounds.max.x + 0.01f, bounds.center.y);
                bottomLeft.transform.localPosition = bounds.min;
                botttomCenter.transform.localPosition = new Vector3(bounds.center.x, bounds.min.y);
                bottomRight.transform.localPosition = new Vector3(bounds.max.x, bounds.min.y);
                ledgeClimbLeft.transform.localPosition = Vector3.Lerp(topLeft.transform.localPosition,
                    bottomLeft.transform.localPosition, 0.5f);
                ledgeClimbRight.transform.localPosition = Vector3.Lerp(topRight.transform.localPosition,
                    bottomRight.transform.localPosition, 0.5f);
                ledgeDropLeft.transform.localPosition = bottomLeft.transform.localPosition +
                                                         Vector3.down * controller.LedgeDropHeight;
                ledgeDropRight.transform.localPosition = bottomRight.transform.localPosition +
                                                         Vector3.down * controller.LedgeDropHeight;
            }
            else
            {
                topLeft.transform.position = new Vector3(bounds.min.x, bounds.max.y);
                topCenter.transform.position = new Vector3(bounds.center.x, bounds.max.y);
                topRight.transform.position = bounds.max;
                centerLeft.transform.position = new Vector3(bounds.min.x - 0.01f, bounds.center.y);
                center.transform.position = bounds.center;
                centerRight.transform.position = new Vector3(bounds.max.x + 0.01f, bounds.center.y);
                bottomLeft.transform.position = bounds.min;
                botttomCenter.transform.position = new Vector3(bounds.center.x, bounds.min.y);
                bottomRight.transform.position = new Vector3(bounds.max.x, bounds.min.y);
                ledgeClimbLeft.transform.position = Vector3.Lerp(topLeft.transform.position,
                     bottomLeft.transform.position, 0.5f);
                ledgeClimbRight.transform.position = Vector3.Lerp(topRight.transform.position,
                    bottomRight.transform.position, 0.5f);
                ledgeDropLeft.transform.localPosition = bottomLeft.transform.localPosition +
                                                         Vector3.down * controller.LedgeDropHeight;
                ledgeDropRight.transform.localPosition = bottomRight.transform.localPosition +
                                                         Vector3.down * controller.LedgeDropHeight;
            }
        }
    }
}
