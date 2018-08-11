#ifndef ES_BILLBOARD_INCLUDED
#define ES_BILLBOARD_INCLUDED

#include "LWRP/ShaderLibrary/Input.hlsl"

float remap(float value, float low1, float high1, float low2, float high2) {
	return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
}

float4 Billboard(float4 vertex, float2 scale) {
	return mul(
		UNITY_MATRIX_P,
		mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 0.1))
			+ float4(vertex.x, vertex.y, 0.0, 0.0)
			* float4(scale.x, scale.y, 1.0, 1.0)
	);
}

// float3 TexelSnap(float2 uv, float3 position, float3 normal) {
// 	// 1.) Calculate how much the texture UV coords need to
// 	//     shift to be at the center of the nearest texel.
// 	float2 originalUV = uv;
// 	float2 centerUV = floor(originalUV * _MainTex_TexelSize.zw)/_MainTex_TexelSize.zw + (_MainTex_TexelSize.xy/2.0);
// 	float2 dUV = centerUV - originalUV;

// 	// 2a.) Get this fragment's world position
// 	// float3 originalWorldPos = position;

// 	// 2b.) Calculate how much the texture coords vary over fragment space.
// 	//      This essentially defines a 2x2 matrix that gets
// 	//      texture space (UV) deltas from fragment space (ST) deltas
// 	// Note: I call fragment space (S,T) to disambiguate.
// 	float2 dUVdS = ddx(originalUV);
// 	float2 dUVdT = ddy(originalUV);

// 	// 2c.) Invert the fragment from texture matrix
// 	float2x2 dSTdUV = float2x2(dUVdT[1], -dUVdS[1], -dUVdT[0], dUVdS[0]) * (1 / (dUVdS[0] * dUVdT[1] - dUVdS[1] * dUVdT[0]));

// 	// 2d.) Convert the UV delta to a fragment space delta
// 	// this was originally mul(dUV, dSTdUV), which was causing the pillowing
// 	float2 dST = mul(dUV, dSTdUV); // dST = How many fragments until the center of the texel
// 	// dd[x|y](normal) is normal variation per fragment

// 	// 2e.) Calculate how much the world coords vary over fragment space.
// 	float3 ndS = ddx(normal);
// 	float3 ndT = ddy(normal);

// 	// 2f.) Finally, convert our fragment space delta to a world space delta
// 	// And be sure to clamp it to SOMETHING in case the derivative calc went insane
// 	// Here I clamp it to -1 to 1 unit in unity, which should be orders of magnitude greater
// 	// than the size of any texel.
// 	// dXYZ = clamp (dXYZ, -1.0, 1.0);

// 	float3 deltaNormal = ndS * dST[0] + ndT * dST[1];
// 	// deltaNormal = clamp (deltaNormal, -1.0, 1.00);

// 	// 3.) Transform the snapped UV back to world space
// 	float3 texelNormal = normal + deltaNormal;

// 	return texelNormal;
// }

float2 TexelSnapComponents(float2 uv, float2 texelSize, float3 position) {
	float2 originalUV = uv;
	float2 centerUV = floor(originalUV * texelSize)/texelSize + (_MainTex_TexelSize.xy/2.0);
	float2 dUV = centerUV - originalUV;

	// Get the change in UVs across fragments
	float2 dUVdS = ddx(originalUV);
	float2 dUVdT = ddy(originalUV);

	// Invert the matrix created by the ddx/y of the UVs
	float2x2 dSTdUV = float2x2(dUVdT[1], -dUVdS[1], -dUVdT[0], dUVdS[0]) * (1 / (dUVdS[0] * dUVdT[1] - dUVdS[1] * dUVdT[0]));

	// Convert the UV delta to a fragment space delta
	return mul(dUV, dSTdUV);
}

float4 TexelSnap4(float4 value, float2 components) {
	return value + (ddx(value) * components.x + ddy(value) * components.y);
}

float3 TexelSnap3(float3 value, float2 components) {
	return value + (ddx(value) * components.x + ddy(value) * components.y);
}

float2 TexelSnap2(float2 value, float2 components) {
	return value + (ddx(value) * components.x + ddy(value) * components.y);
}

float TexelSnap1(float value, float2 components) {
	return value + (ddx(value) * components.x + ddy(value) * components.y);
}

#endif