 Shader "Custom/ShipColor" {
 Properties {
     _MainTex ("Base (RGB)", 2D) = "white" {}
     _SourceColor ("Source Color", Color) = (1.0, 1.0, 1.0, 1.0)
 }
 
 SubShader {
     Pass {
         ZTest Always Cull Off ZWrite Off
         Blend SrcAlpha OneMinusSrcAlpha
         Fog { Mode off }
 
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma fragmentoption ARB_precision_hint_fastest 
 #include "UnityCG.cginc"
 
 uniform sampler2D _MainTex;
 uniform half4 _SourceColor;
 uniform half4 _MainTex_TexelSize;
 
 struct v2f {
     half4 pos : POSITION;
     half2 uv[5] : TEXCOORD0;
 };
 
 v2f vert( appdata_img v )
 {
     v2f o;
     o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
     half2 uv = MultiplyUV( UNITY_MATRIX_TEXTURE0, v.texcoord );
     o.uv[0] = uv;
     o.uv[1] = uv + half2(-_MainTex_TexelSize.x, -_MainTex_TexelSize.y);
     o.uv[2] = uv + half2(+_MainTex_TexelSize.x, -_MainTex_TexelSize.y);
     o.uv[3] = uv + half2(+_MainTex_TexelSize.x, +_MainTex_TexelSize.y);
     o.uv[4] = uv + half2(-_MainTex_TexelSize.x, +_MainTex_TexelSize.y);
     return o;
 }
 
half3 HUEtoRGB(half H)
{
    half R = abs(H * 6 - 3) - 1;
    half G = 2 - abs(H * 6 - 2);
    half B = 2 - abs(H * 6 - 4);
    return saturate(half3(R,G,B));
}

half Epsilon = 1e-10;

half3 RGBtoHCV(in half3 RGB)
{
    half4 P = (RGB.g < RGB.b) ? half4(RGB.bg, -1.0, 2.0/3.0) : half4(RGB.gb, 0.0, -1.0/3.0);
	half4 Q = (RGB.r < P.x) ? half4(P.xyw, RGB.r) : half4(RGB.r, P.yzx);
	half C = Q.x - min(Q.w, Q.y);
	half H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
	return half3(H, C, Q.x);
}

half3 RGBtoHSV(in half3 RGB)
{
	half3 HCV = RGBtoHCV(RGB);
	half S = HCV.y / (HCV.z + Epsilon);
	return half3(HCV.x, S, HCV.z);
}

half3 HSVtoRGB(in half3 HSV)
{
	half3 RGB = HUEtoRGB(HSV.x);
	return ((RGB - 1) * HSV.y + 1) * HSV.z;
}

 half4 frag (v2f i) : COLOR
 {
     half4 origColor = tex2D(_MainTex, i.uv[0]);
     
     half3 hsv = RGBtoHSV(origColor);
     hsv.x = 0;
     half3 destColor = HSVtoRGB(hsv);
     half4 final = half4(destColor.x, destColor.y, destColor.z, origColor.w);
         
     return final;
 }
 ENDCG
     }
 }
 
 Fallback off
 
 }