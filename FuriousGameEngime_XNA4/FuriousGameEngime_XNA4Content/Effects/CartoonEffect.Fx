//-----------------------------------------------------------------------------
// EnvironmentMap.fx
//
// Microsoft Game Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


//------- Universal Variables --------

// Camera settings.
float4x4 World;
float4x4 View;
float4x4 Projection;

float3 LightDirection = normalize(float3(1, 1, 1));
float3 LightColor = float3(0.7, 0.5, 0.3);
float3 AmbientColor = float3(0.1, 0.15, 0.20);
bool enableLighting;

// Settings controlling the Toon lighting technique.
float ToonThresholds[2] = { .9, .5 };
float ToonBrightnessLevels[3] = { 0.8, 0.5, 0.4 };

texture Texture;

// Universal texture wrapper
sampler TextureSampler = sampler_state
{
    Texture = (Texture);
	    
	MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    
    AddressU = Wrap;
    AddressV = Wrap;
};


//------- Technique: Textured --------

struct TexVertexToPixel
{
    float4 Position     : POSITION;    
    float4 Color        : COLOR0;
    float LightingFactor: TEXCOORD0;
    float2 TextureCoords: TEXCOORD1;
};

struct TexPixelToFrame
{
    float4 Color : COLOR0;
};

// also used in the multi-textured technique
texture xTexture0;
sampler TextureSampler0 = sampler_state { texture = <xTexture0> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};texture xTexture1;

TexVertexToPixel TexturedVS( float4 inPos : POSITION, float3 inNormal: NORMAL, float2 inTexCoords: TEXCOORD0)
{    
    TexVertexToPixel Output = (TexVertexToPixel)0;
    float4x4 preViewProjection = mul (View, Projection);
    float4x4 preWorldViewProjection = mul (World, preViewProjection);
    
    Output.Position = mul(inPos, preWorldViewProjection);    
    Output.TextureCoords = inTexCoords;
    
    float3 Normal = normalize(mul(normalize(inNormal), World));    
    Output.LightingFactor = 1;
    if (enableLighting)
        Output.LightingFactor = saturate(dot(Normal, -LightDirection));
    
    return Output;    
}

TexPixelToFrame TexturedPS(TexVertexToPixel PSIn)
{
    TexPixelToFrame Output = (TexPixelToFrame)0;        
    
    Output.Color = tex2D(TextureSampler0, PSIn.TextureCoords);
    Output.Color.rgb *= saturate(PSIn.LightingFactor + AmbientColor);

    return Output;
}

technique Textured
{
    pass Pass0
    {
        VertexShader = compile vs_1_1 TexturedVS();
        PixelShader = compile ps_2_0 TexturedPS();
    }
}

//------- Technique: Multi-Textured --------

//Used in multi-Texture Technique
sampler TextureSampler1 = sampler_state { texture = <xTexture1> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};texture xTexture2;
sampler TextureSampler2 = sampler_state { texture = <xTexture2> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};texture xTexture3;
sampler TextureSampler3 = sampler_state { texture = <xTexture3> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};


struct MTVertexToPixel
{
    float4 Position         : POSITION;    
    float4 Color            : COLOR0;
    float3 Normal            : TEXCOORD0;
    float2 TextureCoords    : TEXCOORD1;
    float4 LightDirection    : TEXCOORD2;
    float4 TextureWeights    : TEXCOORD3;
	float Depth            : TEXCOORD4;
};

MTVertexToPixel MultiTexturedVS( float4 inPos : POSITION, float3 inNormal: NORMAL, float2 inTexCoords: TEXCOORD0, float4 inTexWeights: TEXCOORD1)
{    
    MTVertexToPixel Output = (MTVertexToPixel)0;
    float4x4 preViewProjection = mul (View, Projection);
    float4x4 preWorldViewProjection = mul (World, preViewProjection);
    
    Output.Position = mul(inPos, preWorldViewProjection);
    Output.Normal = mul(normalize(inNormal), World);
    Output.TextureCoords = inTexCoords;
    Output.LightDirection.xyz = -LightDirection;
    Output.LightDirection.w = 1;    
    Output.TextureWeights = inTexWeights;
    Output.Depth = Output.Position.z/Output.Position.w;

    return Output;    
}

struct MTPixelToFrame
{
    float4 Color : COLOR0;
};

