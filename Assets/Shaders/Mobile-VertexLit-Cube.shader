Shader "Zonar/VertexLitCube" {
Properties {
    _Color("Main Color", Color) = (1, 1, 1, 1)
	_IdleColor("Idle Color", Color) = (0, 0, 0, 1)
	[PreRendererData]_OverrideColor("Override Color", Color) = (0, 0, 0, 0)
}

SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 80

	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"

		uniform float _MIN;
		uniform float _MAX;
		uniform float _MULTIPLIER = 1.0f;
		float4 _Color;
		float4 _IdleColor;
		float4 _OverrideColor;

		struct appdata
		{
			float3 pos : POSITION;
		};

		struct v2f
		{
			fixed4 col : COLOR;
		};

		v2f vert(appdata IN)
		{
			v2f o;
			o.col = fixed4(lerp(_MULTIPLIER * _IdleColor.rgb, 
				lerp(_Color.rgb, _OverrideColor.rgb, _OverrideColor.a),
				(IN.pos.y - _MIN) / (_MAX - _MIN)), 1);
			return o;
		}

		fixed4 frag(v2f IN) : SV_Target
		{
			return IN.col;
		}

		ENDCG
	}
}
}
