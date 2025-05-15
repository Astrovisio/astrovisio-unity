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

            struct appdata
            {
                uint vertexID : SV_VertexID;
            };

            StructuredBuffer<float> dataX;
            StructuredBuffer<float> dataY;
            StructuredBuffer<float> dataZ;

            float4x4 datasetMatrix;
            float scalingFactor;

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float pointSize : PSIZE;
            };

            v2f vert(appdata v)
            {
                v2f o;

                float3 pos = float3(
                    dataX[v.vertexID],
                    dataY[v.vertexID],
                    dataZ[v.vertexID]
                );

                pos *= scalingFactor;
                float4 worldPos = mul(datasetMatrix, float4(pos, 1.0));
                o.vertex = UnityObjectToClipPos(worldPos);

                o.color = float4(1.0, 0.0, 0.0, 1.0); // ROSSO brillante
                o.pointSize = 15.0; // punto ben visibile

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
