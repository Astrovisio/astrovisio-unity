using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Astrovisio
{

    [JsonObject(MemberSerialization.OptIn)]
    public struct Vector3Json
    {
        [JsonProperty] public float x;
        [JsonProperty] public float y;
        [JsonProperty] public float z;

        public Vector3Json(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public Vector3 ToVector3() => new Vector3(x, y, z);

        public static implicit operator Vector3Json(Vector3 v) => new Vector3Json(v);
        public static implicit operator Vector3(Vector3Json v) => v.ToVector3();

    }

    public class CameraSettings
    {
        private Vector3Json cameraPosition;
        private Vector3Json cameraDirection;

        [JsonProperty("camera_position")]
        public Vector3Json CameraPosition
        {
            get => cameraPosition;
            set
            {
                if (!cameraPosition.Equals(value))
                {
                    cameraPosition = value;
                }
            }
        }

        [JsonProperty("camera_direction")]
        public Vector3Json CameraDirection
        {
            get => cameraDirection;
            set
            {
                if (!cameraDirection.Equals(value))
                {
                    cameraDirection = value;
                }
            }
        }

        public CameraSettings(Camera camera)
        {
            if (camera == null)
                throw new ArgumentNullException(nameof(camera));
            CameraPosition = camera.transform.position;
            CameraDirection = camera.transform.forward;
        }

        public CameraSettings(Vector3 position, Vector3 direction)
        {
            CameraPosition = position;
            CameraDirection = direction;
        }

        public CameraSettings(Vector3Json position, Vector3Json direction)
        {
            CameraPosition = position;
            CameraDirection = direction;
        }

        public CameraSettings DeepCopy()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<CameraSettings>(json);
        }

        public string Print()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

    }

}
