shader_type canvas_item;

uniform float sky_gradient_steps = 10.0;
uniform float time: hint_range(0.0, 1.0) = 0.0;
uniform vec4 dawn_start: hint_color;
uniform vec4 dawn_end: hint_color;
uniform vec4 noon_start: hint_color;
uniform vec4 noon_end: hint_color;
uniform vec4 dusk_start: hint_color;
uniform vec4 dusk_end: hint_color;
uniform vec4 night_start: hint_color;
uniform vec4 night_end: hint_color;
uniform float cloud_speed = .1;
uniform float game_speed = 1f;

void get_time_colours(in int current_time, out vec4 col1, out vec4 col2)
{
	switch(current_time)
	{
		case 0:
		case 4:
			col1 = night_start;
			col2 = night_end;
			break;
		case 1:
			col1 = dawn_start;
			col2 = dawn_end;
			break;
		case 2:
			col1 = noon_start;
			col2 = noon_end;
			break;
		case 3:
			col1 = dusk_start;
			col2 = dusk_end;
			break;
	}
}

void fragment()
{	
	vec4 sky_start_now = vec4(0);
	vec4 sky_start_next = vec4(0);
	vec4 sky_end_now = vec4(0);
	vec4 sky_end_next = vec4(0);
	
	int time_of_day = int(time * 4.0);
	
	get_time_colours(time_of_day, sky_start_now, sky_end_now);
	get_time_colours((time_of_day + 1) % 4, sky_start_next, sky_end_next);
	
	float sky_mix = (time * 4.0) - float(time_of_day);
	
	int stepped_uv = int(UV.y * sky_gradient_steps);
	vec4 sky_end = mix(sky_end_now, sky_end_next, sky_mix);
	vec4 sky_colour = mix(mix(sky_start_now, sky_start_next, sky_mix), sky_end, float(stepped_uv) / sky_gradient_steps);
	
	// add stars
	float star_sample = texture(TEXTURE, UV).g;
	float noise_sample = texture(TEXTURE, UV + vec2(TIME * .2, 0)).b;
	float stars_amount = 0f;
	if(time <= .25 || time >= .75)
	{
		stars_amount = time;
		
		if(time <= .25)
			stars_amount = (1.0 - stars_amount);
		
		stars_amount /= .75f;
		stars_amount--;
		stars_amount *= 3.0;
	}

	sky_colour = mix(sky_colour, vec4(1),  noise_sample * star_sample * stars_amount);
	
	vec4 cloud_colour = texture(TEXTURE, UV + vec2(fract(TIME * cloud_speed * game_speed), 0)).r * sky_end; 
	COLOR = mix(sky_colour, cloud_colour, cloud_colour.a);
}