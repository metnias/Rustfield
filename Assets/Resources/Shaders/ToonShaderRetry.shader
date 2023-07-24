Shader "Custom/ToonShaderRetry"
{

    Properties
    {
        //_Color ("Color", Color) = (1,1,1,1)
        _PickX ("PickX", Range(0,1)) = 0.5
        _Palette ("Palette", 2D) = "white" {}
        _RangeYTop ("RangeYTop", Range(0,1)) = 1
        _RangeYBtm ("RangeYBtm", Range(0,1)) = 0
        //_Glossiness ("Smoothness", Range(0,1)) = 0.5
        //_Metallic ("Metallic", Range(0,1)) = 0.0
    }// Subshaders allow for different behaviour and options for different pipelines and platforms
    SubShader{
        // These tags are shared by all passes in this sub shader
        Tags{"RenderPipeline" = "UniversalPipeline"}

        // Shaders can have several passes which are used to render different data about the material
        // Each pass has it's own vertex and fragment function and shader variant keywords
        Pass {
            Name "ForwardLit" // For debugging
            Tags{"LightMode" = "UniversalForward"} // Pass specific tags. 
            // "UniversalForward" tells Unity this is the main lighting pass of this shader

            HLSLPROGRAM // Begin HLSL code
            // Register our programmable stage functions
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #pragma vertex Vertex
            #pragma fragment Fragment

            // Include our code file
            #include "SubGraphs/ToonShaderForwardLitPass.hlsl"
            ENDHLSL
        }

        Pass {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ColorMask 0

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "SubGraphs/ToonShaderShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
    FallBack "UniversalRenderPipeline/Lit"
}
