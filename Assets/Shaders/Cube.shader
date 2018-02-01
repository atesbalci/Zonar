// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Simplified Diffuse shader. Differences from regular Diffuse one:
// - no Main Color
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "Mobile/DiffuseCube" {
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
    o.Albedo = lerp(_MULTIPLIER * _IdleColor.rgb, lerp(_Color.rgb, _OverrideColor.rgb, _OverrideColor.a), (IN.worldPos.y - _MIN) / (_MAX - _MIN));
}
ENDCG
}

Fallback "Mobile/VertexLit"
}
