#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 in_textureCoords;
layout (location = 2) in vec3 aNormal;
layout (location = 3) in vec3 in_color;


out vec3 FragPos;
out vec3 Normal;
out vec3 light_Pos;

uniform mat4 transform;
out vec3 pass_color;
uniform mat4 model;
uniform vec3 lightPos; 
//uniform mat4 view;
//uniform mat4 projection;

void main()
{

    FragPos = vec3(model * vec4(aPos, 1.0));
    Normal = mat3(transpose(inverse(model))) * aNormal;  	
    
    
    //gl_Position = projection * view * vec4(FragPos, 1.0);
	gl_Position = transform* vec4(FragPos, 1.0);

	pass_color = in_color;
}