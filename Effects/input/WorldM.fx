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
float2 targetRes;

float twistInten;
float twistRadius;
float twistWidth;
float2 twistCenter;
float2 texSize;

float extractThreshold;

float4 fillColor;

float2 dispCenter;
float dispTimer;
Texture2D dispMap;
float dispInten;

float vigStartPercent;
float vigInten;

sampler2D uDisp = sampler_state
{
    Texture = <dispMap>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = wrap;
    AddressV = wrap;
};

float4 SimpleVignette(float2 coords : TEXCOORD0) : COLOR0
{
    float4 c = tex2D(uImage0, coords);
    float dist = length(coords - float2(0.5, 0.5));
    if (vigInten <= 0 || 2 * dist <= vigStartPercent)
        return c;

    float darkFactor = 1 - (2 * dist - vigStartPercent) * vigInten;
    if (darkFactor <= 0)
        return float4(0, 0, 0, 0);
    return darkFactor * c;
}

float4 Extract(float2 coords : TEXCOORD0) : COLOR0
{
    float4 c = tex2D(uImage0, coords);
    if (c.r * 0.4 + c.g * 0.4 + c.b * 0.2 >= extractThreshold)
        return c;
    return float4(0, 0, 0, 0);
}

float4 FillOnThreshold(float2 coords : TEXCOORD0) : COLOR0
{
    float4 rawC = Extract(coords);
    if (rawC.a == 0)
        return rawC;
    return fillColor;
}

float4 CenterDisplacement(float2 coords : TEXCOORD0) : COLOR0
{
    float realX = coords.x * texSize.x - dispCenter.x;
    float realY = coords.y * texSize.y - dispCenter.y;
    
    float angle = atan(realY / realX);
    if (realX < 0)
        angle += 3.14159265359;
    if (angle < 0)
        angle += 6.28318530718;
    
    float4 dispC = tex2D(uDisp, float2(angle /6.28318530718, dispTimer));
    
    float inten = (dispC.r > 0.7 ? ((dispC.r - 0.7) / 0.3) : 0);
    if (inten > 0.5)
        inten = (1 - (1 - inten) * (1 - inten)) * dispInten;
    else
        inten *= inten * dispInten;
        
    if (length(float2(realX, realY)) < inten)
        return float4(0, 0, 0, 0);
    
    return tex2D(uImage0, float2(coords.x - cos(angle) * inten / texSize.x, coords.y - sin(angle) * inten / texSize.y));

}

float4 CenterTwist(float2 coords : TEXCOORD0) : COLOR0
{
    float2 originalCoord = float2(coords.x * texSize.x, coords.y * texSize.y);
    float diffX = abs(originalCoord.x - twistCenter.x);
    float diffY = abs(originalCoord.y - twistCenter.y);
    float dist = sqrt(diffX * diffX + diffY*diffY);
    
    if (dist > twistRadius + twistWidth || dist < twistRadius - twistWidth)
        return tex2D(uImage0, coords);
    
    float inten = (twistWidth - abs(dist - twistRadius)) / twistWidth * twistInten;
    
    return tex2D(uImage0, float2(coords.x + inten * diffX / texSize.x,
        coords.y + inten * diffY / texSize.y));
}

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
    float dx = 1 / targetRes.x;
    float4 color = float4(0, 0, 0, 0);
    
    for (int i = -2; i <= 2; i++)
    {
            float4 addColor = tex2D(uImage0, float2(coords.x + dx * i, coords.y));
            if (addColor.r * 0.4 + addColor.g * 0.4 + addColor.b * 0.2 >= blurThreshold)
                color += gaussOneD[i + 2] * addColor;
    }
    
    
    color *= bloomInten;
    color.a = 1;
    return color;
}

float4 BlurThresholdV(float2 coords : TEXCOORD0) : COLOR0
{
    float dy = 1 / targetRes.y;
    float4 color = float4(0, 0, 0, 0);
    
    for (int i = -2; i <= 2; i++)
    {
        float4 addColor = tex2D(uImage0, float2(coords.x, coords.y + dy * i));
        if (addColor.r * 0.4 + addColor.g * 0.4 + addColor.b * 0.2 >= blurThreshold)
            color += gaussOneD[i + 2] * addColor;
    }
    
    color *= bloomInten;
    color.a = 1;
    return color;
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

    pass CenterTwist
    {
        PixelShader = compile ps_2_0 CenterTwist();
    }

    pass Extract
    {
        PixelShader = compile ps_2_0 Extract();
    }

    pass FillOnThreshold
    {
        PixelShader = compile ps_2_0 FillOnThreshold();
    }

    pass CenterDisplacement
    {
        PixelShader = compile ps_2_0 CenterDisplacement();
    }

    pass SimpleVignette
    {
        PixelShader = compile ps_2_0 SimpleVignette();
    }
}
