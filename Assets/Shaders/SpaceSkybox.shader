Shader "Custom/SpaceSkybox"
{
    Properties
    {
        _StarDensity ("Star Density", Range(100, 1000)) = 500
        _StarBrightness ("Star Brightness", Range(0, 1)) = 0.8
        _NebulaTex ("Nebula Texture (Optional)", 2D) = "black" {}
        _NebulaStrength ("Nebula Strength", Range(0, 1)) = 0.3
        _ColorTint ("Space Color Tint", Color) = (0.05, 0.05, 0.1, 1)
    }
    
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            float _StarDensity;
            float _StarBrightness;
            sampler2D _NebulaTex;
            float _NebulaStrength;
            float4 _ColorTint;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 texcoord : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 texcoord : TEXCOORD0;
            };
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }
            
            float hash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }
            
            float stars(float3 p)
            {
                float3 c = floor(p * _StarDensity);
                float star = hash(c);
                
                if (star > 0.99)
                {
                    float3 rel = frac(p * _StarDensity) - 0.5;
                    float dist = length(rel);
                    float brightness = 1.0 - dist * 2.0;
                    return max(0, brightness) * _StarBrightness;
                }
                return 0.0;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.texcoord);
                
                fixed4 col = _ColorTint;
                
                float starField = stars(dir);
                col.rgb += starField;
                
                float2 nebulaUV = float2(
                    atan2(dir.x, dir.z) / (2.0 * 3.14159) + 0.5,
                    asin(dir.y) / 3.14159 + 0.5
                );
                fixed4 nebula = tex2D(_NebulaTex, nebulaUV);
                col.rgb = lerp(col.rgb, nebula.rgb, _NebulaStrength * nebula.a);
                
                return col;
            }
            ENDCG
        }
    }
}