MTPixelToFrame MultiTexturedPS(MTVertexToPixel PSIn)
{
	//float4 color = tex2D(TextureSampler, input.TextureCoordinate);
    
    //float light;
	
    MTPixelToFrame Output = (MTPixelToFrame)0;        
    
    float lightingFactor = 1;
    if (enableLighting)
        lightingFactor = saturate(saturate(dot(PSIn.Normal, PSIn.LightDirection)) + AmbientColor);

	float blendDistance = .99f;
	float blendWidth = 0.005f;
	float blendFactor = clamp((PSIn.Depth-blendDistance)/blendWidth, 0, 1);

	float4 farColor;
	farColor = tex2D(TextureSampler0, PSIn.TextureCoords)*PSIn.TextureWeights.x;
	farColor += tex2D(TextureSampler1, PSIn.TextureCoords)*PSIn.TextureWeights.y;
	farColor += tex2D(TextureSampler2, PSIn.TextureCoords)*PSIn.TextureWeights.z;
	farColor += tex2D(TextureSampler3, PSIn.TextureCoords)*PSIn.TextureWeights.w;
    
	float4 nearColor;
	float2 nearTextureCoords = PSIn.TextureCoords*3;
	nearColor = tex2D(TextureSampler0, nearTextureCoords)*PSIn.TextureWeights.x;
	nearColor += tex2D(TextureSampler1, nearTextureCoords)*PSIn.TextureWeights.y;
	nearColor += tex2D(TextureSampler2, nearTextureCoords)*PSIn.TextureWeights.z;
	nearColor += tex2D(TextureSampler3, nearTextureCoords)*PSIn.TextureWeights.w;

	//if (input.LightAmount > ToonThresholds[0])
	//	lightingFactor = ToonBrightnessLevels[0];
    //else if (input.LightAmount > ToonThresholds[1])
    //    lightingFactor = ToonBrightnessLevels[1];
    //else
    //    lightingFactor = ToonBrightnessLevels[2];

	Output.Color = lerp(nearColor, farColor, blendFactor);
	Output.Color *= lightingFactor;
                
    //color.rgb *= light;
    

	//Original Code
	//Output.Color = tex2D(TextureSampler0, PSIn.TextureCoords)*PSIn.TextureWeights.x;
	//Output.Color += tex2D(TextureSampler1, PSIn.TextureCoords)*PSIn.TextureWeights.y;
	//Output.Color += tex2D(TextureSampler2, PSIn.TextureCoords)*PSIn.TextureWeights.z;
	//Output.Color += tex2D(TextureSampler3, PSIn.TextureCoords)*PSIn.TextureWeights.w;
	//Output.Color *= lightingFactor;

    //return color;
    return Output;
}

technique MultiTextured
{
    pass Pass0
    {
        VertexShader = compile vs_1_1 MultiTexturedVS();
        PixelShader = compile ps_2_0 MultiTexturedPS();
    }
}

//------- Technique: Hardware Instancing(Diffuse) --------


struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
    float2 TextureCoordinate : TEXCOORD0;
	float4 Position : POSITION0;
    float LightAmount : TEXCOORD1;
};

// Vertex shader helper function shared between the two techniques.
VertexShaderOutput VertexShaderToon(VertexShaderInput input, float4x4 instanceTransform)
{
    VertexShaderOutput output;

    // Apply the world and camera matrices to compute the output position.
    float4 worldPosition = mul(input.Position, instanceTransform);
    
	// Compute lighting
    float3 worldNormal = mul(input.Normal, instanceTransform);

    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	
	// Copy across the input texture coordinate.    
	output.TextureCoordinate = input.TextureCoordinate;

	output.LightAmount = dot(worldNormal, LightDirection);

	//Not used
    //float diffuseAmount = max(-dot(worldNormal, LightDirection), 0);    
    //float3 lightingResult = saturate(diffuseAmount * DiffuseLight + AmbientLight);
    
	//Not used
    //output.Color = float4(lightingResult, 1);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 color = tex2D(TextureSampler, input.TextureCoordinate);
    
    float light;

    if (input.LightAmount > ToonThresholds[0])
        light = ToonBrightnessLevels[0];
    else if (input.LightAmount > ToonThresholds[1])
        light = ToonBrightnessLevels[1];
    else
        light = ToonBrightnessLevels[2];
                
    color.rgb *= light;
    
    return color;
}

// Hardware instancing reads the per-instance world transform from a secondary vertex stream.
VertexShaderOutput HardwareInstancingVertexShader(VertexShaderInput input, float4x4 instanceTransform : BLENDWEIGHT)
{
    return VertexShaderToon(input, mul(World, transpose(instanceTransform)));
}

technique DiffuseMapping
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 HardwareInstancingVertexShader();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}


//------- Technique: Normal Depth --------

