#version 330

// Input from vertex shader
in vec3 fragPosition;
in vec2 fragTexCoord;
in vec4 fragColor;
in vec3 fragNormal;

// Material properties
uniform sampler2D texture0;
uniform vec4 colDiffuse;

// Light properties
uniform vec4 ambient;
uniform vec3 lightPos;
uniform vec4 lightColor;

// Output
out vec4 finalColor;

void main()
{
    // Basis-Farbe (Textur * Material-Farbe * Vertex-Farbe)
    vec4 texelColor = texture(texture0, fragTexCoord);
    vec3 baseColor = texelColor.rgb * colDiffuse.rgb * fragColor.rgb;
    
    // Normale normalisieren
    vec3 normal = normalize(fragNormal);
    
    // Lichtrichtung berechnen
    vec3 lightDir = normalize(lightPos - fragPosition);
    
    // Diffuse Beleuchtung (Lambertian)
    float NdotL = max(dot(normal, lightDir), 0.0);
    
    // Finale Farbe = Ambient + Diffuse
    vec3 ambientColor = ambient.rgb * baseColor;
    vec3 diffuseColor = lightColor.rgb * baseColor * NdotL;
    
    vec3 finalRGB = ambientColor + diffuseColor;
    
    finalColor = vec4(finalRGB, texelColor.a * colDiffuse.a * fragColor.a);
}