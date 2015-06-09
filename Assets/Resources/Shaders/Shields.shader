Shader "FX/Directional Shield" {
    Properties {
        _Offset ("Time", Range (0, 1)) = 0.0
        _Color ("Tint (RGBA)", Color) = (1,1,1,1)
        _Position ("Position",Vector) = (0,0,0,0)
        _RadialFactor ("Radial Factor",Range (0,1)) = 1.0
        _MainTex ("Texture (RGB)", 2D) = "white" {}
    }
    SubShader {
        ZWrite Off
        Tags { "Queue" = "Transparent" }
        Blend One One
        Cull Off

        Pass { 
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_fog_exp2
            #include "UnityCG.cginc"

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float3 normal : TEXCOORD2;
                float color:COLOR0;
            };

            uniform float _Offset;
            uniform float _RadialFactor;
            uniform float4 _Position;
            uniform float4 _Color;
            uniform sampler2D _MainTex : register(s1);
            
            v2f vert (appdata_base v) {
                v2f o;
                o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
                v.texcoord.x = v.texcoord.x;
                v.texcoord.y = v.texcoord.y + _Offset;
                o.uv = TRANSFORM_UV (1);
                o.normal = v.normal;
                o.color = _RadialFactor/distance (v.vertex,_Position.xyz),0.0,1.0;
                if (o.color < 0.15)
                       o.color = 0.0;
                return o;
            }

            half4 frag (v2f f) : COLOR
            {
                half4 tex = tex2D (_MainTex, f.uv)*f.color*_Color*_Color.a ;
                return half4 (tex.r, tex.g, tex.b, tex.a);
            }
            ENDCG


        }
    }
    Fallback "Transparent/VertexLit"
}
