Shader "Hidden/SimpleXYZPointShader"
{
    Properties { }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            StructuredBuffer<float> dataX;
            StructuredBuffer<float> dataY;
            StructuredBuffer<float> dataZ;

            float4x4 datasetMatrix;
            float scalingFactor;

            struct appdata
            {
                uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float pointSize : PSIZE;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float3 pos = float3(
                    dataX[v.vertexID],
                    dataY[v.vertexID],
                    dataZ[v.vertexID]
                );

                pos *= scalingFactor;
                float4 worldPos = mul(datasetMatrix, float4(pos, 1.0));

                // Stereo-safe projection
                o.vertex = mul(UNITY_MATRIX_VP, worldPos);
                o.color = float4(1.0, 0.0, 0.0, 1.0); // rosso
                o.pointSize = 15.0;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return i.color;
            }

            ENDCG
        }
    }
    FallBack Off
}
