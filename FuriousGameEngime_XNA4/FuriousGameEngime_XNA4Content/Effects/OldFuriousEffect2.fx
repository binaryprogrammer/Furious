float4x4 World;
float4x4 View;
float4x4 Projection; //shadow
float4x4 LightViewProj; //

// ***** Light properties *****
float3 LightPosition; //normal
float4 LightColor; //normal
float4 AmbientLightColor;
//*****************************

// ***** material properties *****
// output from phong specular will be scaled by this amount
float Shininess; //normal

// specular exponent from phong lighting model.  controls the "tightness" of specular highlights.
float SpecularPower; //normal
// *******************************

float DepthBias = 0.001f; //Shadow // how close it is drawn on top of the textures... Z-fighting

texture2D NormalMap;
sampler2D NormalMapSampler = sampler_state { Texture = <NormalMap>; MinFilter = linear; MagFilter = linear; MipFilter = linear; };

texture2D DiffuseMap;
sampler2D DiffuseTextureSampler = sampler_state { Texture = <DiffuseMap>; MinFilter = point; MagFilter = point; MipFilter = point; };

texture ShadowMap;
sampler ShadowMapSampler = sampler_state { Texture = <ShadowMap>; };

struct VertexShaderInput
{
    float4 Position	: POSITION0;
	float2 TexCoord	: TEXCOORD0;
	float3 Normal	: NORMAL0;
	float3 Binormal	: BINORMAL0;
    float3 Tangent	: TANGENT0;
};

// output from the vertex shader, and input to the pixel shader. LightDirection and viewDirection are in world space.
// NOTE: even though the tangentToWorld matrix is only marked with TEXCOORD4, it will actually take TEXCOORD4, 5, and 6.
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float3 Normal   : TEXCOORD1;
	float4 WorldPosition : TEXCOORD2;
	float3 LightDirection : TEXCOORD3;
	float3 ViewDirection    : TEXCOORD4;
    float3x3 TangentToWorld    : TEXCOORD5;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    
	// transform the position into projection space using the world, view, and projection matricies
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.WorldPosition = worldPosition;
	
	// pass the texture coordinate through without additional processing
    output.TexCoord = input.TexCoord;

    // Transform the model's verticies and normal
    output.Normal =  normalize(mul(input.Normal, World));

	// calculate the light direction ( from the surface to the light ), which is not normalized and is in world space
    output.LightDirection = LightPosition - worldPosition; //normal
	//output.LightDirection = worldPosition - LightPosition; //normal
        
    // similarly, calculate the view direction, from the eye to the surface. Not normalized, in world space.
    float3 cameraPosition = mul(-View._m30_m31_m32, transpose(View)); // might just pass in the camera's Forward
    output.ViewDirection = worldPosition - cameraPosition; //normal
    
    // calculate tangent space to world space matrix using the world space tangent, binormal, and normal as basis vectors.
	//the pixel shader will normalize these in case the world matrix has scaling.
    output.TangentToWorld[0] = mul(input.Tangent, World); //normal
    output.TangentToWorld[1] = mul(input.Binormal, World); //normal
    output.TangentToWorld[2] = mul(input.Normal, World); //normal
	
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // look up the normal from the normal map, and transform from tangent space into world space using the matrix created above.  
	// normalize the result in case the matrix contains scaling.
    float3 normalFromMap = tex2D(NormalMapSampler, input.TexCoord); //normal
    normalFromMap = mul(normalFromMap, input.TangentToWorld); //normal
    normalFromMap = normalize(normalFromMap); //normal

	// clean up our inputs a bit
    input.ViewDirection = normalize(input.ViewDirection); //normal
    input.LightDirection = normalize(input.LightDirection); //normal
		    
    // use the normal we looked up to do phong diffuse style lighting.    
    float nDotL = max(dot(normalFromMap, input.LightDirection), 0); //normal
    float4 diffuse = LightColor * nDotL; //normal
		    
    // use phong to calculate specular highlights: reflect the incoming light vector off the normal, 
	// and use a dot product to see how "similar" the reflected vector is to the view vector.    
    float3 reflectedLight = reflect(input.LightDirection, normalFromMap); //normal
    float rDotV = max(dot(reflectedLight, input.ViewDirection), 0); //normal
    float4 specular = Shininess * LightColor * pow(rDotV, SpecularPower); //normal // for specMap use another texture in place of Shininess

	float4 diffuseTexture = tex2D(DiffuseTextureSampler, input.TexCoord); //normal

    // Intensity based on the direction of the light
    float diffuseIntensity = saturate(dot(input.LightDirection, input.Normal)); // shadow how dark
    // Final diffuse color with ambient color added
    //diffuse += diffuseIntensity * diffuseTexture + LightColor; // shadow ambient color

	// Find the position of this pixel in light space
    float4 lightingPosition = mul(input.WorldPosition, LightViewProj); //Shadow
    
    // Find the position in the shadow map for this pixel
    float2 ShadowTexCoord = 0.5 * lightingPosition.xy / lightingPosition.w + float2( 0.5, 0.5 ); 
    ShadowTexCoord.y = 1.0f - ShadowTexCoord.y;

    // Get the current depth stored in the shadow map
    float shadowdepth = tex2D(ShadowMapSampler, ShadowTexCoord).r; //shadow
    
    // Calculate the current pixel depth. The bias is used to prevent folating point errors that occur when the pixel of the occluder is being drawn
    float ourdepth = (lightingPosition.z / lightingPosition.w) - DepthBias; // shadow
    	
	diffuse = (diffuse + LightColor) * diffuseTexture + specular; //normal //was returned

    // Check to see if this pixel is in front or behind the value in the shadow map
    if (shadowdepth < ourdepth)
    {
        // Shadow the pixel by lowering the intensity
        diffuse *= float4(0.5,0.5,0.5,0);
    };

    // return the combined result.
    return diffuse;
}

