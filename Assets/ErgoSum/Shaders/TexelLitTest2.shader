Shader "Unlit/TexelLit2"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_MainTex_TexelSize("Texel Size", Vector) = (0, 0, 0, 0)
		_Color ("Color", Color) = (1, 1, 1, 1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Tags { "LightMode" = "LightweightForward" }

			HLSLPROGRAM
			#pragma prefer_hlslcc gles
            // #pragma exclude_renderers d3d11_9x
			#pragma enable_d3d11_debug_symbols
            #pragma target 2.0

			#pragma multi_compile _ _ADDITIONAL_LIGHTS

			#pragma vertex LitPassVertexSimple
			#pragma fragment frag
			#pragma multi_compile_fog
			
			uniform float4 _MainTex_TexelSize;

			#include "LWRP/ShaderLibrary/InputSurfaceSimple.hlsl"
            #include "LWRP/ShaderLibrary/LightweightPassLitSimple.hlsl"

			half4 frag (LightweightVertexOutput i) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(IN);

				// 1.) Calculate how much the texture UV coords need to
				//     shift to be at the center of the nearest texel.
				float2 originalUV = i.uv;
				float2 centerUV = floor(originalUV * _MainTex_TexelSize.zw)/_MainTex_TexelSize.zw + (_MainTex_TexelSize.xy/2.0);
				float2 dUV = originalUV - centerUV;

				if (length(dUV) < 0.001) {
					return half4(i.normal, 1);
				}
			
				// 2a.) Get this fragment's world position
				float3 originalWorldPos = i.posWSShininess.xyz;
			
				// 2b.) Calculate how much the texture coords vary over fragment space.
				//      This essentially defines a 2x2 matrix that gets
				//      texture space (UV) deltas from fragment space (ST) deltas
				// Note: I call fragment space (S,T) to disambiguate.
				float2 dUVdS = ddx(originalUV);
				float2 dUVdT = ddy(originalUV);
			
				// 2c.) Invert the fragment from texture matrix
				float2x2 dSTdUV = float2x2(dUVdT[1], -dUVdS[1], -dUVdT[0], dUVdS[0]) * (1 / (dUVdS[0] * dUVdT[1] - dUVdS[1] * dUVdT[0]));
			
				// 2d.) Convert the UV delta to a fragment space delta
				float2 dST = mul(dSTdUV, dUV); // dST = How many fragments until the center of the texel
				// dd[x|y](normal) is normal variation per fragment
			
				// 2e.) Calculate how much the world coords vary over fragment space.
				// float3 dXYZdS = ddx(originalWorldPos);
				// float3 dXYZdT = ddy(originalWorldPos);

				float3 ndS = ddx(i.normal);
				float3 ndT = ddy(i.normal);
			
				// 2f.) Finally, convert our fragment space delta to a world space delta
				// And be sure to clamp it to SOMETHING in case the derivative calc went insane
				// Here I clamp it to -1 to 1 unit in unity, which should be orders of magnitude greater
				// than the size of any texel.
				// float3 dXYZ = dXYZdS * dST[0] + dXYZdT * dST[1];
				// dXYZ = clamp (dXYZ, -1.0, 1.0);

				float3 deltaNormal = ndS * dST[0] + ndT * dST[1];
				// deltaNormal = clamp (deltaNormal, -1.0, 1.00);
			
				// 3.) Transform the snapped UV back to world space
				// float3 snappedWorldPos = originalWorldPos + dXYZ;

				// i.posWSShininess.xyz = snappedWorldPos;
				float3 texelNormal = i.normal + deltaNormal;
				
				float3 lightDirection = normalize(i.posWSShininess.xyz - _MainLightPosition);
				float nDotL = floor(dot(texelNormal, lightDirection) * 100) / 100;

				if (nDotL < 0.4) {
					nDotL = 0.4;
				} else if (nDotL >= 0.4 && nDotL < 0.5) {
					nDotL = 0.7;
				} else {
					nDotL = 1.0;
				}

				//return half4(texelNormal, 1.0);
				//return half4(deltaNormal.x * 10, -deltaNormal.x * 10, 0, 1.0);
					// + half4(0, ddx(i.normal).y * 500, -ddx(i.normal).y * 500, 1.0);

				i.normal = texelNormal;
				return (nDotL * SampleAlbedoAlpha(i.uv, TEXTURE2D_PARAM(_MainTex, sampler_MainTex))) ;//+
					//(half4(i.normal, 1));
                //return LitPassFragmentSimple(i);
			}
			ENDHLSL
		}
	}
}
