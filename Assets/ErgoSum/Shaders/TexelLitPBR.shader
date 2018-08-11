Shader "Ergo Sum/Texel-Lit PBR"
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
				float3 texelNormal = TexelSnap(i.uv, i.posWSShininess.xyz, i.normal);				
                i.normal = texelNormal;
				return LitPassFragmentSimple(i);
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
