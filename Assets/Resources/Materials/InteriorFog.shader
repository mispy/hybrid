Shader "Custom/InteriorFog"
{
	Properties
	{
		_MainTex ("Hidden Texture", 2D) = "white" {}
		[PerRendererData] _Visibility ("Visibility Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
				half2 uv        : TEXCOORD4;
				float4 localPos : TEXCOORD2;
				float2 worldPos : TEXCOORD3;
			};
			
			fixed4 _Color;
            float4 _MainTex_ST;

			v2f vert(appdata_t IN)
			{
				v2f OUT;				
				OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);				
				OUT.texcoord = IN.texcoord;		
				OUT.uv = TRANSFORM_TEX(IN.texcoord, _MainTex);
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif
				OUT.localPos = IN.vertex;
                OUT.worldPos = mul(_Object2World, IN.vertex).xy;                

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			sampler2D _Visibility;
			float _AlphaSplitEnabled;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);
				if (_AlphaSplitEnabled)
					color.a = tex2D (_AlphaTex, uv).r;

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture (IN.uv) * IN.color;
				float2 blockPos = IN.texcoord;
				fixed4 vis = tex2D(_Visibility, blockPos);
				
				if (vis.a > 0.5)
					c.rgb *= c.a;
				else
					c = fixed4(0, 0, 0, 0);
				return c;
			}
		ENDCG
		}
	}
}
