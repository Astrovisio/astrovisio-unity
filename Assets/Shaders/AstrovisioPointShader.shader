Shader "Astrovisio/PointShader"
{
    Properties 
    {




    }

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
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            #define LINEAR 0
            #define LOG 1
            #define SQRT 2

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
                float DataMinVal;
                float DataMaxVal;
                int InverseMapping;
                float Offset;
                int ScalingType;
                float TargetMinVal;
                float TargetMaxVal;
            };

            // Properties variables
            uniform int useUniformColor;
            uniform int useUniformOpacity;
            uniform int useNoise;
            uniform int isolateSelection;
            // Data buffers for positions and values
            StructuredBuffer<float> dataX;
            StructuredBuffer<float> dataY;
            StructuredBuffer<float> dataZ;
            StructuredBuffer<float> dataCmap;
            StructuredBuffer<float> dataOpacity;
            StructuredBuffer<int> dataVisible;
            StructuredBuffer<MappingConfig> mappingConfigs;
            // Color maps
            uniform sampler2D colorMap;
            uniform float colorMapIndex;
            uniform int numColorMaps;
            // Uniforms
            uniform float4 color;
            uniform float opacity;
            uniform float noiseStrength;

            float4x4 datasetMatrix;

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float mustDiscard : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float signed_log10(float x, float scale) {
                return sign(x) * log10(1 + abs(x) / scale);
            }

            float signed_sqrt(float x, float scale) {
                return sign(x) * sqrt(abs(x) / scale);
            }
            
            float map(float value, float min1, float max1, float min2, float max2) {
                return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
            }

            float noise(float3 p)
            {
                float dotProduct = dot(p, float3(12.9898, 78.233, 37.719));
                return frac(sin(dotProduct) * 43758.5453);
            }

            float applyNoise(float input, float3 p) 
            {
                return input + (noise(p) - 0.5) * noiseStrength;
            }

            float applyScaling(float input, MappingConfig config)
            {

                float scaledValue;
                switch (config.ScalingType)
                {
                    case LOG:
                        scaledValue = signed_log10(input, 1.0);
                        scaledValue = map(scaledValue, signed_log10(config.DataMinVal, 1.0), signed_log10(config.DataMaxVal, 1.0), config.TargetMinVal, config.TargetMaxVal);
                        break;
                    case SQRT:
                        scaledValue = signed_sqrt(input, 1.0);
                        scaledValue = map(scaledValue, signed_sqrt(config.DataMinVal, 1.0), signed_sqrt(config.DataMaxVal, 1.0), config.TargetMinVal, config.TargetMaxVal);
                        break;
                    default:
                        scaledValue = input;
                        scaledValue = map(scaledValue, config.DataMinVal, config.DataMaxVal, config.TargetMinVal, config.TargetMaxVal);
                        break;
                }

                // if (config.Clamped) {
                //     scaledValue = clamp(scaledValue, config.TargetMinVal, config.TargetMaxVal);
                // }

                // scaledValue += config.Offset;
                
                return scaledValue;
            }

            v2f vert(appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                if (isolateSelection && !dataVisible[v.vertexID]) {
                    o.mustDiscard = 1.0;
                    return o;
                }

                if ((dataX[v.vertexID] > mappingConfigs[X_INDEX].DataMaxVal) || (dataX[v.vertexID] < mappingConfigs[X_INDEX].DataMinVal)) {
                    o.mustDiscard = 1.0;
                    return o;
                }
                if ((dataY[v.vertexID] > mappingConfigs[Y_INDEX].DataMaxVal) || (dataY[v.vertexID] < mappingConfigs[Y_INDEX].DataMinVal)) {
                    o.mustDiscard = 1.0;
                    return o;
                }
                if ((dataZ[v.vertexID] > mappingConfigs[Z_INDEX].DataMaxVal) || (dataZ[v.vertexID] < mappingConfigs[Z_INDEX].DataMinVal)) {
                    o.mustDiscard = 1.0;
                    return o;
                }               
                if (!useUniformColor && (mappingConfigs[CMAP_INDEX].Clamped) && ((dataCmap[v.vertexID] > mappingConfigs[CMAP_INDEX].DataMaxVal) || (dataCmap[v.vertexID] < mappingConfigs[CMAP_INDEX].DataMinVal))) {
                    o.mustDiscard = 1.0;
                    return o;
                }
                if (!useUniformOpacity && (mappingConfigs[OPACITY_INDEX].Clamped) && ((dataOpacity[v.vertexID] > mappingConfigs[OPACITY_INDEX].DataMaxVal) || (dataOpacity[v.vertexID] < mappingConfigs[OPACITY_INDEX].DataMinVal))) {
                    o.mustDiscard = 1.0;
                    return o;
                }

                float x = applyScaling(dataX[v.vertexID], mappingConfigs[X_INDEX]);
                float y = applyScaling(dataY[v.vertexID], mappingConfigs[Y_INDEX]);
                float z = applyScaling(dataZ[v.vertexID], mappingConfigs[Z_INDEX]);

                if(useNoise) {
                    x = applyNoise(x, float3(dataX[v.vertexID], dataY[v.vertexID], dataZ[v.vertexID]));
                    y = applyNoise(y, float3(dataY[v.vertexID], dataZ[v.vertexID], dataX[v.vertexID]));
                    z = applyNoise(z, float3(dataZ[v.vertexID], dataX[v.vertexID], dataY[v.vertexID]));
                }


                float3 pos = float3(
                    x,
                    y,
                    z
                );

                float4 worldPos = mul(datasetMatrix, float4(pos, 1.0));
                o.vertex = UnityObjectToClipPos(worldPos);

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

            fixed4 frag(v2f i) : SV_Target
            {
                if(i.mustDiscard > 0.5) {
                    discard;
                }
                return i.color;
            }

            ENDCG
        }
    }
    FallBack Off
}
