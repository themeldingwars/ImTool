#version 450

layout(set = 0, binding = 0) uniform ViewStateBuffer
{
    mat4 View;
    mat4 Projection;
    vec3 CamPos;
    float padding1;
    vec3 CamDir;
    float padding2;
    float CamNear;
    float CamFar;
} View;

layout(set = 1, binding = 0) uniform WorldBuffer
{
    mat4 World;
};

layout(location = 0) in vec3 Position;
layout(location = 1) in vec2 Uvs;

layout(location = 0) out vec3 OutPos;
layout(location = 1) out vec2 OutUvs;

vec3 gridPlane[6] = vec3[](
    vec3(1, 1, 0), vec3(-1, -1, 0), vec3(-1, 1, 0),
    vec3(-1, -1, 0), vec3(1, 1, 0), vec3(1, -1, 0)
);

void main() {
    vec3 p = gridPlane[gl_VertexIndex].xyz;
    gl_Position = vec4(p, 1.0);

    OutUvs = Uvs;
}