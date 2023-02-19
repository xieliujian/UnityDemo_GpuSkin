Shader "LingRen/GPUSkinVertex"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _VertexAnimTex ("Vertex Animation Tex", 2D) = "white" {}
        _VertexCount ("Vertex Count", Float) = 0
        _CurFrame ("Current Frame", Float) = 0
        _CurFramePixelIndex ("Current Frame Pixel Index", Float) = 0
		_CurFrameCount("Frame Count", Float) = 30
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Tags {"LightMode" = "UniversalForward"}
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            #pragma multi_compile_instancing

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float2 uv3 : TEXCOORD2;
                uint index : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _VertexAnimTex;			
			float4 _VertexAnimTex_TexelSize;
            float _VertexCount;

			uniform int g_GpuSkinFrameIndex;

            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float, _CurFrame)
            UNITY_DEFINE_INSTANCED_PROP(float, _CurFramePixelIndex)
			UNITY_DEFINE_INSTANCED_PROP(float, _CurFrameCount)
            UNITY_INSTANCING_BUFFER_END(Props)
			
            float3 GetVertexPos(uint vertexIndex, uint curFrame, uint curFramePixelIndex)
            {
                uint vertexAnimTexWidth = (uint)_VertexAnimTex_TexelSize.z;
                uint vertexCount = (uint)_VertexCount;

                uint pixelIndex = curFramePixelIndex + vertexCount * curFrame + vertexIndex;
                float4 uv = float4(	(pixelIndex % vertexAnimTexWidth + 0.5) * _VertexAnimTex_TexelSize.x,
									(pixelIndex / vertexAnimTexWidth + 0.5) * _VertexAnimTex_TexelSize.y, 0, 0);
                float3 vertexPos =  tex2Dlod(_VertexAnimTex, uv).xyz;

                return vertexPos;
            }

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                uint curFrame = (uint)UNITY_ACCESS_INSTANCED_PROP(Props, _CurFrame);
                uint curFramePixelIndex = (uint)UNITY_ACCESS_INSTANCED_PROP(Props, _CurFramePixelIndex);
				uint curFrameCount = (uint)UNITY_ACCESS_INSTANCED_PROP(Props, _CurFrameCount);

				uint realCurFrame = (curFrame + g_GpuSkinFrameIndex) % curFrameCount;

                float3 vertexPos = GetVertexPos(v.index, realCurFrame, curFramePixelIndex);
                o.vertex = TransformObjectToHClip(vertexPos);

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                return col;
            }
			
            ENDHLSL
        }
    }
}
