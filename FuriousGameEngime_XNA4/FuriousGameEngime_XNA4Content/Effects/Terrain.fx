
//------- Constants --------
float4x4 View;
float4x4 Projection;
float4x4 World;
float4x4 WorldViewProjection;
float3 LightDir;

float TerrainScale;
float TerrainWidth;
float EcoPoints;

float xAmbient;
bool xEnableLighting;

//------- Texture Samplers --------
Texture TextureMap			: TEXTURE_MAP;
sampler TextureMapSampler = sampler_state { texture = <TextureMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU  = Wrap; AddressV  = Wrap; AddressW  = Wrap;};

Texture TextureR		: RED_TEXTURE;
sampler RedTextureSampler = sampler_state { texture = <TextureR> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU  = Wrap; AddressV  = Wrap; AddressW  = Wrap;};

Texture TextureG			: GREEN_TEXTURE;
sampler GreenTextureSampler = sampler_state { texture = <TextureG> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter =LINEAR; AddressU  = Wrap; AddressV  = Wrap; AddressW  = Wrap;};

Texture TextureB			: BLUE_TEXTURE;
sampler BlueTextureSampler = sampler_state { texture = <TextureB> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU  = Wrap; AddressV  = Wrap; AddressW  = Wrap;};

 struct VS_INPUT
 {
     float4 Position	: POSITION0;    
     float3 Normal		: NORMAL0;
 };

struct VS_OUTPUT
{
    float4 Position     : POSITION;
    float4 TexCoord     : TEXCOORD0;
    float3 Normal		: TEXCOORD1;
    float4 WorldPos		: TEXCOORD2;	
	float Depth         : TEXCOORD3;
};
 
// ================================================
//------- Technique: MultiTextured --------

VS_OUTPUT VertexShaderMT( VS_INPUT input)    
{
    VS_OUTPUT Output;
     
    Output.Position = mul(input.Position, WorldViewProjection);
    Output.Normal = normalize(mul(input.Normal, World));
    Output.WorldPos = mul(input.Position, World);

    Output.TexCoord.x = input.Position.x * 0.05f / TerrainScale;
    Output.TexCoord.y = input.Position.z * 0.05f / TerrainScale;
     
    Output.TexCoord.z = input.Position.x / (TerrainWidth * TerrainScale);
    Output.TexCoord.w = input.Position.z / (TerrainWidth * TerrainScale);
    Output.Depth = Output.Position.z/Output.Position.w;
    return Output;    
}

float4 PixelShaderMT(VS_OUTPUT input) : COLOR0
{
	float3 TerrainColorWeight = tex2D(TextureMapSampler, input.TexCoord.zw * TerrainScale + TerrainWidth / 2);

	float4 farColor;
	farColor = tex2D(RedTextureSampler, input.TexCoord)* TerrainColorWeight.r;
	farColor += tex2D(GreenTextureSampler, input.TexCoord)* TerrainColorWeight.g;
	farColor += tex2D(BlueTextureSampler, input.TexCoord)* TerrainColorWeight.b;
    
	float4 nearColor;
	float2 nearTextureCoords = input.TexCoord * 8;
	nearColor = tex2D(RedTextureSampler, nearTextureCoords)* TerrainColorWeight.r;
	nearColor += tex2D(GreenTextureSampler, nearTextureCoords)* TerrainColorWeight.g;
	nearColor += tex2D(BlueTextureSampler, nearTextureCoords)* TerrainColorWeight.b;
	
    float lightingFactor = 1;
	if (xEnableLighting)
        lightingFactor = saturate(saturate(dot(input.Normal, LightDir)) + xAmbient);

	float blendDistance = .99f;
	float blendWidth = 0.0099f;
	float blendFactor = clamp((input.Depth-blendDistance)/blendWidth, 0, 1);

	float4 Color = lerp(nearColor, farColor, blendFactor);
	 
	return Color;
}
 
technique MultiTextured
{
    pass Pass0
    {
        VertexShader = compile vs_1_1 VertexShaderMT();
        PixelShader = compile ps_2_0 PixelShaderMT();
    }
}