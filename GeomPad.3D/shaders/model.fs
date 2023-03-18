#version 330 core
const vec2 lightBias = vec2(0.6, 0.7);//just indicates the balance between diffuse and ambient lighting

in vec2 pass_textureCoords;
in vec3 pass_normal;
in vec3 pass_color;


out vec4 FragColor;

uniform sampler2D diffuseMap;
uniform bool useColors;
//uniform vec4 color;
uniform float mult;
uniform float opacity;
uniform vec3 lightDirection;

void main()
{
//FragColor=vec4(1,1,1,1);
//FragColor=vec4(pass_color.x,pass_color.y,pass_color.z,1);

 //vec3 lightDirection=vec3(1,1,-1); 
 //lightDirection=normalize(lightDirection);
 vec3 lightDirection2=vec3(0,-1,-2);

vec4 diffuseColour = texture(diffuseMap, pass_textureCoords);		
    //FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);
	vec3 unitNormal = normalize(pass_normal);
	float diffuseLight = max(dot(-lightDirection, unitNormal), 0.0) * lightBias.x + lightBias.y;
		float diffuseLight2 = max(dot(-lightDirection2, unitNormal), 0.0) * lightBias.x + lightBias.y;
	//FragColor = diffuseColour * diffuseLight;
	if(!useColors){
	//FragColor = mult*mix(diffuseColour *diffuseLight,diffuseColour *diffuseLight2,0.5f);
	FragColor = mult*diffuseColour *diffuseLight;
	}else{
	
	//FragColor = mult*mix(vec4(pass_color,1)*diffuseLight,vec4(pass_color,1)*diffuseLight2,0.5f); 
	FragColor = mult*vec4(pass_color,opacity)*(diffuseLight);

	//FragColor=vec4(pass_color.x,pass_color.y,pass_color.z,1);
	}
	
} 