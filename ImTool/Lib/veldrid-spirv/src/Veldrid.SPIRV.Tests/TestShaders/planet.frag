#version 450
#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable

layout(set = 0, binding = 0) uniform ProjView
{
    mat4 View;
    mat4 Proj;
};

layout(set = 0, binding = 2) uniform LightInfo
{
    vec3 LightDirection;
    float padding0;
    vec3 CameraPosition;
    float padding1;
};

layout(set = 1, binding = 0) uniform texture2D Tex;
layout(set = 1, binding = 1) uniform sampler Samp;

layout(location = 0) in vec3 fsin_Position_WorldSpace;
layout(location = 1) in vec3 fsin_Normal;
layout(location = 2) in vec2 fsin_TexCoord;

layout(location = 0) out vec4 outputColor;

void main()
{
    vec4 texColor = texture(sampler2D(Tex, Samp), fsin_TexCoord);

    float diffuseIntensity = clamp(dot(fsin_Normal, -LightDirection), 0, 1);
    vec4 diffuseColor = texColor * diffuseIntensity;

    // Specular color
    vec4 specColor = vec4(0, 0, 0, 0);
    vec3 lightColor = vec3(1, 1, 1);
    float specPower = 5.0f;
    float specIntensity = 0.3f;
    vec3 vertexToEye = -normalize(fsin_Position_WorldSpace - CameraPosition);
    vec3 lightReflect = normalize(reflect(LightDirection, fsin_Normal));
    float specularFactor = dot(vertexToEye, lightReflect);
    if (specularFactor > 0)
    {
        specularFactor = pow(abs(specularFactor), specPower);
        specColor = vec4(lightColor * specIntensity * specularFactor, 1.0f) * texColor;
    }

    outputColor = diffuseColor + specColor;
    outputColor.r *= View[0][0] * Proj[0][1];
}
