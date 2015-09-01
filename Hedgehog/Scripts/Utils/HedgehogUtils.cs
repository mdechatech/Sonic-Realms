using Hedgehog.Actors;
using UnityEngine;

namespace Hedgehog.Utils
{
    public static class HedgehogUtils
    {
        public const string GeneratedSensorsName = "__Generated Sensors__";

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
            var generatedSensors = HedgehogUtils.SearchGeneratedSensors(hedgehog.transform);
            if (generatedSensors != null)
            {
                Object.DestroyImmediate(generatedSensors.gameObject);
                generatedSensors = null;
            }

            var sensorsObject = new GameObject();
            sensorsObject.name = HedgehogUtils.GeneratedSensorsName;
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
