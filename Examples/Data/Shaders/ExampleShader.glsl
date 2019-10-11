-- Vertex
#version 140
in vec3 InPosition;
uniform mat4 ModelViewProjectionMatrix;

void main()
{
	gl_Position = ModelViewProjectionMatrix * vec4(InPosition,1);
}

-- Fragment
#version 140
out vec4 FragColor;

void main()
{
	FragColor = vec4(1);
}