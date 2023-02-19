Shader "Character/GPUSkinBone"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BoneAnimTex ("Bone Animation Tex", 2D) = "white" {}
        _BoneCount ("Bone Count", Float) = 0
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
            sampler2D _BoneAnimTex;
			float4 _BoneAnimTex_TexelSize;
            float _BoneCount;

			uniform int g_GpuSkinFrameIndex;

            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float, _CurFrame)
            UNITY_DEFINE_INSTANCED_PROP(float, _CurFramePixelIndex)
			UNITY_DEFINE_INSTANCED_PROP(float, _CurFrameCount)
            UNITY_INSTANCING_BUFFER_END(Props)

            float4x4 GetMeshToLocalMatrix(uint boneIndex, uint curFrame, uint curFramePixelIndex)
            {
                uint boneAnimTexWidth = (uint)_BoneAnimTex_TexelSize.z;
                uint boneCount = (uint)_BoneCount;

                uint pixelIndex = curFramePixelIndex + (boneCount * curFrame + boneIndex) * 4;
                float4 rowUV = float4(	(pixelIndex % boneAnimTexWidth + 0.5) * _BoneAnimTex_TexelSize.x,
										(pixelIndex / boneAnimTexWidth + 0.5) * _BoneAnimTex_TexelSize.y, 0, 0);
                float4 row0 =  tex2Dlod(_BoneAnimTex, rowUV);

                pixelIndex = pixelIndex + 1;
                rowUV = float4(	(pixelIndex % boneAnimTexWidth + 0.5) * _BoneAnimTex_TexelSize.x,
								(pixelIndex / boneAnimTexWidth + 0.5) * _BoneAnimTex_TexelSize.y, 0, 0);
                float4 row1 =  tex2Dlod(_BoneAnimTex, rowUV);

                pixelIndex = pixelIndex + 1;
                rowUV = float4(	(pixelIndex % boneAnimTexWidth + 0.5) * _BoneAnimTex_TexelSize.x,
								(pixelIndex / boneAnimTexWidth + 0.5) * _BoneAnimTex_TexelSize.y, 0, 0);
                float4 row2 =  tex2Dlod(_BoneAnimTex, rowUV);

                pixelIndex = pixelIndex + 1;
                rowUV = float4(	(pixelIndex % boneAnimTexWidth + 0.5) * _BoneAnimTex_TexelSize.x,
								(pixelIndex / boneAnimTexWidth + 0.5) * _BoneAnimTex_TexelSize.y, 0, 0);
                float4 row3 =  tex2Dlod(_BoneAnimTex, rowUV);

                return float4x4(row0, row1, row2, row3);
            }

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                uint bone0Index = (uint)v.uv2.x;
                uint bone1Index = (uint)v.uv2.y;
                float bone0Weight = v.uv3.x;
                float bone1Weight = v.uv3.y;
                uint curFrame = (uint)UNITY_ACCESS_INSTANCED_PROP(Props, _CurFrame);
                uint curFramePixelIndex = (uint)UNITY_ACCESS_INSTANCED_PROP(Props, _CurFramePixelIndex);
				uint curFrameCount = (uint)UNITY_ACCESS_INSTANCED_PROP(Props, _CurFrameCount);

				uint realCurFrame = (curFrame + g_GpuSkinFrameIndex) % curFrameCount;

                float4x4 meshToLocal0 = GetMeshToLocalMatrix(bone0Index, realCurFrame, curFramePixelIndex);
                float4x4 meshToLocal1 = GetMeshToLocalMatrix(bone1Index, realCurFrame, curFramePixelIndex);
                float4 localPos0 = mul(meshToLocal0, v.vertex);
                float4 localPos1 = mul(meshToLocal1, v.vertex);

				float4 finalPos = localPos0 * bone0Weight + localPos1 * bone1Weight;
                float4 curVertex = TransformObjectToHClip(finalPos.xyz);

                o.vertex = curVertex;

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
