#version 330

// Input from vertex shader
in vec3 fragPosition;
in vec2 fragTexCoord;
in vec4 fragColor;
in vec3 fragNormal;

// Output
out vec4 finalColor;

// Uniforms
uniform sampler2D texture0;
uniform vec4 colDiffuse;

// ===== LIGHTING UNIFORMS =====
uniform vec4 ambient;           // Ambient light color
uniform vec3 lightDir;          // Directional light direction
uniform vec4 lightColor;        // Directional light color

// ===== SHADOW FUNCTION =====
float calculateShadow(vec3 normal, vec3 lightDirection)
{
    // Einfaches Contact Shadow (kein echtes Shadow Mapping)
    float NdotL = dot(normal, lightDirection);
    
    // Wenn Oberfläche von Licht abgewandt ist
    if (NdotL < 0.0)
        return 0.3;  // Dunklerer Schatten
    
    return 1.0;  // Volle Beleuchtung
}

void main()
{
    // Sample texture
    vec4 texelColor = texture(texture0, fragTexCoord);
    
    // Combine with vertex color and diffuse
    vec4 baseColor = texelColor * fragColor * colDiffuse;
    
    // ===== AMBIENT LIGHTING =====
    vec3 ambientLight = ambient.rgb * ambient.a;
    
    // ===== DIRECTIONAL LIGHTING =====
    vec3 normal = normalize(fragNormal);
    vec3 lightDirection = normalize(-lightDir);
    
    // Diffuse (Lambertian)
    float diff = max(dot(normal, lightDirection), 0.0);
    
    // ===== SHADOW CALCULATION =====
    float shadow = calculateShadow(normal, lightDirection);
    
    vec3 diffuseLight = lightColor.rgb * lightColor.a * diff * shadow;
    
    // ===== SOFT LIGHTING (weniger harte Schatten) =====
    float wrap = 0.4;
    float wrapDiff = max(0.0, (dot(normal, lightDirection) + wrap) / (1.0 + wrap));
    
    // ===== RIM LIGHTING (subtle glow an Kanten) =====
    vec3 viewDir = normalize(vec3(0.0, 10.0, 10.0) - fragPosition);
    float rim = 1.0 - max(dot(viewDir, normal), 0.0);
    rim = pow(rim, 3.0) * 0.15;
    vec3 rimLight = lightColor.rgb * rim;
    
    // ===== AMBIENT OCCLUSION (einfach) =====
    float ao = 1.0 - (fragPosition.y * 0.01);  // Dunkler weiter unten
    ao = clamp(ao, 0.7, 1.0);
    
    // ===== COMBINE ALL LIGHTING =====
    vec3 lighting = (ambientLight * ao) + (diffuseLight * 0.7) + rimLight;
    lighting = lighting * (0.5 + wrapDiff * 0.5);
    
    // ===== FINAL COLOR =====
    vec3 finalRGB = baseColor.rgb * lighting;
    
    // Slight color grading für cozy feel
    finalRGB = pow(finalRGB, vec3(0.95));
    
    finalColor = vec4(finalRGB, baseColor.a);
}