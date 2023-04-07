#version 450

layout (lines) in;                              // now we can access 2 vertices
layout (triangle_strip, max_vertices = 4) out;  // always (for now) producing 2 triangles (so 4 vertices)

layout(location = 0) in vec4 Color[];
layout(location = 1) in float Thickness[];

layout (location = 0) out vec4 outColor;

void main()
{
    vec4 p1 = gl_in[0].gl_Position;
    vec4 p2 = gl_in[1].gl_Position;
    
    vec4 v = p1;

    if (v.x < -1.0 || v.x > 1.0 || v.y < -1.0 || v.y > 1.0 || v.z < -1.0 || v.z > 1.0)
    {
        vec2 dir    = normalize((p2.xy - p1.xy) * vec2(1280, 720));
        vec2 offset = vec2(-dir.y, dir.x) * Thickness[1] / vec2(1280, 720);
    
        gl_Position = p1 + vec4(offset.xy * p1.w, 0.0, 0.0);
        outColor = Color[0];
        EmitVertex();
        
        gl_Position = p1 - vec4(offset.xy * p1.w, 0.0, 0.0);
        outColor = Color[0];
        EmitVertex();
        
        gl_Position = p2 + vec4(offset.xy * p2.w, 0.0, 0.0);
        outColor = Color[0];
        EmitVertex();
        
        gl_Position = p2 - vec4(offset.xy * p2.w, 0.0, 0.0);
        outColor = Color[0];
        EmitVertex();
    
        EndPrimitive();
    }
}