#if OPENGL
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0
    #define PS_SHADERMODEL ps_4_0
#endif

// SuperFastSoftShadows.fx
//
// Port of Scott Lembcke's "2D Lighting with Soft Shadows" shader
// to HLSL / MonoGame-style effect.
//
// Geometry:
//   Each shadow quad is built from a line segment.
//
//   a_segment.xy = endpoint_b
//   a_segment.zw = endpoint_a
//
//   a_shadow_coord = (x, y)
//      x = 0 → vertex associated with endpoint_a
//      x = 1 → vertex associated with endpoint_b
//      y = 0 → far (projected) edge of shadow
//      y = 1 → near (segment) edge of shadow
//
//   Quad vertex layout per segment (two triangles):
//      (endpoint_a, (0,0))
//      (endpoint_b, (1,0))
//      (endpoint_a, (0,1))
//      (endpoint_b, (1,1))
//
// Uniforms:
//   u_matrix: transforms from light-space quad coords to clip-space
//   u_light.xy: light position in same space as endpoints
//   u_light.z : light radius
//   LightPenetration: small value (e.g. 0.01) to bleed light into surfaces

// ============================================================
// Constant buffers
// ============================================================

cbuffer LightParams : register(b0)
{
    float4x4 u_matrix; // world-view-projection (or light-space → clip)
    float3 u_light; // xy = position, z = radius
    float LightPenetration; // ~0.01 recommended
}

// ============================================================
// Helpers
// ============================================================

// Build a 2x2 matrix from *column* vectors (GLSL-style mat2(a,b)).
float2x2 Mat2Cols(float2 c0, float2 c1)
{
    return float2x2(
        c0.x, c1.x,
        c0.y, c1.y
    );
}

// 2x2 adjugate matrix.
float2x2 Adjugate(float2x2 m)
{
    // [ m11 m12 ]
    // [ m21 m22 ]
    return float2x2(
         m._22, -m._12,
        -m._21, m._11
    );
}

// Multiply 2x2 matrix by 2D vector (column-style).
float2 MulM2x2(float2x2 m, float2 v)
{
    return float2(
        m._11 * v.x + m._12 * v.y,
        m._21 * v.x + m._22 * v.y
    );
}

// ============================================================
// Vertex / pixel structs
// ============================================================

struct VSInput
{
    float4 Segment : POSITION0; // (b.x, b.y, a.x, a.y) as per article
    float2 ShadowCoord : TEXCOORD0; // (x = endpoint selector, y = near/far)
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    float4 Penumbras : TEXCOORD0; // penumbra gradient coords (a.xy, b.xy)
    float3 Edges : TEXCOORD1; // x/y = penetration helpers, z = clip edge
    float3 ProjPos : TEXCOORD2; // projected pixel position (for penetration)
    float4 Endpoints : TEXCOORD3; // endpoint_a.xy, endpoint_b.xy (scaled)
};

// ============================================================
// Vertex shader
// ============================================================

VSOutput VS_SoftShadow(VSInput input)
{
    VSOutput o;

    float2 endpoint_a = input.Segment.zw;
    float2 endpoint_b = input.Segment.xy;
    float2 endpoint = lerp(endpoint_a, endpoint_b, input.ShadowCoord.x);

    float light_radius = u_light.z;
    float2 light_pos = u_light.xy;

    // Deltas from segment endpoints to light center.
    float2 delta_a = endpoint_a - light_pos;
    float2 delta_b = endpoint_b - light_pos;
    float2 delta = endpoint - light_pos;

    // Offsets from light center to edge of light volume (approximate tangents).
    float2 offset_a = float2(-light_radius, light_radius) * normalize(delta_a).yx;
    float2 offset_b = float2(light_radius, -light_radius) * normalize(delta_b).yx;
    float2 offset = lerp(offset_a, offset_b, input.ShadowCoord.x);

    // Projective coordinate (w = 0 → far, w = 1 → near).
    float w = input.ShadowCoord.y;
    float2 proj_xy = lerp(delta - offset, endpoint - light_pos, w);

    // Transform to clip space in a stable affine form.
    float4 clipPos = mul(float4(proj_xy + light_pos * w, 0.0f, w), u_matrix);
    o.Position = clipPos;

    // --------------------------------------------------------
    // Penumbra gradients (two sides of light).
    // --------------------------------------------------------

    float2x2 mA = Mat2Cols(offset_a, -delta_a);
    float2x2 mB = Mat2Cols(-offset_b, delta_b);

    float2 vA = delta - lerp(offset, delta_a, w);
    float2 vB = delta - lerp(offset, delta_b, w);

    float2 penumbra_a = MulM2x2(Adjugate(mA), vA);
    float2 penumbra_b = MulM2x2(Adjugate(mB), vB);

    // Handle radius == 0 as special case.
    if (light_radius > 0.0f)
    {
        o.Penumbras = float4(penumbra_a, penumbra_b);
    }
    else
    {
        o.Penumbras = float4(0.0f, 1.0f, 0.0f, 1.0f);
    }

    // --------------------------------------------------------
    // Edge clipping: stop shadows projecting "forward".
    // --------------------------------------------------------

    float2 seg_delta = endpoint_b - endpoint_a;
    float2 seg_normal = seg_delta.yx * float2(-1.0f, 1.0f);

    // z is used for clipping in the pixel shader.
    o.Edges.z = dot(seg_normal, delta - offset) * (1.0f - w);

    // --------------------------------------------------------
    // Light penetration setup (optional, but recommended).
    // --------------------------------------------------------

    float2x2 mPen = Mat2Cols(seg_delta, delta_a + delta_b);
    float2 edges_xy = -MulM2x2(Adjugate(mPen), (delta - offset * (1.0f - w)));
    edges_xy.y *= 2.0f; // save a multiply in the pixel shader

    o.Edges.xy = edges_xy;

    float lp = LightPenetration;
    if (lp <= 0.0f)
        lp = 0.01f;

    // Scale proj position and endpoints to avoid per-pixel multiplies.
    // Keep z above zero to avoid unstable divides in the pixel shader.
    o.ProjPos = float3(proj_xy, max(w * lp, 1e-4f));
    o.Endpoints = float4(endpoint_a, endpoint_b) / lp;

    return o;
}

