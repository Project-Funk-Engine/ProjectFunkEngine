shader_type canvas_item;

uniform float height = 5.0;
uniform float speed = 5.0f;
uniform float freq = 10.0f;

uniform vec3 bg_top_color;
uniform vec3 bg_bottom_color;
uniform float gradient_ratio;
uniform float time_scale;

float rand(vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898,78.233))) * 43758.5453123);
}

void vertex() {
	// Called for every vertex the material is visible on.
	VERTEX.y -= height * sin((TIME * speed) + (freq * UV.x));
}

void fragment() {

	float color = 0.0f;

	if (COLOR.rgb == vec3(1)){
		if (rand(SCREEN_UV.xy / 20.0) > 0.996){
			COLOR.rbg = vec3(1);
		}else {
			vec3 gradient = mix(bg_top_color, bg_bottom_color, SCREEN_UV.y / gradient_ratio);
			COLOR.rgb = gradient;
		}
		/*
		if (rand(SCREEN_UV.xy / 20.0) > 0.996)
		{
			float r = rand(SCREEN_UV.xy);
			color = r * (0.85 * sin((TIME * time_scale) * (r * 5.0) + 720.0 * r) + 0.95);
		}

		vec4 gradient_color = mix(bg_top_color, bg_bottom_color, SCREEN_UV.y / gradient_ratio);
		COLOR = vec4(vec3(color),1.0) + gradient_color;
		*/


	}
	// Called for every pixel the material is visible on.
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
