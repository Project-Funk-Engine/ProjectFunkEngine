shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

void fragment() {
	vec4 screen = texture(SCREEN_TEXTURE, SCREEN_UV);

	COLOR.rgb =vec3(0.6 * screen.r + 0.6 * screen.g + 0.2 * screen.b);
}

