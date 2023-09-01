#version 450

layout(location = 0) in vec2 Uvs;
layout(location = 1) in vec3 Norm;
layout(location = 2) in vec3 FragPos;

layout(set = 1, binding = 0) uniform WorldBuffer
{
    mat4 World;
    uint SelectionId;
    uint Flags;
};

layout(set = 2, binding = 0) uniform texture2D SurfaceTexture;
layout(set = 2, binding = 2) uniform sampler SurfaceSampler;

layout(location = 0) out vec4 outColor;
layout(location = 1) out uint outId;
layout(location = 2) out uint selectedMask;

void main() {
    //outColor = vec4(1, 1, 1, 1);
    
    vec3 norm = normalize(Norm);
    vec3 lightDir = normalize(vec3(0, 10, 0) - FragPos);
    
    //float diff = max(dot(norm, lightDir), 0.0);
    //vec3 diffuse = diff * vec3(0.5, 0.1, 0.7);


    float diff = clamp(dot(Norm, lightDir), 0.f, 100000);
    vec4 IDiffuse = vec4(diff, diff, diff, diff);

    vec4 IAmbient = vec4(0.2f, 0.2f, 0.2f, 1.0f);
    vec4 texCol = texture(sampler2D(SurfaceTexture, SurfaceSampler), Uvs);
    outColor = (IAmbient + IDiffuse) * vec4(texCol.rgb, 1.0f);
    //outColor = texture(sampler2D(SurfaceTexture, SurfaceSampler), Uvs);
    //outColor = vec4(1, 1, 1, 1);

    outId = SelectionId;
    selectedMask = 0;

    if (Flags == 1)
    {
        selectedMask = 0xFF;
    }
}