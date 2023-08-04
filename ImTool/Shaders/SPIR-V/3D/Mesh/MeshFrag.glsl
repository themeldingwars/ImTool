#version 450

layout(location = 0) in vec2 Uvs;
layout(location = 1) in vec2 NormUvs;
layout(location = 2) in vec3 Norm;
layout(location = 3) in vec3 Tang;
layout(location = 4) flat in int Color;
layout(location = 5) in vec3 FragPos;

layout(location = 0) out vec4 outColor;

void main() {
    //outColor = vec4(1, 1, 1, 1);
    
    vec3 norm = normalize(Norm);
    vec3 lightDir = normalize(vec3(0, 10, 0) - FragPos);
    
    float diff = max(dot(norm, lightDir), 0.0) + 0.2f;
    vec3 diffuse = diff * vec3(0.5, 0.1, 0.7);

    outColor = vec4(diffuse, 1);
    //outColor = vec4(1, 1, 1, 1);
}