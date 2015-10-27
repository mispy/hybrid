Shader "Custom/Circle" {
	Properties {
		[PerRendererData] _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
	}

	SubShader {
		Tags { 
			"Queue"="Transparent" 
		}

		ZWrite off
		ZTest off
		
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha     

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform float _ScanRadius;

            struct appdata_t {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

			struct v2f {
				half4 pos : POSITION;
				half2 uv : TEXCOORD0;
				half4 vertex : TEXCOORD2;
                fixed4 color    : COLOR;
			};

            fixed4 _Color;

			v2f vert(appdata_t v) {
				v2f o;
				o.vertex = v.vertex;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				half2 uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);
				o.uv = uv;
                o.color = v.color * _Color;
				return o;
			}

			half4 frag (v2f i) : COLOR {
				half4 color = tex2D(_MainTex, i.uv) * i.color;
				if (length(half2(0.5, 0.5) - i.uv) > 0.49) discard;				
				return color;
			}
			ENDCG
		}

	}

	Fallback off
}