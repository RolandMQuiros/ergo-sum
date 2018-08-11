Shader "Ergo Sum/Texel-Lit Toon"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_TextureDimensions("Texel Size, Texture Size", Vector) = (0, 0, 0, 0)
		_Color ("Color", Color) = (1, 1, 1, 1)
		_ShadowTint ("Shadow Tint", Color) = (0, 0, 0, 1)
	}
	SubShader
	{
		Tags {
			"RenderType" = "Opaque"
			"RenderPipeline" = "LightweightPipeline"
			"IgnoreProjectors" = "True"
		}
		LOD 100

		//Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On
        Cull Back

		Pass
		{
			Tags { "LightMode" = "LightweightForward" }

			HLSLPROGRAM
			#pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

			#pragma vertex LitPassVertexSimple
			#pragma fragment frag

            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _VERTEX_LIGHTS
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
            #pragma multi_compile _ _SHADOWS_ENABLED
            #pragma multi_compile _ _SHADOWS_CASCADE

			#pragma multi_compile_instancing
			#pragma multi_compile_fog
			
			uniform float4 _MainTex_TexelSize;
			uniform float4 _ShadowTint;

			#define _SHADOWS_ENABLED 1 // stupid error won't go away

			#include "CoreRP/ShaderLibrary/Common.hlsl"
			#include "CoreRP/ShaderLibrary/CommonMaterial.hlsl"

			#include "LWRP/ShaderLibrary/Shadows.hlsl"
			#include "LWRP/ShaderLibrary/InputSurfaceSimple.hlsl"
            #include "LWRP/ShaderLibrary/LightweightPassLitSimple.hlsl"
			#include "ErgoSum.hlsl"

			uniform float4 _TextureDimensions;

			half4 frag (LightweightVertexOutput i) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(IN);
				
				float2 snapComponents = TexelSnapComponents(i.uv, _TextureDimensions.zw, i.posWSShininess.xyz);
				float3 texelNormal = TexelSnap3(i.normal, snapComponents);

				half4 finalFragment = half4(0,0,0,0);
				int pixelLightCount = GetPixelLightCount();
				int l = 0;

				Light light = GetMainLight();
				do {
					float nDotL = dot(texelNormal, light.direction);

					float shadowAttenuation = 0.0;
					half4 shadowTint = 0.0;
					if (l == 0) {
					 	shadowAttenuation = MainLightRealtimeShadowAttenuation(TexelSnap4(i.shadowCoord, snapComponents));
						shadowTint = _ShadowTint;// * (-nDotL + 1.0);
					} else {
						shadowAttenuation = LocalLightRealtimeShadowAttenuation(light.index, i.posWSShininess.xyz);
					}
					nDotL *= shadowAttenuation;

					// TODO: Optimize this
					if (nDotL < 0.42) {
						nDotL = 0.42;
					} else if (nDotL >= 0.42 && nDotL < 0.45) {
						nDotL = 0.7;
					} else {
						nDotL = 1.0;
					}

					half3 attenuatedLightColor = light.color * light.attenuation;
					finalFragment += nDotL * SampleAlbedoAlpha(i.uv, TEXTURE2D_PARAM(_MainTex, sampler_MainTex)) * _Color
						* half4(attenuatedLightColor, 1.0)
						+ shadowTint;
					light = GetLight(l, i.posWSShininess.xyz);
				} while (l++ < pixelLightCount);

				return finalFragment;
				// return LitPassFragmentSimple(i);
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
		Pass
        {
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #include "LWRP/ShaderLibrary/InputSurfaceUnlit.hlsl"
            #include "LWRP/ShaderLibrary/LightweightPassDepthOnly.hlsl"
            ENDHLSL
        }
	}
}
