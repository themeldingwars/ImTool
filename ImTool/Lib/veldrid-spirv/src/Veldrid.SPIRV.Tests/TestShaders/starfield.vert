#version 450
#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable

layout(location = 0) out vec4 fsin_ClipPos;
layout(location = 1) out vec3 fsin_TexCoord;

void main()
{
    fsin_TexCoord = vec3((gl_VertexIndex << 1) & 2, gl_VertexIndex & 2, gl_VertexIndex & 2);
    gl_Position = vec4(fsin_TexCoord.xy * 2.0f - 1.0f, 0.0f, 1.0f);
    fsin_ClipPos = gl_Position;
    fsin_ClipPos.y *= -1;
}
