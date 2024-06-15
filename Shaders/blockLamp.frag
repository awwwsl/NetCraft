#version 430 core
out vec4 FragColor;

uniform vec3 fragColor;

void main()
{
    FragColor = vec4(fragColor, 1.0);
}
