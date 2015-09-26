 Shader "Custom/ShipColor" {
 Properties {
     _MainTex ("Base (RGB)", 2D) = "white" {}
     _SourceColor ("Source Color", Color) = (1.0, 1.0, 1.0, 1.0)
 }
 
 SubShader {
     Pass {
         ZTest LEqual
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
     half2 uv : TEXCOORD0;
 };
 
 v2f vert( appdata_img v )
 {
     v2f o;
     o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
     half2 uv = MultiplyUV( UNITY_MATRIX_TEXTURE0, v.texcoord );
     o.uv = uv;
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

half3 RGBtoHSL(in half3 RGB)
{
    half3 HCV = RGBtoHCV(RGB);
    half L = HCV.z - HCV.y * 0.5;
    half S = HCV.y / (1 - abs(L * 2 - 1) + Epsilon);
    return half3(HCV.x, S, L);
}

half3 HSLtoRGB(in half3 HSL)
{
    half3 RGB = HUEtoRGB(HSL.x);
    half C = (1 - abs(2 * HSL.z - 1)) * HSL.y;
    return (RGB - 0.5) * C + HSL.z;
}

half3 HSVtoRGB(in half3 HSV)
{
    half3 RGB = HUEtoRGB(HSV.x);
    return ((RGB - 1) * HSV.y + 1) * HSV.z;
}

 half4 frag (v2f i) : COLOR
 {
     half4 origColor = tex2D(_MainTex, i.uv);
     half4 baseColor = half4(0, 0, 0, 1);
     half4 highlightColor = half4(1, 0, 0, 1);
     
     half3 origHSL = RGBtoHSL(origColor);
     half3 baseHSL = RGBtoHSL(baseColor);
     half3 highlightHSL = RGBtoHSL(highlightColor);
     
     half3 outHSL = origHSL;
     if (origHSL.y < 0.2) {
         outHSL.x = baseHSL.x;
         outHSL.y = lerp(outHSL.y, baseHSL.y, 0.5);
         outHSL.z = lerp(outHSL.z, baseHSL.z, 0.5);
     } else {
         outHSL.x = highlightHSL.x;
        outHSL.y = lerp(outHSL.y, highlightHSL.y, 0.5);
        outHSL.z = lerp(outHSL.z, highlightHSL.z, 0.5);
     }
           
     half3 outColor = HSLtoRGB(outHSL);
     half4 final = half4(outColor.x, outColor.y, outColor.z, origColor.w);
         
     //return final;
     return origColor;
 }
 ENDCG
     }
 }
 
 Fallback off
 
 }