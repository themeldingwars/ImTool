#version 450

layout(location = 0) in vec3 pos;
layout(location = 1) in vec2 Uvs;
layout(location = 0) out vec4 outColor;

layout(set = 1, binding = 0) uniform WorldBuffer
{
    vec4 Color;
	float Thickness;
};

layout(set = 1, binding = 1) uniform utexture2D SurfaceTexture;
layout(set = 1, binding = 2) uniform sampler SurfaceSampler;

vec2 rotate2D(vec2 _st, float _angle){
    _st -= 0.5;
    _st =  mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle)) * _st;
    _st += 0.5;
    return _st;
}

vec2 tile(vec2 _st, float _zoom){
    _st *= _zoom;
    return fract(_st);
}

float box(vec2 _st, vec2 _size, float _smoothEdges){
    _size = vec2(0.5)-_size*0.5;
    vec2 aa = vec2(_smoothEdges*0.5);
    vec2 uv = smoothstep(_size,_size+aa,_st);
    uv *= smoothstep(_size,_size+aa,vec2(1.0)-_st);
    return uv.x*uv.y;
}

void main() {
	vec4 texCol = texture(usampler2D(SurfaceTexture, SurfaceSampler), Uvs);
	vec2 texSize = textureSize(usampler2D(SurfaceTexture, SurfaceSampler), 0);

	const vec3 target = ivec3(0.0, 0.0, 0.0);
    const float TAU = 6.28318530;
	const float steps = 32.0;
    
	float radius = Thickness;
	vec2 uv = Uvs / texSize;
    vec2 aspect = 1.0 / texSize;
    
	outColor = vec4(uv.y, 0.0, uv.x, 0.0);
	for (float i = 0.0; i < TAU; i += TAU / steps) {
		// Sample image in a circular pattern
        vec2 offset = vec2(sin(i), cos(i)) * aspect * radius;
		vec4 col = texture(usampler2D(SurfaceTexture, SurfaceSampler), Uvs + offset);
		
		// Mix outline with background
		float alpha = smoothstep(0.5, 0.7, distance(col.rgb, target));
		outColor = mix(outColor, Color, alpha);
	}

	if (texCol.r == 1)
	{
	    vec2 shape = Uvs.xy;
        shape = tile(shape,200.0);
        shape = rotate2D(shape, 3.14159265358979323846 * 0.25);
        float color = box(shape, vec2(0.7), 0.01);

		outColor.a = 0.7;

		if (color == 0)
		{
			discard;
		}
	}

	if (texCol.r == 255)
	{
		discard;
	}
}