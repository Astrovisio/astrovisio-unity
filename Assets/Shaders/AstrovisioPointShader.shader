Shader "Astrovisio/PointShader"
{
    Properties { }

    SubShader
    {
        // Tags { "RenderType"="Opaque" }
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
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

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float pointSize : PSIZE;
                float mustDiscard : TEXCOORD0;
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

            v2f vert(appdata v)
            {
                v2f o;

                if ((dataX[v.vertexID] > mappingConfigs[X_INDEX].MaxVal) || (dataX[v.vertexID] < mappingConfigs[X_INDEX].MinVal)) {
                    o.mustDiscard = 1.0;
                    return o;
                }
                if ((dataY[v.vertexID] > mappingConfigs[Y_INDEX].MaxVal) || (dataY[v.vertexID] < mappingConfigs[Y_INDEX].MinVal)) {
                    o.mustDiscard = 1.0;
                    return o;
                }
                if ((dataZ[v.vertexID] > mappingConfigs[Z_INDEX].MaxVal) || (dataZ[v.vertexID] < mappingConfigs[Z_INDEX].MinVal)) {
                    o.mustDiscard = 1.0;
                    return o;
                }               
                if ((dataCmap[v.vertexID] > mappingConfigs[CMAP_INDEX].MaxVal) || (dataCmap[v.vertexID] < mappingConfigs[CMAP_INDEX].MinVal)) {
                    o.mustDiscard = 1.0;
                    return o;
                }

                float3 pos = float3(
                    applyScaling(dataX[v.vertexID], mappingConfigs[X_INDEX]),
                    applyScaling(dataY[v.vertexID], mappingConfigs[Y_INDEX]),
                    applyScaling(dataZ[v.vertexID], mappingConfigs[Z_INDEX])
                );

                pos *= scalingFactor;
                float4 worldPos = mul(datasetMatrix, float4(pos, 1.0));
                o.vertex = UnityObjectToClipPos(worldPos);

                o.pointSize = 15.0; // punto ben visibile (Fuziona ????)

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
