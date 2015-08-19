 Shader "Custom/PowerCircle" {
	 Properties {
	     _MainTex ("Base (RGB)", 2D) = "white" {}
	     _SourceColor ("Source Color", Color) = (1.0, 1.0, 1.0, 1.0)
	 }
	 
	 SubShader {
	     Tags { "Queue"="Transparent" }
	     
	     
		Pass {
		     Blend SrcAlpha OneMinusSrcAlpha     
	 
			 CGPROGRAM
			 #pragma vertex vert
			 #pragma fragment frag
			 #include "UnityCG.cginc"
			 
			 uniform sampler2D _MainTex;
			 
			 struct v2f {
			     half4 pos : POSITION;
			     half2 uv : TEXCOORD0;
			 };
			 
			 v2f vert(appdata_img v) {
			     v2f o;
			     o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			     half2 uv = MultiplyUV( UNITY_MATRIX_TEXTURE0, v.texcoord );
			     o.uv = uv;
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