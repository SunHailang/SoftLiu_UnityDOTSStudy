Shader "Unlit/ShaderSHL/Base"
{
	Properties
	{
		_Texture2D("Texture2D", 2D) = "white"{}
		_Color ("Color", Color) = (1, 1, 1, 1)
	}

	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			sampler2D _Texture2D;
			//float4 _Color;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float4 color = tex2D(_Texture2D, i.uv);
				color *= fixed4(i.uv.x, i.uv.y, 0, 1);
				return color;
			}
			ENDCG
		}
	}
}
