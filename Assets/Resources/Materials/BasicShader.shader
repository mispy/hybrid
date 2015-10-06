Shader "Custom/BasicShader" {
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
			uniform sampler2D _Visibility;
			uniform half4 _MainTex_ST;
			
			struct v2f {
				half4 pos : POSITION;
				half2 uv : TEXCOORD0;
				half2 texcoord : TEXCOORD2;
			};

			v2f vert(appdata_img v) {
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = v.texcoord;
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			half4 frag (v2f i) : COLOR {
				half4 color = tex2D(_MainTex, i.uv);
				return color;
			}
			ENDCG
		}

	}

	Fallback off
}