technique DrawModel
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

//*********** Normal Mapping *****************

struct VS_INPUT
{
    float4 position            : POSITION0;
    float2 texCoord            : TEXCOORD0;
    float3 normal            : NORMAL0;    
    float3 binormal            : BINORMAL0;
    float3 tangent            : TANGENT0;
};

// output from the vertex shader, and input to the pixel shader.
// lightDirection and viewDirection are in world space.
// NOTE: even though the tangentToWorld matrix is only marked 
// with TEXCOORD3, it will actually take TEXCOORD3, 4, and 5.
struct VS_OUTPUT
{
    float4 position            : POSITION0;
    float2 texCoord            : TEXCOORD0;
    float3 lightDirection    : TEXCOORD1;
    float3 viewDirection    : TEXCOORD2;
    float3x3 tangentToWorld    : TEXCOORD3;
};

VS_OUTPUT NormalVertexShaderFunction( VS_INPUT input )
{
    VS_OUTPUT output;
    
    // transform the position into projection space
    float4 worldSpacePos = mul(input.position, World);
    output.position = mul(worldSpacePos, View);
    output.position = mul(output.position, Projection);
    
    // calculate the light direction ( from the surface to the light ), which is not
    // normalized and is in world space
    output.lightDirection = LightPosition - worldSpacePos;
        
    // similarly, calculate the view direction, from the eye to the surface.  not
    // normalized, in world space.
    float3 eyePosition = mul(-View._m30_m31_m32, transpose(View));    
    output.viewDirection = worldSpacePos - eyePosition;    
    
    // calculate tangent space to world space matrix using the world space tangent,
    // binormal, and normal as basis vectors.  the pixel shader will normalize these
    // in case the world matrix has scaling.
    output.tangentToWorld[0] = mul(input.tangent,    World);
    output.tangentToWorld[1] = mul(input.binormal,    World);
    output.tangentToWorld[2] = mul(input.normal,    World);
    
    // pass the texture coordinate through without additional processing
    output.texCoord = input.texCoord;
    
    return output;
}

float4 NormalPixelShaderFunction( VS_OUTPUT input ) : COLOR0
{    
    // look up the normal from the normal map, and transform from tangent space into world space using the matrix created above.  
	// normalize the result in case the matrix contains scaling.
    float3 normalFromMap = tex2D(NormalMapSampler, input.texCoord);
    normalFromMap = mul(normalFromMap, input.tangentToWorld);
    normalFromMap = normalize(normalFromMap);
    
    // clean up our inputs a bit
    input.viewDirection = normalize(input.viewDirection);
    input.lightDirection = normalize(input.lightDirection);    
    
    // use the normal we looked up to do phong diffuse style lighting.    
    float nDotL = max(dot(normalFromMap, input.lightDirection), 0);
    float4 diffuse = LightColor * nDotL;
    
    // use phong to calculate specular highlights: reflect the incoming light vector off the normal, 
	// and use a dot product to see how "similar" the reflected vector is to the view vector.    
    float3 reflectedLight = reflect(input.lightDirection, normalFromMap);
    float rDotV = max(dot(reflectedLight, input.viewDirection), 0);
    float4 specular = Shininess * LightColor * pow(rDotV, SpecularPower);
    
    float4 diffuseTexture = tex2D(DiffuseTextureSampler, input.texCoord);
    
    // return the combined result.
    return (diffuse + AmbientLightColor) * diffuseTexture + specular;
}

Technique NormalMapping
{
    Pass Go
    {
        VertexShader = compile vs_2_0 NormalVertexShaderFunction();
        PixelShader = compile ps_2_0 NormalPixelShaderFunction();
    }
}

//*********** Create Shadow Map **************

struct ShadowMapVertexShaderOutput
{
    float4 Position : POSITION;
    float Depth     : TEXCOORD0;
};

// Transforms the model into light space an renders out the depth of the object
ShadowMapVertexShaderOutput CreateShadowMap_VertexShader(float4 Position: POSITION)
{
    ShadowMapVertexShaderOutput Out;
    Out.Position = mul(Position, mul(World, LightViewProj)); 
    Out.Depth = Out.Position.z / Out.Position.w;    
    return Out;
}

// Saves the depth value out to the 32bit floating point texture
float4 CreateShadowMap_PixelShader(ShadowMapVertexShaderOutput input) : COLOR
{ 
    return float4(input.Depth, 0, 0, 0);
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