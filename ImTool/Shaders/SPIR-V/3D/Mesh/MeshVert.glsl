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
    uint SelectionId;
};

layout(location = 0) in vec3 Position;
layout(location = 1) in vec2 Uvs;
layout(location = 2) in vec3 Norm;

layout(location = 0) out vec2 out_Uvs;
layout(location = 1) out vec3 out_Norm;
layout(location = 2) out vec3 out_FragPos;

void main() {
    vec4 worldPosition = World * vec4(Position, 1);
    vec4 viewPosition = View.View * worldPosition;
    vec4 clipPosition = View.Projection * viewPosition;
    
    gl_Position = clipPosition;
    out_Uvs = Uvs;
    
    out_FragPos = vec3(World * vec4(Position, 1.0));
     
    //out_NormUvs = NormUvs;
    out_Norm = mat3(transpose(inverse(World))) * Norm;
    out_Norm = Norm;
    //out_Tang = Tang;
}