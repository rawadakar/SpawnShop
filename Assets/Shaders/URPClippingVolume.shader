Shader "Custom/URPClippingVolume"
{
    Properties
    {
        _StencilRef ("Stencil Reference", Int) = 1
    }
    SubShader
    {
        // Render just after normal geometry so it marks stencil first
        Tags { "Queue"="Geometry+1" "RenderPipeline"="UniversalPipeline" }

        // Front faces: increment stencil when depth test passes
        Pass
        {
            Name "StencilIncrement"
            Tags { "LightMode"="UniversalForward" }
            Cull Front
            ZWrite Off
            Blend Zero One
            ColorMask 0
            Stencil
            {
                Ref [_StencilRef]
                Comp Always
                Pass IncrSat
            }
        }

        // Back faces: decrement stencil when depth test passes
        Pass
        {
            Name "StencilDecrement"
            Tags { "LightMode"="UniversalForward" }
            Cull Back
            ZWrite On
            ColorMask 0
            Stencil
            {
                Ref [_StencilRef]
                Comp Always
                Pass DecrSat
            }
        }
    }
}
