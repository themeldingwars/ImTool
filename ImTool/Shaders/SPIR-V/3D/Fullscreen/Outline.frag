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

void main() {
	//vec4 texCol = imageLoad(SurfaceTexture, ivec2(Uvs));
	vec4 texCol = texture(usampler2D(SurfaceTexture, SurfaceSampler), Uvs);

	const vec3 target = ivec3(0.0, 0.0, 0.0); // Find green
    const float TAU = 6.28318530;
	const float steps = 32.0;
    
	float radius = Thickness;
	vec2 uv = Uvs / textureSize(usampler2D(SurfaceTexture, SurfaceSampler), 0);
    
    // Correct aspect ratio
    vec2 aspect = 1.0 / textureSize(usampler2D(SurfaceTexture, SurfaceSampler), 0);
    
	outColor = vec4(uv.y, 0.0, uv.x, 0.0);
	for (float i = 0.0; i < TAU; i += TAU / steps) {
		// Sample image in a circular pattern
        vec2 offset = vec2(sin(i), cos(i)) * aspect * radius;
		vec4 col = texture(usampler2D(SurfaceTexture, SurfaceSampler), Uvs + offset);
		
		// Mix outline with background
		float alpha = smoothstep(0.5, 0.7, distance(col.rgb, target));
		outColor = mix(outColor, Color, alpha);
	}

	if (texCol.r == 255)
	{
		//outColor.a = 1.0;
		discard;
	}
}