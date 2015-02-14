//
// Terrain.fx
// 
// This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
// for license details.
//

//------- Constants --------
float4x4 View				: VIEW;
float4x4 Projection			: PROJECTION;
float4x4 World				: WORLD;
float4x4 WorldViewProjection: WORLDVIEWPROJECTION;
float4x4 LightViewProjection: LIGHTVIEWPROJECTION;
float4 LightDir				: LIGHT_DIRECTION;
float4 AmbientColor			: AMBIENT_COLOR;
float AmbientPower			: AMBIENT_POWER;
float4 SpecularColor		: SPECULAR_COLOR;
float SpecularPower			: SPECULAR_POWER;
float4 DiffuseColor			: DIFFUSE_COLOR;
float4 CameraForward		: VIEW_FORWARD;
float4 CameraPos			: VIEW_POS;
float FogNear				: FOG_NEAR;
float FogFar				: FOG_FAR;
float FogDistanceRange		: FOG_DISTANCE_RANGE;
float4 FogColor				: FOG_COLOR;
float FogAltitude			: FOG_ALTITUDE;
float FogThinning			: FOG_THINNING;

float TerrainScale			: SCALE_FACTOR;
float TerrainWidth			: TERRAIN_WIDTH;

float WaterElevation		: WATER_ELEVATION;
float4 WaterColor			: WATER_COLOR;

float DepthBias = 0.0001f;

//------- Texture Samplers --------
Texture TextureMap			: TEXTURE_MAP;
sampler TextureMapSampler = sampler_state { texture = <TextureMap> ; magfilter = LINEAR; minfilter = LINEAR; 
                                                                         mipfilter = LINEAR; AddressU  = Wrap;
                                                                         AddressV  = Wrap; AddressW  = Wrap;};

Texture GrassTexture		: GRASS_TEXTURE;
sampler GrassTextureSampler = sampler_state { texture = <GrassTexture> ; magfilter = LINEAR; minfilter = LINEAR; 
                                                                         mipfilter=LINEAR; AddressU  = Wrap;
                                                                         AddressV  = Wrap; AddressW  = Wrap;};

Texture SandTexture			: SAND_TEXTURE;
sampler SandTextureSampler = sampler_state { texture = <SandTexture> ; magfilter = LINEAR; minfilter = LINEAR; 
                                                                       mipfilter =LINEAR; AddressU  = Wrap;
                                                                       AddressV  = Wrap; AddressW  = Wrap;};

Texture RockTexture			: ROCK_TEXTURE;
sampler RockTextureSampler = sampler_state { texture = <RockTexture> ; magfilter = LINEAR; minfilter = LINEAR; 
                                                                       mipfilter = LINEAR; AddressU  = Wrap;
                                                                       AddressV  = Wrap; AddressW  = Wrap;};

Texture GrassNormal			:GRASS_NORMAL;
sampler2D GrassNormalSampler : TEXUNIT1 = sampler_state
{ Texture   = (GrassNormal); magfilter = LINEAR; minfilter = LINEAR; 
                             mipfilter = LINEAR; AddressU  = Wrap;
                             AddressV  = Wrap; AddressW  = Wrap;};

Texture SandNormal			: SAND_NORMAL;
sampler2D SandNormalSampler : TEXUNIT1 = sampler_state
{ Texture   = (SandNormal); magfilter  = LINEAR; minfilter = LINEAR; 
                             mipfilter = LINEAR; AddressU  = Wrap;
                             AddressV  = Wrap; AddressW  = Wrap;};

Texture RockNormal			: ROCK_NORMAL;
sampler2D RockNormalSampler : TEXUNIT1 = sampler_state
{ Texture   = (RockNormal); magfilter = LINEAR; minfilter = LINEAR; 
                             mipfilter=LINEAR; AddressU  = Wrap;
                             AddressV  = Wrap; AddressW  = Wrap;};
                             
Texture ShadowMap				: SHADOW_MAP;	
sampler ShadowMapSampler = sampler_state { texture = <ShadowMap>; MinFilter = POINT; MagFilter = POINT;
																  MipFilter = NONE;  AddressU = Clamp;
																  AddressV = Clamp;	 AddressW  = Wrap; };
																  
		
		
