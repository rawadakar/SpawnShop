Shader "Custom/ClipShader"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _ClipSize("Clip Size (Half Extents)", Vector) = (1,1,1,0)
    }

        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 200

            Pass
            {
                Name "ClippedPass"
                Tags { "LightMode" = "UniversalForward" }

                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

                struct Attributes
                {
                    float4 positionOS : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct Varyings
                {
                    float4 positionCS : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    float3 worldPos : TEXCOORD1;
                };

                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);

                float4 _ClipSize;
                float4x4 _ClipWorldToLocal;

                Varyings vert(Attributes IN)
                {
                    Varyings OUT;
                    float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                    OUT.worldPos = worldPos;
                    OUT.positionCS = TransformWorldToHClip(worldPos);
                    OUT.uv = IN.uv;
                    return OUT;
                }

                half4 frag(Varyings IN) : SV_Target
                {
                    float3 localPos = mul(_ClipWorldToLocal, float4(IN.worldPos, 1.0)).xyz;

                    if (any(abs(localPos) > _ClipSize.xyz))
                        discard;

                    return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                }
                ENDHLSL
            }
        }
}
