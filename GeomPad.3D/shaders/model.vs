
#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 in_textureCoords;
layout (location = 2) in vec3 in_normal;
layout (location = 3) in vec3 in_color;

out vec2 pass_textureCoords;
out vec3 pass_normal;
out vec3 pass_color;

uniform mat4 transform;
uniform mat4 model;

void main()
{

    gl_Position = transform*model* vec4(aPos.x, aPos.y, aPos.z, 1.0);
	pass_normal = mat3(transpose(inverse(model))) * in_normal;  	
	

	//pass_normal = in_normal.xyz;
	pass_textureCoords = in_textureCoords;
	pass_color = in_color;

}