shader_type canvas_item;

float rand_from_seed(uint seed) {
	int k;
	int s = int(seed);
	if (s == 0)
	s = 305420679;
	k = s / 127773;
	s = 16807 * (s - k * 127773) - 2836 * k;
	if (s < 0)
		s += 2147483647;
	return float(seed % uint(65536)) / 65535.0;
}

float rand_from_seed_m1_p1(uint seed) {
	return rand_from_seed(seed) * 2.0 - 1.0;
}

uint hash(uint x) {
	x = ((x >> uint(16)) ^ x) * uint(73244475);
	x = ((x >> uint(16)) ^ x) * uint(73244475);
	x = (x >> uint(16)) ^ x;
	return x;
}

void fragment()
{
	COLOR = texture(SCREEN_TEXTURE, SCREEN_UV);
	uint seed = hash(uint(TIME * 7324.44750 / ((UV.r * .73244475) / UV.g * .73244475)));
	float n = rand_from_seed_m1_p1(seed);
	COLOR = vec4(n,n,n,1.0);
}