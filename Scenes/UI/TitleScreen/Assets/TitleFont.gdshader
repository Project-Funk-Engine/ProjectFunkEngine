shader_type canvas_item;

uniform float height = 5.0;
uniform float speed = 5.0f;
uniform float freq = 10.0f;

void vertex() {
	VERTEX.y -= height * sin((TIME * speed) + (freq * UV.x));
}