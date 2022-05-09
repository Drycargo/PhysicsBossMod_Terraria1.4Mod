sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float uOpacity;
float3 uSecondaryColor;
float uTime;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uImageOffset;
float uIntensity;
float uProgress;
float2 uDirection;
float2 uZoom;
float2 uImageSize0;
float2 uImageSize1;

float2 vibInten;
float blurInten;

float bloomInten;
float blurThreshold;

float gauss[3][3] =
{
    0.075, 0.124, 0.075,
    0.124, 0.204, 0.124,
    0.075, 0.124, 0.075
};

float gaussOneD[5] =
    { 0.054, 0.244, 0.403, 0.244, 0.054};


float4 GaussBlur(float2 coords : TEXCOORD0) : COLOR0
{
    float dx = 1 / uScreenResolution.x;
    float dy = 1 / uScreenResolution.y;
    float4 color = float4(0, 0, 0, 0);
    for (int i = -1; i <= 1; i++)
    {
        for (int j = -1; j <= 1; j++)
        {
            color += gauss[i + 1][j + 1] * tex2D(uImage0, float2(coords.x + dx * i, coords.y + dy * j));
        }
    }
    return blurInten * color + (1 - blurInten) * tex2D(uImage0, coords);

}

float4 BlurOnThreshold(float2 coords : TEXCOORD0) : COLOR0
{
    float dx = 1 / uScreenResolution.x;
    float dy = 1 / uScreenResolution.y;
    float4 color = float4(0, 0, 0, 0);
    
    for (int i = -1; i <= 1; i++)
    {
        for (int j = -1; j <= 1; j++)
        {
            if (i!=0 && j != 0)
                continue;
            float4 addColor = tex2D(uImage0, float2(coords.x + dx * i, coords.y + dy * j));
            if (addColor.r * 0.4 + addColor.g * 0.4 + addColor.b * 0.2 >= blurThreshold)
                color += gauss[i + 1][j + 1] * addColor;
        }
    }
    
    
    return bloomInten * color;
}

float4 BlurThresholdH(float2 coords : TEXCOORD0) : COLOR0
{
    float dx = 1 / uScreenResolution.x;
    float4 color = float4(0, 0, 0, 0);
    
    for (int i = -2; i <= 2; i++)
    {
            float4 addColor = tex2D(uImage0, float2(coords.x + dx * i, coords.y));
            if (addColor.r * 0.4 + addColor.g * 0.4 + addColor.b * 0.2 >= blurThreshold)
                color += gaussOneD[i + 2] * addColor;
    }
    
    
    return bloomInten * color;
}

float4 BlurThresholdV(float2 coords : TEXCOORD0) : COLOR0
{
    float dy = 1 / uScreenResolution.y;
    float4 color = float4(0, 0, 0, 0);
    
    for (int i = -2; i <= 2; i++)
    {
        float4 addColor = tex2D(uImage0, float2(coords.x, coords.y + dy * i));
        if (addColor.r * 0.4 + addColor.g * 0.4 + addColor.b * 0.2 >= blurThreshold)
            color += gaussOneD[i + 2] * addColor;
    }
    
    
    return bloomInten * color;
}

float4 Inverse(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    if (!any(color))
        return color;
    return float4(float3(1,1,1)-color.rgb, color.a);
}

float4 Shake(float2 coords : TEXCOORD0) : COLOR0
{
    return tex2D(uImage0, float2(coords.x + vibInten.x / uScreenResolution.x, coords.y + vibInten.y / uScreenResolution.y));
}

technique Technique1
{
    pass Inverse
    {
        PixelShader = compile ps_2_0 Inverse();
    }

    pass Shake
    {
        PixelShader = compile ps_2_0 Shake();
    }

    pass GaussBlur
    {
        PixelShader = compile ps_2_0 GaussBlur();
    }

    pass BlurOnThreshold
    {
        PixelShader = compile ps_2_0 BlurOnThreshold();
    }

    pass BlurThresholdH
    {
        PixelShader = compile ps_2_0 BlurThresholdH();
    }

    pass BlurThresholdV
    {
        PixelShader = compile ps_2_0 BlurThresholdV();
    }
}
