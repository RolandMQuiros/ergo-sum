Shader "Ergo Sum/BillboardedSprite"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		[MaterialToggle] PixelSnap ("Pixel Snap", Float) = 0
	}
	SubShader
	{
		Tags{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
			"RenderPipeline" = "LightweightPipeline"
			"IgnoreProjector" = "True"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
			"DisableBatching" = "True"
		}
		LOD 100

		Cull Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog

			#include "LWRP/ShaderLibrary/InputSurfaceUnlit.hlsl"

			struct VertexInput {
				float4 position     : POSITION;
				float2 texcoord     : TEXCOORD0;
				float4 color	    : COLOR0;
			};

			struct VertexOutput {
				float2 uv           : TEXCOORD0;
				float4 clipPos      : SV_POSITION;
				float4 color	    : COLOR0;
			};

			VertexOutput vert(VertexInput i) {
				VertexOutput v;
				v.uv = TRANSFORM_TEX(i.texcoord, _MainTex);

				float4x4 mv = UNITY_MATRIX_MV;
				float2 scale = float2(
					length(float3(mv._m00, mv._m10, mv._m20)),
					length(float3(mv._m01, mv._m11, mv._m21))
				);
				v.clipPos = mul(
					UNITY_MATRIX_P,
					mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0))
						+ float4(i.position.x, i.position.y, 0.0, 0.0)
						* float4(scale.x, scale.y, 1.0, 1.0)
				);
				v.color = i.color;

				return v;
			}

			half4 frag(VertexOutput v) : SV_Target {
				return SampleAlbedoAlpha(v.uv, TEXTURE2D_PARAM(_MainTex, sampler_MainTex)) * v.color;
			}
			ENDHLSL
		}
	}
}
