
//------- Constants --------
float4x4 View;
float4x4 Projection;
float4x4 World;
float4x4 WorldViewProjection;
float3 LightDir;
float3 xLightDirection;
float3 CameraPosition;
float FogNear;
float FogFar;
float FogDistanceRange;
float4 FogColor;
float FogAltitude;
float FogThinning;

float TerrainScale;
float TerrainWidth;

float xAmbient;
bool xEnableLighting;

//------- Texture Samplers --------
Texture TextureMap			: TEXTURE_MAP;
sampler TextureMapSampler = sampler_state { texture = <TextureMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU  = Wrap; AddressV  = Wrap; AddressW  = Wrap;};

Texture GrassTexture		: GRASS_TEXTURE;
sampler GrassTextureSampler = sampler_state { texture = <GrassTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU  = Wrap; AddressV  = Wrap; AddressW  = Wrap;};

Texture SandTexture			: SAND_TEXTURE;
sampler SandTextureSampler = sampler_state { texture = <SandTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter =LINEAR; AddressU  = Wrap; AddressV  = Wrap; AddressW  = Wrap;};

Texture RoadTexture			: ROAD_TEXTURE;
sampler RoadTextureSampler = sampler_state { texture = <RoadTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU  = Wrap; AddressV  = Wrap; AddressW  = Wrap;};

//Texture GrassNormal			:GRASS_NORMAL;
//sampler2D GrassNormalSampler : TEXUNIT1 = sampler_state { Texture   = (GrassNormal); magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU  = Wrap; AddressV  = Wrap; AddressW  = Wrap;};

//Texture SandNormal			: SAND_NORMAL;
//sampler2D SandNormalSampler : TEXUNIT1 = sampler_state { Texture   = (SandNormal); magfilter  = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU  = Wrap; AddressV  = Wrap; AddressW  = Wrap;};

//Texture RockNormal			: ROCK_NORMAL;
//RockNormalSampler : TEXUNIT1 = sampler_state { Texture   = (RockNormal); magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU  = Wrap; AddressV  = Wrap; AddressW  = Wrap;};
                             
//Texture ShadowMap				: SHADOW_MAP;	
//sampler ShadowMapSampler = sampler_state { texture = <ShadowMap>; MinFilter = POINT; MagFilter = POINT; MipFilter = NONE;  AddressU = Clamp; AddressV = Clamp;	 AddressW  = Wrap; };
 
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
    return Output;    
}

float4 PixelShaderMT(VS_OUTPUT input) : COLOR0
{
	float3 TerrainColorWeight = tex2D(TextureMapSampler, input.TexCoord.zw);
	 
    float4 Color = tex2D(SandTextureSampler, input.TexCoord)   * TerrainColorWeight.r;
    Color += tex2D(GrassTextureSampler, input.TexCoord)		* TerrainColorWeight.g;
    Color += tex2D(RoadTextureSampler, input.TexCoord)			* TerrainColorWeight.b;
     
    float lightingFactor = 1;
	if (xEnableLighting)
        lightingFactor = saturate(saturate(dot(input.Normal, LightDir)) + xAmbient);
		
    //Color.rgb *= (AmbientColor + (DiffuseColor * lightingFactor) + (SpecularColor * lightingFactor));
	//Color.a = 1.0f;

    float3 WorldPosToCam = float3(input.WorldPos - CameraPosition);
    float d = length(WorldPosToCam);
	 
    float l = saturate((d - FogNear) / (FogFar - FogNear) / clamp(input.WorldPos.y / FogAltitude + 1, 1, FogThinning));
	 
	return lerp(Color, FogColor, l);
}
 
technique MultiTextured
{
    pass Pass0
    {
        VertexShader = compile vs_1_1 VertexShaderMT();
        PixelShader = compile ps_2_0 PixelShaderMT();
    }
}