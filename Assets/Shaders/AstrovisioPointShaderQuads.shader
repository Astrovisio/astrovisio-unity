Shader "Astrovisio/PointShaderQuads"
{
    Properties { }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        // Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Pass
        {

            // Basic additive blending
			ZWrite Off
			// Blend SrcAlpha One
            Blend SrcAlpha One

            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #pragma multi_compile_particles_point
            #pragma target 3.5

            #include "UnityCG.cginc"

            #define LINEAR 0
            #define LOG 1
            #define SQRT 2
            #define SQUARED 3
            #define EXP 4

            #define X_INDEX 0
            #define Y_INDEX 1
            #define Z_INDEX 2
            #define CMAP_INDEX 3
            #define OPACITY_INDEX 4
            // #define POINT_SIZE_INDEX 5
            // #define POINT_SHAPE_INDEX 6

            struct appdata
            {
                uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct MappingConfig
            {
                int Clamped;
                float MinVal;
                float MaxVal;
                int InverseMapping;
                float Offset;
                float Scale;
                int ScalingType;
                
                // Future use: Will filter points based on this range
                float FilterMinVal;
                float FilterMaxVal;
            };

            // Properties variables
            uniform int useUniformColor;
            uniform int useUniformOpacity;
            // Data buffers for positions and values
            StructuredBuffer<float> dataX;
            StructuredBuffer<float> dataY;
            StructuredBuffer<float> dataZ;
            StructuredBuffer<float> dataCmap;
            StructuredBuffer<float> dataOpacity;
            StructuredBuffer<MappingConfig> mappingConfigs;
            // Filtering
            uniform float cutoffMin;
            uniform float cutoffMax;
            // Color maps
            uniform sampler2D colorMap;
            uniform float colorMapIndex;
            uniform int numColorMaps;
            // Uniforms
            uniform float4 color;
            uniform float opacity;

            float4x4 datasetMatrix;
            float scalingFactor;

            // Struttura per l'output del vertex shader e input del geometry shader
            struct v2g
            {
                float4 worldPos : POSITION;
                float4 color : COLOR;
            };

            // Struttura per l'output del geometry shader e input del fragment shader
            struct g2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            
            float map(float value, float min1, float max1, float min2, float max2) {
                return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
            }

            float applyScaling(float input, MappingConfig config)
            {
                float scaledValue;
                switch (config.ScalingType)
                {
                    case LOG:
                        scaledValue = log(input);
                        break;
                    case SQRT:
                        scaledValue = sqrt(input);
                        break;
                    case SQUARED:
                        scaledValue = input * input;
                        break;
                    case EXP:
                        scaledValue = exp(input);
                        break;
                    default:
                        scaledValue = input * config.Scale;
                        break;
                }
                
                if (config.Clamped)
                {
                    scaledValue = clamp(scaledValue, config.MinVal, config.MaxVal);
                }

                float toMin = 0;
                float toMax = 1;
                if (config.InverseMapping) {
                    toMin = 1;
                    toMax = 0;
                }
                scaledValue = map(scaledValue, config.MinVal, config.MaxVal, toMin, toMax);

                scaledValue += config.Offset;
                
                return scaledValue;
            }

            v2g vert(appdata v)
            {
                v2g o;

                float3 pos = float3(
                    applyScaling(dataX[v.vertexID], mappingConfigs[X_INDEX]),
                    applyScaling(dataY[v.vertexID], mappingConfigs[Y_INDEX]),
                    applyScaling(dataZ[v.vertexID], mappingConfigs[Z_INDEX])
                );

                pos *= scalingFactor;
                float4 worldPos = mul(datasetMatrix, float4(pos, 1.0));
                o.worldPos = worldPos;
                //o.vertex = UnityObjectToClipPos(worldPos);

                if (!useUniformColor) {        
                    float value = applyScaling(dataCmap[v.vertexID], mappingConfigs[CMAP_INDEX]);
      
                    // Apply color mapping
                    float colorMapOffset = 1.0 - (0.5 + colorMapIndex) / numColorMaps;
                    o.color = tex2Dlod(colorMap, float4(value, colorMapOffset, 0, 0));
                }
                else {
                    o.color = color;
                }

                if (!useUniformOpacity) {                
                    o.color.a = applyScaling(dataOpacity[v.vertexID], mappingConfigs[OPACITY_INDEX]);
                }
                else {
                    o.color.a = opacity;
                }    
                
                return o;
            }

            // Geometry shader
            [maxvertexcount(6)]
            void geom(point v2g input[1], inout TriangleStream<g2f> triStream)
            {
                float3 center = input[0].worldPos;
                float4 col = input[0].color;

                // Calcola i vettori per orientare il quad verso la camera
                float3 right = normalize(_WorldSpaceCameraPos - center);
                float3 up = float3(0, 1, 0);
                float3 forward = normalize(cross(up, right));
                right = normalize(cross(forward, up));

                float size = 0.01; // Dimensione del quad

                float3 offsetRight = right * size;
                float3 offsetUp = up * size;

                // Definisci i quattro vertici del quad
                float3 v0 = center - offsetRight - offsetUp;
                float3 v1 = center + offsetRight - offsetUp;
                float3 v2 = center + offsetRight + offsetUp;
                float3 v3 = center - offsetRight + offsetUp;

                g2f o;

                // Primo triangolo
                o.vertex = UnityObjectToClipPos(float4(v0, 1.0));
                o.color = col;
                triStream.Append(o);

                o.vertex = UnityObjectToClipPos(float4(v1, 1.0));
                o.color = col;
                triStream.Append(o);

                o.vertex = UnityObjectToClipPos(float4(v2, 1.0));
                o.color = col;
                triStream.Append(o);

                // Secondo triangolo
                o.vertex = UnityObjectToClipPos(float4(v2, 1.0));
                o.color = col;
                triStream.Append(o);

                o.vertex = UnityObjectToClipPos(float4(v3, 1.0));
                o.color = col;
                triStream.Append(o);

                o.vertex = UnityObjectToClipPos(float4(v0, 1.0));
                o.color = col;
                triStream.Append(o);
            }

            fixed4 frag(g2f i) : SV_Target
            {
                return i.color;
            }

            ENDCG
        }
    }
    FallBack Off
}
