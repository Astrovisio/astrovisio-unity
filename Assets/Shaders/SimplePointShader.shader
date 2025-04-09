Shader "Hidden/SimplePointShader"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            StructuredBuffer<float> dataX;
            StructuredBuffer<float> dataY;
            StructuredBuffer<float> dataZ;
            StructuredBuffer<float> dataSize;
            StructuredBuffer<float> dataRho;

            float4x4 datasetMatrix;
            float scalingFactor;

            float thresholdMin;
            float thresholdMax;
            
            sampler2D colormapTex;

            struct appdata
            {
                uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float size : TEXCOORD0; 
                float rho : TEXCOORD1;
                float pointSize : PSIZE;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                float x = dataX[v.vertexID];
                float y = dataY[v.vertexID];
                float z = dataZ[v.vertexID];
                float4 pos = float4(x, y, z, 1.0);

                pos = mul(datasetMatrix, pos);
                pos.xyz *= scalingFactor;
                o.vertex = mul(UNITY_MATRIX_VP, pos);

                o.pointSize = 100.0;
                o.size = dataSize[v.vertexID];
                o.rho = dataRho[v.vertexID];
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                if(i.size < thresholdMin || i.size > thresholdMax)
                    discard;
                
                fixed4 col = tex2D(colormapTex, float2(i.size, 0.5));
                return col;
            }
            ENDCG
        }
    }
}
