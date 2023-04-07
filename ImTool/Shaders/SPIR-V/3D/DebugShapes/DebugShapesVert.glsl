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
layout(location = 1) in vec4 Color;
layout(location = 2) in float Thickness;

layout(location = 0) out vec4 out_Color;
layout(location = 1) out float out_Thickness;

void main() {
    vec4 worldPosition = World * vec4(Position, 1);
    vec4 viewPosition = View.View * worldPosition;
    vec4 clipPosition = View.Projection * viewPosition;
    
    gl_Position = clipPosition;
    
    out_Color = Color;
    out_Thickness = Thickness;
}