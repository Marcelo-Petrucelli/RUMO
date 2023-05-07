Shader "BlackAndWhiteShader"
{
    Properties
    {
        _Hue("Hue", Range(-360, 360)) = 0.
        _Brightness("Brightness", Range(-1, 1)) = 0.
        _Contrast("Contrast", Range(0, 2)) = 1
        _Saturation("Saturation", Range(0, 2)) = 1
        _SatIntensity("Saturation Intensity", Vector) = (0.299,0.587,0.114,0)
    }
    SubShader
    {
        // Draw after all opaque geometry
        //Tags { "Queue" = "Background" }
        //Tags { "Queue" = "Transparent" }
        //Tags { "RenderType" = "Transparent" }
        //Tags { "RenderType" = "Background" }

        //Cull Off
        //ZWrite Off
        //Lighting Off
        //Blend One OneMinusSrcAlpha

        // Grab the screen behind the object into _BackgroundTexture
        GrabPass
        {
            "_BackgroundTexture"
        }

        Pass
        {
         Stencil {
            Ref 254
            Comp always
            Pass replace
        }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f
            {
                float4 grabPos : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            float _Hue;
            float _Brightness;
            float _Contrast;
            float _Saturation;
            float4 _SatIntensity;

            v2f vert(appdata_base v) {
                v2f o;
                // use UnityObjectToClipPos from UnityCG.cginc to calculate 
                // the clip-space of the vertex
                o.pos = UnityObjectToClipPos(v.vertex);

                // use ComputeGrabScreenPos function from UnityCG.cginc
                // to get the correct texture coordinate
                o.grabPos = ComputeGrabScreenPos(o.pos);
                return o;
            }

            inline float3 applyHue(float3 aColor, float aHue)
            {
                float angle = radians(aHue);
                float3 k = float3(0.57735, 0.57735, 0.57735);
                float cosAngle = cos(angle);
                //Rodrigues' rotation formula
                return aColor * cosAngle + cross(k, aColor) * sin(angle) + k * dot(k, aColor) * (1 - cosAngle);
            }
            inline float4 applyHSBEffect(float4 startColor)
            {
                float4 outputColor = startColor;
                outputColor.rgb = applyHue(outputColor.rgb, _Hue);
                outputColor.rgb = (outputColor.rgb - 0.5f) * (_Contrast)+0.5f;
                outputColor.rgb = outputColor.rgb + _Brightness;
                float3 intensity = dot(outputColor.rgb, _SatIntensity.xyz);
                outputColor.rgb = lerp(intensity, outputColor.rgb, _Saturation);
                return outputColor;
            }

            sampler2D _BackgroundTexture;

            half4 frag(v2f i) : SV_Target
            {
                half4 bgcolor = tex2Dproj(_BackgroundTexture, i.grabPos);
                //if (bgcolor.a < 0.1)  discard;
                return applyHSBEffect(bgcolor); //1 - bgcolor;
            }
            ENDCG
        }

    }
}