// ============================================================
// Pixel shader
// ============================================================

float4 PS_SoftShadow(VSOutput input) : SV_Target
{
    // --------------------------------------------------------
    // Penumbra gradients for both sides of the segment.
    // --------------------------------------------------------

    // Gradient coordinates in "uv" space.
    // Guard: at near vertices (w=1) the y-components degenerate to zero, causing 0/0 = NaN.
    // When |yw| is tiny, force grad to 0 (mid-smoothstep, neutral contribution).
    float2 valid = step(1e-6f, abs(input.Penumbras.yw));
    float2 grad = input.Penumbras.xz * valid / max(abs(input.Penumbras.yw), 1e-6f);

    // Smooth falloff, clamp to [-1,1] and [0,1].
    float2 pen = smoothstep(-1.0f, 1.0f, grad);

    // Only keep the part where y >= 0 for each gradient.
    float2 mask = step(float2(0.0f, 0.0f), input.Penumbras.yw);
    float penumbra = dot(pen, mask);

    // Precision fudge factor to hide cracks between segments.
    penumbra -= 1.0f / 64.0f;

    // --------------------------------------------------------
    // Light penetration (soften near edge, show rim light).
    // --------------------------------------------------------

    // Intersection of light→pixel ray with segment, clamped to segment.
    float intersection_t =
        clamp(input.Edges.x / abs(input.Edges.y), -0.5f, 0.5f);

    float2 intersection_point =
        (0.5f - intersection_t) * input.Endpoints.xy +
        (0.5f + intersection_t) * input.Endpoints.zw;

    // Pixel position in the same space, recovered from proj coords.
    float2 pixel_pos = input.ProjPos.xy / max(abs(input.ProjPos.z), 1e-4f);

    float2 penetration_delta = intersection_point - pixel_pos;

    // Simple quadratic falloff for how much light "bleeds" into the surface.
    float bleed = min(dot(penetration_delta, penetration_delta), 1.0f);

    // --------------------------------------------------------
    // Final shadow mask for this segment.
    //   penumbra → 0..1 shadow strength
    //   bleed    → 0..1 light penetration factor
    //   step() with Edges.z clips reversed shadows.
    // --------------------------------------------------------

    float shadow = bleed * (1.0f - penumbra) * step(input.Edges.z, 0.0f);

    // Clip shadow to the light's radius.
    // pixel_pos is in light-relative world space scaled by LightPenetration,
    // so multiplying its length back by LightPenetration recovers the world distance.
    float distFromLight = length(pixel_pos) * LightPenetration;
    shadow *= step(distFromLight, u_light.z);

    shadow = saturate(shadow);

    // Output single-channel mask in RGB (alpha = 1).
    //float bad = (any(isnan(input.Penumbras)) || any(isnan(input.Edges)) || any(isnan(input.ProjPos))) ? 1.0f : 0.0f;
    //return float4(1, 1, 1, 0.5f);
    return float4(shadow, shadow, shadow, 1.0f);
}

// ============================================================
// Technique
// ============================================================

technique SoftShadow
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL VS_SoftShadow();
        PixelShader = compile PS_SHADERMODEL PS_SoftShadow();
    }
}
