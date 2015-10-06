Shader "Custom/Circle" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
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

			struct v2f {
				half4 pos : POSITION;
				half2 uv : TEXCOORD0;
				half4 vertex : TEXCOORD2;
			};

			v2f vert(appdata_img v) {
				v2f o;
				o.vertex = v.vertex;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				half2 uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);
				o.uv = uv;
				return o;
			}

			half4 frag (v2f i) : COLOR {
				half4 color = tex2D(_MainTex, i.uv);
				if (length(half2(0.5, 0.5) - i.uv) > 0.25) discard;				
				return color;
			}
			ENDCG
		}

	}

	Fallback off
}