// This texture contains normals (in the color channels) and depth (in alpha)
// for the main scene image. Differences in the normal and depth data are used
// to detect where the edges of the model are.
texture NormalDepthTexture;

// Output structure for the vertex shader that renders normal and depth information.
struct NormalDepthVertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

// Simple pixel shader for rendering the normal and depth information.
float4 NormalDepthPixelShader(float4 color : COLOR0) : COLOR0
{
    return color;
}

// Alternative vertex shader outputs normal and depth values, which are then
// used as an input for the edge detection filter in PostprocessEffect.fx.
NormalDepthVertexShaderOutput NormalDepthVertexShader(VertexShaderInput input, float4x4 instanceTransform)
{
    NormalDepthVertexShaderOutput output;

    // Apply camera matrices to the input position.
    output.Position = mul(mul(mul(input.Position, instanceTransform), View), Projection);
    
    float3 worldNormal = mul(input.Normal, instanceTransform);

    // The output color holds the normal, scaled to fit into a 0 to 1 range.
    output.Color.rgb = (worldNormal + 1) / 2;

    // The output alpha holds the depth, scaled to fit into a 0 to 1 range.
    output.Color.a = output.Position.z / output.Position.w;
    
    return output;    
}

// Hardware instancing reads the per-instance world transform from a secondary vertex stream.
NormalDepthVertexShaderOutput HardwareInstancingNormalDepthVertexShader(VertexShaderInput input, float4x4 instanceTransform : BLENDWEIGHT)
{
    return NormalDepthVertexShader(input, mul(World, transpose(instanceTransform)));
}

technique NormalDepthMapping
{
	pass P0
    {
        VertexShader = compile vs_3_0 HardwareInstancingNormalDepthVertexShader();
        PixelShader = compile ps_3_0 NormalDepthPixelShader();
    }
}

//------- Technique: Edge Detection --------

// Settings controlling the edge detection filter.
float EdgeWidth = 1;
float EdgeIntensity = 1;

// How sensitive should the edge detection be to tiny variations in the input data?
// Smaller settings will make it pick up more subtle edges, while larger values get
// rid of unwanted noise.
float NormalThreshold = 0.5;
float DepthThreshold = 0.1;

// How dark should the edges get in response to changes in the input data?
float NormalSensitivity = 1;
float DepthSensitivity = 10;

// This texture contains the main scene image, which the edge detection
// and/or sketch filter are being applied over the top of.
texture SceneTexture;

// Pass in the current screen resolution.
float2 ScreenResolution;

sampler NormalDepthSampler : register(s1) = sampler_state
{
    Texture = (NormalDepthTexture);
    
    MinFilter = Linear;
    MagFilter = Linear;
    
    AddressU = Clamp;
    AddressV = Clamp;
};

// Pixel shader applies the edge detection.
float4 PixelShaderCommon(float2 texCoord : TEXCOORD0) : COLOR0
{
    // Look up the original color from the main scene.
    float3 scene = tex2D(TextureSampler, texCoord);

    // Look up four values from the normal/depth texture, offset along the
    // four diagonals from the pixel we are currently shading.
    float2 edgeOffset = EdgeWidth / ScreenResolution;
        
    float4 n1 = tex2D(NormalDepthSampler, texCoord + float2(-1, -1) * edgeOffset);
    float4 n2 = tex2D(NormalDepthSampler, texCoord + float2( 1,  1) * edgeOffset);
    float4 n3 = tex2D(NormalDepthSampler, texCoord + float2(-1,  1) * edgeOffset);
    float4 n4 = tex2D(NormalDepthSampler, texCoord + float2( 1, -1) * edgeOffset);

    // Work out how much the normal and depth values are changing.
    float4 diagonalDelta = abs(n1 - n2) + abs(n3 - n4);

    float normalDelta = dot(diagonalDelta.xyz, 1);
    float depthDelta = diagonalDelta.w;
        
    // Filter out very small changes, in order to produce nice clean results.
    normalDelta = saturate((normalDelta - NormalThreshold) * NormalSensitivity);
    depthDelta = saturate((depthDelta - DepthThreshold) * DepthSensitivity);

    // Does this pixel lie on an edge?
    float edgeAmount = saturate(normalDelta + depthDelta) * EdgeIntensity;
        
    // Apply the edge detection result to the main scene color.
    scene *= (1 - edgeAmount);

    return float4(scene, 1);
}

// Compile the pixel shader for doing edge detection without any sketch effect.
technique EdgeDetect
{
    pass P0
    {
        PixelShader = compile ps_2_0 PixelShaderCommon();
    }
}