float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 LightViewProj;

float3 LightPosition;
float4 LightColor;
float4 AmbientLightColor;
float3 LightDirection; //Used for shadows
float DepthBias = 0.001f; // Used for shadows

// output from phong specular will be scaled by this amount
float Shininess;

// specular exponent from phong lighting model.  controls the "tightness" of specular highlights.
float SpecularPower;

texture2D NormalMap;
sampler2D NormalMapSampler = sampler_state
{
    Texture = <NormalMap>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
};

texture2D Texture;
sampler2D DiffuseTextureSampler = sampler_state
{
    Texture = <Texture>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
};

texture ShadowMap;
sampler ShadowMapSampler = sampler_state
{
    Texture = <ShadowMap>;
};

//****************** ShadowMap Original ******************

struct VS_INPUT
{
    float4 Position : POSITION0;
    float3 Normal   : NORMAL0;
    float2 TexCoord : TEXCOORD0;    
    float3 Binormal	: BINORMAL0;
    float3 Tangent	: TANGENT0;
};

// output from the vertex shader, and input to the pixel shader. lightDirection and viewDirection are in world space.
// NOTE: even though the tangentToWorld matrix is only marked with TEXCOORD4, it will actually take TEXCOORD4, 5, and 6.
struct VS_OUTPUT
{
    float4 Position				: POSITION0;
    float3 Normal				: TEXCOORD0;
    float2 TexCoord				: TEXCOORD1;
    float4 WorldPos				: TEXCOORD2;
	float3 viewDirection		: TEXCOORD3;
    float3x3 tangentToWorld		: TEXCOORD4;
};

// Draws the model with shadows
VS_OUTPUT VertexShaderFunction(VS_INPUT input)
{
    VS_OUTPUT Output;

    float4x4 WorldViewProj = mul(mul(World, View), Projection);
    
    // Transform the models verticies and normal
    Output.Position = mul(input.Position, WorldViewProj);
    Output.Normal =  normalize(mul(input.Normal, World));
    Output.TexCoord = input.TexCoord;
    
	// similarly, calculate the view direction, from the eye to the surface.  not normalized, in world space.
    float3 cameraPosition = mul(-View._m30_m31_m32, transpose(View));    
    Output.viewDirection = Output.Position - cameraPosition;    
    
    // calculate tangent space to world space matrix using the world space tangent, binormal, and normal as basis vectors. the pixel shader will normalize these in case the world matrix has scaling.
    Output.tangentToWorld[0] = mul(input.Tangent,    World);
    Output.tangentToWorld[1] = mul(input.Binormal,    World);
    Output.tangentToWorld[2] = mul(input.Normal,    World);

    // Save the vertices postion in world space
    Output.WorldPos = mul(input.Position, World);
    
    return Output;
}

// Determines the depth of the pixel for the model and checks to see if it is in shadow or not
float4 PixelShaderFunction(VS_OUTPUT input) : COLOR
{
    // look up the normal from the normal map, and transform from tangent space into world space using the matrix created above. normalize the result in case the matrix contains scaling.
    float3 normalFromMap = tex2D(NormalMapSampler, input.TexCoord);
    normalFromMap = mul(normalFromMap, input.tangentToWorld);
    normalFromMap = normalize(normalFromMap);
    
    // clean up our inputs a bit
    input.viewDirection = normalize(input.viewDirection);
    
    // use the normal we looked up to do phong diffuse style lighting.    
    float nDotL = max(dot(normalFromMap, LightDirection), 0);
    float4 diffuse = LightColor * nDotL;
    
    // use phong to calculate specular highlights: reflect the incoming light vector off the normal, and use a dot product to see how "similar" the reflected vector is to the view vector.    
    float3 reflectedLight = reflect(LightDirection, normalFromMap);
    float rDotV = max(dot(reflectedLight, input.viewDirection), 0);
    float4 specular = Shininess * LightColor * pow(rDotV, SpecularPower);
    
	// Find the position of this pixel in light space
    float4 lightingPosition = mul(input.WorldPos, LightViewProj);

    float4 diffuseTexture = tex2D(DiffuseTextureSampler, input.TexCoord); //normal
    float4 diffuseColor = tex2D(DiffuseTextureSampler, input.TexCoord); //shadow
    float diffuseIntensity = saturate(dot(LightDirection, input.Normal)); // shadow; Intensity based on the direction of the light
    
	// was = now is += bucause we created and used diffuse for the normal map
	diffuse += diffuseIntensity * diffuseColor + AmbientLightColor; // shadow; Final diffuse color with ambient color added
    
    // Find the position in the shadow map for this pixel
    float2 ShadowTexCoord = 0.5 * lightingPosition.xy / lightingPosition.w + float2( 0.5, 0.5 ); //shadow
    ShadowTexCoord.y = 1.0f - ShadowTexCoord.y; //shadow

    // Get the current depth stored in the shadow map
    float shadowdepth = tex2D(ShadowMapSampler, ShadowTexCoord).r; //shadow
    
    // Calculate the current pixel depth. The bias is used to prevent folating point errors that occur when the pixel of the occluder is being drawn
    float ourdepth = (lightingPosition.z / lightingPosition.w) - DepthBias; //shadow
	    
    // Check to see if this pixel is in front or behind the value in the shadow map
    if (shadowdepth < ourdepth)
    {
        // Shadow the pixel by lowering the intensity
        diffuse *= float4(0.5,0.5,0.5,0);
    };

    // return the combined result.
    //return (diffuse + AmbientLightColor) * diffuseTexture + specular;
    return diffuse; // shadow
}

// Technique for drawing with the shadow map
technique DrawWithShadowMap
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}


//****************** Create ShadowMap ******************
struct CreateShadowMap_VSOut
{
    float4 Position : POSITION;
    float Depth     : TEXCOORD0;
};

// Transforms the model into light space an renders out the depth of the object
CreateShadowMap_VSOut CreateShadowMap_VertexShader(float4 Position: POSITION)
{
    CreateShadowMap_VSOut Out;
    Out.Position = mul(Position, mul(World, LightViewProj)); 
    Out.Depth = Out.Position.z / Out.Position.w;    
    return Out;
}

// Saves the depth value out to the 32bit floating point texture
float4 CreateShadowMap_PixelShader(CreateShadowMap_VSOut input) : COLOR
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
//******************************************************