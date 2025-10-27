/*
 * Astrovisio - Astrophysical Data Visualization Tool
 * Copyright (C) 2024-2025 Metaverso SRL
 *
 * This file is part of the Astrovisio project.
 *
 * Astrovisio is free software: you can redistribute it and/or modify it under the terms 
 * of the GNU Lesser General Public License (LGPL) as published by the Free Software 
 * Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Astrovisio is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
 * PURPOSE. See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with 
 * Astrovisio in the LICENSE file. If not, see <https://www.gnu.org/licenses/>.
 *
 */

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

    public class ObjectTransform
    {
        private Vector3Json position;
        private Vector3Json direction;
        private Vector3Json scale;

        [JsonProperty("position")]
        public Vector3Json Position
        {
            get => position;
            set
            {
                if (!position.Equals(value))
                {
                    position = value;
                }
            }
        }

        [JsonProperty("direction")]
        public Vector3Json Direction
        {
            get => direction;
            set
            {
                if (!direction.Equals(value))
                {
                    direction = value;
                }
            }
        }

        [JsonProperty("scale")]
        public Vector3Json Scale
        {
            get => scale;
            set
            {
                if (!scale.Equals(value))
                {
                    scale = value;
                }
            }
        }

        public ObjectTransform(GameObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            Position = obj.transform.position;
            Direction = obj.transform.forward;
            Scale = obj.transform.localScale;
        }

        public ObjectTransform(Vector3 position, Vector3 direction, Vector3 scale)
        {
            Position = position;
            Direction = direction;
            Scale = scale;
        }

        public ObjectTransform(Vector3Json position, Vector3Json direction, Vector3Json scale)
        {
            Position = position;
            Direction = direction;
            Scale = scale;
        }

        public ObjectTransform DeepCopy()
        {
            string json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<ObjectTransform>(json);
        }

        public string Print()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

    }

}
