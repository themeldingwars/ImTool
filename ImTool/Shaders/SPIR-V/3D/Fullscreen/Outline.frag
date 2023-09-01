#version 450

layout(location = 0) in vec3 pos;
layout(location = 1) in vec2 Uvs;
layout(location = 0) out vec4 outColor;

layout(set = 1, binding = 0) uniform WorldBuffer
{
    mat4 World;
    uint SelectionId;
};

layout(set = 1, binding = 1) uniform texture2D SurfaceTexture;
layout(set = 1, binding = 2) uniform sampler SurfaceSampler;

void main() {
	vec4 texCol = texture(sampler2D(SurfaceTexture, SurfaceSampler), Uvs);

	if (SelectionId == texCol.r)
	{
		outColor = vec4(1.0f, 0.0f, 0.0f, 1.0f);
	}
	else
	{
		outColor = vec4(0.0f, 0.0f, 0.0f, 0.0f);
	}
}