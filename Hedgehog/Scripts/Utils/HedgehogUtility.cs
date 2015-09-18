using System;
using System.Linq;
using Hedgehog.Actors;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hedgehog.Utils
{
    public class HedgehogPhysicsValues
    {
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

        public void Apply(HedgehogController hedgehog)
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
                        controllerField.SetValue(hedgehog, field.GetValue(this));
                    else
                        Debug.LogError("Controller has no property " + field.Name + ".");
                }
            }
        }
    }

    public static class HedgehogUtility
    {
        public const float MegadriveCameraPlayerRatio = 2.8f;
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

        public static Transform SearchGeneratedSensors(Transform hedgehog)
        {
            foreach (var child in hedgehog.transform)
            {
                var transform = child as Transform;
                if (transform.name == GeneratedSensorsName)
                {
                    return transform;
                }
            }

            return null;
        }

        public static void GenerateSensors(HedgehogController hedgehog, Bounds bounds,
            bool isLocal = false)
        {
            var generatedSensors = SearchGeneratedSensors(hedgehog.transform);
            if (generatedSensors != null)
            {
                Object.DestroyImmediate(generatedSensors.gameObject);
            }

            var sensorsObject = new GameObject
            {
                name = GeneratedSensorsName
            };
            sensorsObject.transform.SetParent(hedgehog.transform);
            if (isLocal) sensorsObject.transform.localPosition = bounds.center;
            else sensorsObject.transform.position = bounds.center;

            var sensorTopLeft = new GameObject();
            sensorTopLeft.name = "Top Left";

            var sensorTopMid = new GameObject();
            sensorTopMid.name = "Top Mid";

            var sensorTopRight = new GameObject();
            sensorTopRight.name = "Top Right";

            var sensorMidLeft = new GameObject();
            sensorMidLeft.name = "Mid Left";

            var sensorMidMid = new GameObject();
            sensorMidMid.name = "Mid Mid";

            var sensorMidRight = new GameObject();
            sensorMidRight.name = "Mid Right";

            var sensorBotLeft = new GameObject();
            sensorBotLeft.name = "Bot Left";

            var sensorBotMid = new GameObject();
            sensorBotMid.name = "Bot Mid";

            var sensorBotRight = new GameObject();
            sensorBotRight.name = "Bot Right";

            sensorTopLeft.transform.SetParent(sensorsObject.transform);
            hedgehog.SensorTopLeft = sensorTopLeft.transform;

            sensorTopMid.transform.SetParent(sensorsObject.transform);
            hedgehog.SensorTopMiddle = sensorTopMid.transform;

            sensorTopRight.transform.SetParent(sensorsObject.transform);
            hedgehog.SensorTopRight = sensorTopRight.transform;

            sensorMidLeft.transform.SetParent(sensorsObject.transform);
            hedgehog.SensorMiddleLeft = sensorMidLeft.transform;

            sensorMidMid.transform.SetParent(sensorsObject.transform);
            hedgehog.SensorMiddleMiddle = sensorMidMid.transform;

            sensorMidRight.transform.SetParent(sensorsObject.transform);
            hedgehog.SensorMiddleRight = sensorMidRight.transform;

            sensorBotLeft.transform.SetParent(sensorsObject.transform);
            hedgehog.SensorBottomLeft = sensorBotLeft.transform;

            sensorBotMid.transform.SetParent(sensorsObject.transform);
            hedgehog.SensorBottomMiddle = sensorBotMid.transform;

            sensorBotRight.transform.SetParent(sensorsObject.transform);
            hedgehog.SensorBottomRight = sensorBotRight.transform;

            if (isLocal)
            {
                sensorTopLeft.transform.localPosition = new Vector3(bounds.min.x, bounds.max.y);
                sensorTopMid.transform.localPosition = new Vector3(bounds.center.x, bounds.max.y);
                sensorTopRight.transform.localPosition = bounds.max;
                sensorMidLeft.transform.localPosition = new Vector3(bounds.min.x - 0.01f, bounds.center.y);
                sensorMidMid.transform.localPosition = bounds.center;
                sensorMidRight.transform.localPosition = new Vector3(bounds.max.x + 0.01f, bounds.center.y);
                sensorBotLeft.transform.localPosition = bounds.min;
                sensorBotMid.transform.localPosition = new Vector3(bounds.center.x, bounds.min.y);
                sensorBotRight.transform.localPosition = new Vector3(bounds.max.x, bounds.min.y);
            }
            else
            {
                sensorTopLeft.transform.position = new Vector3(bounds.min.x, bounds.max.y);
                sensorTopMid.transform.position = new Vector3(bounds.center.x, bounds.max.y);
                sensorTopRight.transform.position = bounds.max;
                sensorMidLeft.transform.position = new Vector3(bounds.min.x - 0.01f, bounds.center.y);
                sensorMidMid.transform.position = bounds.center;
                sensorMidRight.transform.position = new Vector3(bounds.max.x + 0.01f, bounds.center.y);
                sensorBotLeft.transform.position = bounds.min;
                sensorBotMid.transform.position = new Vector3(bounds.center.x, bounds.min.y);
                sensorBotRight.transform.position = new Vector3(bounds.max.x, bounds.min.y);
            }

            hedgehog.SensorTopLeft = sensorTopLeft.transform;
            hedgehog.SensorTopMiddle = sensorTopMid.transform;
            hedgehog.SensorTopRight = sensorTopRight.transform;
            hedgehog.SensorMiddleLeft = sensorMidLeft.transform;
            hedgehog.SensorMiddleMiddle = sensorMidMid.transform;
            hedgehog.SensorMiddleRight = sensorMidRight.transform;
            hedgehog.SensorBottomLeft = sensorBotLeft.transform;
            hedgehog.SensorBottomMiddle = sensorBotMid.transform;
            hedgehog.SensorBottomRight = sensorBotRight.transform;
        }
    }
}
