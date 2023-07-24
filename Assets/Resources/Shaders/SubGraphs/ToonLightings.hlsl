#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

void AddAdditionalLights_float(float Smoothness, float3 WorldPosition, float3 WorldNormal, float3 WorldView,
    float MainDiffuse, float MainSpecular, float3 MainColor,
    out float SumDiffuse, out float SumSpecular, out float3 SumColor)
{
    SumDiffuse = MainDiffuse;
    SumSpecular = MainSpecular;
    SumColor = MainColor * (MainDiffuse + MainSpecular);

#ifndef SHADERGRAPH_PREVIEW
    int pixelLightCount = GetAdditionalLightsCount();
    for (int i = 0; i < pixelLightCount; ++i)
    {
        Light light = GetAdditionalLight(i, WorldPosition);
        half NdotL = saturate(dot(WorldNormal, light.direction));
        half atten = light.distanceAttenuation * light.shadowAttenuation;
        half thisDiffuse = atten * NdotL;
        half thisSpecular = LightingSpecular(thisDiffuse, light.direction, WorldNormal, WorldView, 1, Smoothness);
        SumDiffuse += thisDiffuse;
        SumSpecular + thisSpecular;
        SumColor += light.color * (thisDiffuse + thisSpecular);
    }
#endif
    
    half total = SumDiffuse + SumSpecular;
    // If no light touches this pixel, set the color to the main light's color
    SumColor = total <= 0 ? MainColor : (SumColor / total);
}

#endif