// For any shader that can support shadow mapping, simply add 
// this function and call it from your pixel shader. Of course,
// you'll need to make sure this shader and material are setup to
// receive the ShadowMap sampler, and any needed parameters (e.g. LightViewProjection)														  
float4 ComputeShadowColor(float4 worldPos, float4 Color)
{
     // Find the position of this pixel in light space
    float4 lightingPosition = mul(worldPos, LightViewProjection);
    
    // Find the position in the shadow map for this pixel
    float2 ShadowTexCoord = 0.5 * lightingPosition.xy / 
                            lightingPosition.w + float2( 0.5, 0.5 );
    ShadowTexCoord.y = 1.0f - ShadowTexCoord.y;

    // Get the current depth stored in the shadow map
    float4 shadowInfo = tex2D(ShadowMapSampler, ShadowTexCoord);
    float shadowdepth = shadowInfo.r;
    float shadowOpacity = 0.5f + 0.5f * (1 - shadowInfo.g);
    
    // Calculate the current pixel depth
    // The bias is used to prevent folating point errors that occur when
    // the pixel of the occluder is being drawn
    float ourdepth = (lightingPosition.z / lightingPosition.w) - DepthBias;

    // Check to see if this pixel is in front or behind the value in the shadow map
    if ( shadowdepth < ourdepth)
    {
        // Shadow the pixel by lowering the intensity
        Color *= float4(shadowOpacity, shadowOpacity, shadowOpacity, 1);
    };
    
    return Color;
}
 
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

 VS_OUTPUT MultiTexturedVS( VS_INPUT input)    
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
 
 float4 MultiTexturedPS(VS_OUTPUT input) : COLOR0
 {
	 float3 TerrainColorWeight = tex2D(TextureMapSampler, input.TexCoord.zw);
	 
     float4 Color = tex2D(SandTextureSampler, input.TexCoord)   * TerrainColorWeight.r;
     Color += tex2D(GrassTextureSampler, input.TexCoord)		* TerrainColorWeight.g;
     Color += tex2D(RockTextureSampler, input.TexCoord)			* TerrainColorWeight.b;
     
     // Factor in normal mapping and terrain vertex normals as well in lighting of the pixel
     float lightingFactor = saturate(dot(input.Normal, -LightDir));
     
     Color.rgb *= (AmbientColor + (DiffuseColor * lightingFactor) + (SpecularColor * lightingFactor));
	 Color.a = 1.0f;

     float4 WorldPosToCam = float4(input.WorldPos - CameraPos);
     float d = length(WorldPosToCam);  
     
     // Handle fog differently if there is water, and also handle if the camera is underwater
     float waterTerrainDiff = WaterElevation - input.WorldPos.y;
	 
	 Color = ComputeShadowColor(input.WorldPos, Color);
	 
     float l = saturate((d - FogNear) / (FogFar - FogNear) / clamp(input.WorldPos.y / FogAltitude + 1, 1, FogThinning));
	 
	 return lerp(Color, FogColor, l);
 }
 
 technique MultiTextured
 {
     pass Pass0
     {
         VertexShader = compile vs_1_1 MultiTexturedVS();
         PixelShader = compile ps_2_0 MultiTexturedPS();
     }
 }
 
 
 
 // ================================================
 //------- Technique: MultiTexturedNoShadow --------

 VS_OUTPUT MultiTexturedNoShadowVS( VS_INPUT input)    
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
 
 float4 MultiTexturedNoShadowPS(VS_OUTPUT input) : COLOR0
 {
	 float3 TerrainColorWeight = tex2D(TextureMapSampler, input.TexCoord.zw);
	 
     float4 Color = tex2D(SandTextureSampler, input.TexCoord)   * TerrainColorWeight.r;
     Color += tex2D(GrassTextureSampler, input.TexCoord)		* TerrainColorWeight.g;
     Color += tex2D(RockTextureSampler, input.TexCoord)			* TerrainColorWeight.b;
     
     // Factor in normal mapping and terrain vertex normals as well in lighting of the pixel
     float lightingFactor = saturate(dot(input.Normal, -LightDir));
     
     Color.rgb *= (AmbientColor + (DiffuseColor * lightingFactor) + (SpecularColor * lightingFactor));
	 Color.a = 1.0f;

     float4 WorldPosToCam = float4(input.WorldPos - CameraPos);
     float d = length(WorldPosToCam);  
     
     // Handle fog differently if there is water, and also handle if the camera is underwater
     float waterTerrainDiff = WaterElevation - input.WorldPos.y;
	 
     float l = saturate((d - FogNear) / (FogFar - FogNear) / clamp(input.WorldPos.y / FogAltitude + 1, 1, FogThinning));
	 
	 return lerp(Color, FogColor, l);
 }
 
 technique MultiTexturedNoShadow
 {
     pass Pass0
     {
         VertexShader = compile vs_1_1 MultiTexturedNoShadowVS();
         PixelShader = compile ps_2_0 MultiTexturedNoShadowPS();
     }
 }
 

 
 
// =================================================================
// This section is for shadow mapping. This must be in any shader that
// is part of shadow mapping.

struct DrawWithShadowMap_VSIn
{
    float4 Position : POSITION0;
    float3 Normal   : NORMAL0;
};

struct CreateShadowMap_VSOut
{
    float4 Position : POSITION;
    float Depth     : TEXCOORD0;
};

// Transforms the model into light space an renders out the depth of the object
CreateShadowMap_VSOut CreateShadowMap_VertexShader(DrawWithShadowMap_VSIn input)
{
    CreateShadowMap_VSOut Out;
    Out.Position = mul(input.Position, mul(World, LightViewProjection));      
    Out.Depth = Out.Position.z / Out.Position.w;    
    return Out;
}

// Saves the depth value out to the 32bit floating point texture
float4 CreateShadowMap_PixelShader(CreateShadowMap_VSOut input) : COLOR
{ 
    return float4(input.Depth, 1, 0, 1);
}

// Technique for creating the shadow map
technique CreateShadowMap
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 CreateShadowMap_VertexShader();
        PixelShader = compile ps_2_0 CreateShadowMap_PixelShader();
    }
}
// =================================================================