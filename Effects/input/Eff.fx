sampler uImage0 : register(s0);
float alpha;
float2 texSize;
float timer;

float gauss[3][3] = {
    0.075, 0.124, 0.075,
    0.124, 0.204, 0.124,
    0.075, 0.124, 0.075
};

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0{
    float4 color = tex2D(uImage0, coords);
    if (!any(color))
        return color;
    // »Ò¶È = r*0.3 + g*0.59 + b*0.11
    float gs = dot(float3(0.3, 0.59, 0.11), color.rgb);
    return float4(gs, gs, gs, alpha);
}

float4 IdentityFunction(float2 coords : TEXCOORD0) : COLOR0{
    float4 c = tex2D(uImage0, coords);
    if (!any(c))
        return c;
    c.a = alpha;
    return c;
}

float4 Blur(float2 coords : TEXCOORD0) : COLOR0{
    float dx = 1 / texSize.x;
    float dy = 1 / texSize.y;
    float4 color = float4(0, 0, 0, 0);
    for (int i = -1; i <= 1; i++) {
        for (int j = -1; j <= 1; j++) {
            color += gauss[i + 1][j + 1] * tex2D(uImage0, float2(coords.x + dx * i, coords.y + dy * j));
        }
    }
    return 5*timer*color + tex2D(uImage0, coords);

}

technique Technique1 {
    pass Test {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }

    pass Identity {
        PixelShader = compile ps_2_0 IdentityFunction();
    }

    pass Blur {
        PixelShader = compile ps_2_0 Blur();
    }
}