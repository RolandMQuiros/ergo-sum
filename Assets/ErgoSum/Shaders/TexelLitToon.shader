Shader "Ergo Sum/Texel-Lit Toon"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_MainTex_TexelSize("Texel Size", Vector) = (0, 0, 0, 0)
		_Color ("Color", Color) = (1, 1, 1, 1)
		_ShadowTint ("Shadow Tint", Color) = (0, 0, 0, 1)
	}
	SubShader
	{
		Tags {
			"RenderType" = "Opaque"
			"RenderPipeline" = "LightweightPipeline"
			"IgnoreProjector" = "True"
		}
		LOD 100

		Pass
		{
			Tags { "LightMode" = "LightweightForward" }

			HLSLPROGRAM
			#pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

			#pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _VERTEX_LIGHTS

			#pragma vertex LitPassVertexSimple
			#pragma fragment frag

			#pragma multi_compile_instancing
			#pragma multi_compile_fog
			
			uniform float4 _MainTex_TexelSize;
			uniform float4 _ShadowTint;

			#include "LWRP/ShaderLibrary/InputSurfaceSimple.hlsl"
            #include "LWRP/ShaderLibrary/LightweightPassLitSimple.hlsl"
			#include "ErgoSum.hlsl"

			half4 frag (LightweightVertexOutput i) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(IN);
				float3 texelNormal = TexelNormal(i.uv, i.posWSShininess.xyz, i.normal);
				half4 finalFragment = half4(0,0,0,0);
				int pixelLightCount = GetPixelLightCount();
				int l = 0;

				Light light = GetMainLight(i.posWSShininess.xyz);
				do {
					float nDotL = dot(texelNormal, light.direction);
					if (nDotL < 0.42) {
						nDotL = 0.42;
					} else if (nDotL >= 0.42 && nDotL < 0.45) {
						nDotL = 0.7;
					} else {
						nDotL = 1.0;
					}
					half3 attenuatedLightColor = light.color * light.attenuation;
					finalFragment += (_ShadowTint * (-nDotL + 1.0))
						+ nDotL * SampleAlbedoAlpha(i.uv, TEXTURE2D_PARAM(_MainTex, sampler_MainTex))
						* half4(attenuatedLightColor, 1.0);
					light = GetLight(l, i.posWSShininess.xyz);
				} while (l++ < pixelLightCount);

				return finalFragment;
				//return LitPassFragmentSimple(i);
			}
			ENDHLSL
		}
		Pass
        {
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _GLOSSINESS_FROM_BASE_ALPHA

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "LWRP/ShaderLibrary/InputSurfaceSimple.hlsl"
            #include "LWRP/ShaderLibrary/LightweightPassShadow.hlsl"
            ENDHLSL
        }
	}
}
