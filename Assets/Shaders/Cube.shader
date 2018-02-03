Shader "Zonar/DiffuseCube" {
Properties {
    _Color("Main Color", Color) = (1, 1, 1, 1)
	_IdleColor("Idle Color", Color) = (0, 0, 0, 1)
	[PreRendererData]_OverrideColor("Override Color", Color) = (0, 0, 0, 0)
}
SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 150

	CGPROGRAM
	#pragma surface surf Lambert noforwardadd

	uniform float _MIN;
	uniform float _MAX;
	uniform float _MULTIPLIER = 1.0f;
	float4 _Color;
	float4 _IdleColor;
	float4 _OverrideColor;

	struct Input {
		float3 worldPos;
	};

	void surf (Input IN, inout SurfaceOutput o) {
		o.Albedo = lerp(_MULTIPLIER * _IdleColor.rgb,
		lerp(_Color.rgb, _OverrideColor.rgb, clamp(min(_OverrideColor.a, (IN.worldPos.y - _MIN)), 0.0f, 1.0f)),
		(IN.worldPos.y - _MIN) / (_MAX - _MIN));
	}
	ENDCG
}

Fallback "Mobile/VertexLit"
}
