#version 430 core
out vec4 FragColor;

uniform vec4 fragColor;

void main()
{
    FragColor = vec4(fragColor);
}
