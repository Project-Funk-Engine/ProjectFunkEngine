shader_type canvas_item;

uniform float time_scale;

float rand(vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898, 78.233))) * 43758.5453123);
}

void fragment() {
    float star_intensity = 0.0;
    float alpha = 0.0;

    if (rand(SCREEN_UV.xy / 20.0) > 0.996) {
        float r = rand(SCREEN_UV.xy);
        star_intensity = r * (0.85 * sin((TIME * time_scale) * (r * 5.0) + 720.0 * r) + 0.95);
        alpha = star_intensity; 
    }
    
    COLOR = vec4(vec3(star_intensity), alpha);
}
