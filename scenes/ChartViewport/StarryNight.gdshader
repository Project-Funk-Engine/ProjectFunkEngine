//based on this star shader. Thanks gerardogc2378
//https://godotshaders.com/shader/stars-shader/

shader_type canvas_item;

uniform vec4 bg_color;

float rand(vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898,78.233))) * 43758.5453123);
}

void fragment() {
	float color = 0.0;

	if (rand(SCREEN_UV.xy / 20.0) > 0.996)
	{
		float r = rand(SCREEN_UV.xy);
		color = r * (0.85 * sin(TIME * (r * 5.0) + 720.0 * r) + 0.95);
	}
	COLOR = vec4(vec3(color),1.0) + bg_color;
}