// This file contains the vertex and fragment functions for the forward lit pass
// This is the shader pass that computes visible colors for a material
// by reading material, light, shadow, etc. data

// Pull in URP library functions and our own common functions
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

// Textures
TEXTURE2D(_Palette); SAMPLER(sampler_Palette); // RGB = albedo, A = alpha
half _PickX;
half _RangeYTop;
half _RangeYBtm;

// This attributes struct receives data about the mesh we're currently rendering
// Data is automatically placed in fields according to their semantic
struct Attributes {
	float3 positionOS : POSITION; // Position in object space
	float3 normalOS : NORMAL;
	//float2 uv : TEXCOORD0;
};

// This struct is output by the vertex function and input to the fragment function.
// Note that fields will be transformed by the intermediary rasterization stage
struct Interpolators {
	// This value should contain the position in clip space (which is similar to a position on screen)
	// when output from the vertex function. It will be transformed into pixel position of the current
	// fragment on the screen when read from the fragment function
	float4 positionCS : SV_POSITION;
	//float2 uv : TEXCOORD0;
	float3 positionWS : TEXCOORD1;
	float3 normalWS : TEXCOORD2;
};

// The vertex function. This runs for each vertex on the mesh.
// It must output the position on the screen each vertex should appear at,
// as well as any data the fragment function will need
Interpolators Vertex(Attributes input) {
	Interpolators output;

	VertexPositionInputs posnInputs = GetVertexPositionInputs(input.positionOS);
	VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);

	output.positionCS = posnInputs.positionCS;
	//output.uv = TRANSFORM_TEX(input.uv, _ColorMap);
	output.normalWS = normInputs.normalWS;
	output.positionWS = posnInputs.positionWS;

	return output;
}

// The fragment function. This runs once per fragment, which you can think of as a pixel on the screen
// It must output the final color of this pixel
float4 Fragment(Interpolators input) : SV_TARGET {

	//float2 uv = input.uv;
	SurfaceData surfaceInput = (SurfaceData)0;
	surfaceInput.albedo = (1,1,1);
	surfaceInput.alpha = 1;

	InputData inputData = (InputData)0;
	inputData.positionWS = input.positionWS;
	inputData.normalWS = normalize(input.normalWS);
	inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
	inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);

	float4 lightResult = UniversalFragmentPBR(inputData, surfaceInput);
	//return lightResult;
	half remapped = _RangeYBtm + (_RangeYTop - _RangeYBtm) * (lightResult.r);
	float4 colorSample = SAMPLE_TEXTURE2D(_Palette, sampler_Palette, (_PickX, remapped));
	return colorSample;
}

