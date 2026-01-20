shader_type spatial;
render_mode unshaded;

uniform vec2 position;
uniform float circle_radius : hint_range(0.0, 1.0, 0.001);
uniform float circle_feather : hint_range(0.0, 1.0, 0.001);
uniform vec4 color_background : source_color = vec4(0, 0, 0, 1.0);
uniform vec4 color_circle : source_color = vec4(0, 0, 0, 1.0);

float circle(vec2 center, float radius, float feather)
{
	return smoothstep(radius, radius + feather, dot(center, center) * 4.0);
}

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	float f = circle(UV - position, circle_radius, circle_feather);
	vec4 col = mix(color_circle, color_background, f);
	ALBEDO = col.rgb;
	ALPHA = 1.0 - f;
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
