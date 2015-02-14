float4x4 World;
float4x4 View;
float4x4 Projection;

texture2D Texture;
sampler2D DiffuseTextureSampler = sampler_state
{
    Texture = <Texture>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
};

struct VertexShaderInput
{
    float4 position : POSITION0;
	float2 texCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 position            : POSITION0;
    float2 texCoord            : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.position = mul(viewPosition, Projection);

	output.texCoord = input.texCoord;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    return tex2D(DiffuseTextureSampler, input.texCoord);
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
