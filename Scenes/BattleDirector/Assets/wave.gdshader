shader_type canvas_item;

uniform float fillLevel : hint_range(0.0, 1.0) = 0.5;
uniform float waveIntensity : hint_range(0.0, 3.0) = 1.0;
uniform sampler2D waveGradient;
uniform vec4 backWaveTint : source_color = vec4(0.7, 0.7, 0.8, 1.0);

void fragment() {
	vec2 centeredUV = UV * 2.0 - 1.0;

	vec4 outputColor = vec4(0.0, 0.0, 0.0, 1.0);

	//Only do stuff if the bar is filled
	if (fillLevel > 0.0) {

		////**** Gradient Texture Sampling ****////

		//Gets the centered pos of the fillLevel
		float centeredFillLevel = mix(1.0, -1.0, fillLevel);

		//Calculate where we should sample the gradient for this section of the water
		float gradientPosition = fillLevel - (centeredUV.y - centeredFillLevel) / 2.0;

		//Finally get the color from the gradient
		vec4 sampledColorFromGradient = texture(waveGradient, vec2(gradientPosition, 0.5));

		////**** Wave Sim ****////

		float waveBase = smoothstep(0.1, 0.9, sin(centeredUV.x * 2.0 + PI * 0.5) - 0.3);

		float centerWaveAmp = 0.05 * waveIntensity; //How much larger the center wave should be
		float mainWaveAmp = 0.04 * waveIntensity; //Overall sin amplitude
		float surfaceRippleAmp = 0.03 * waveIntensity; //How much the two waves should vary
		float verticalMovement = 0.03 * waveIntensity; //How much it moves vertically

		//Flatten stuff out if the bar is full
		if (fillLevel > 0.9999) {
			centerWaveAmp = 0.0;
			mainWaveAmp = 0.0;
			surfaceRippleAmp = 0.0;
			verticalMovement = 0.0;
		}

		//How far from the the bottom the waves should appear
		float waveHeight = mix(1.0, -1.0, fillLevel) + sin(TIME * PI * 0.5) * verticalMovement;

		//Offset wave position and change direction
		float frontWaveYOffset = sin((TIME * 2.0 + centeredUV.x * 3.0) * 2.0) * mainWaveAmp;
		float backWaveYOffset  = sin((TIME * -2.0 + centeredUV.x * 3.0 + PI) * 2.0) * mainWaveAmp;

		//Calc surface ripples
		float surfaceAmplitudes = sin(TIME * 3.0) * surfaceRippleAmp * waveBase;

		//Final Y positions of the waves
		float frontWaveY = waveHeight - (surfaceAmplitudes + frontWaveYOffset);
		float backWaveY  = waveHeight - (-surfaceAmplitudes + backWaveYOffset);

		//Determine which wave is in front for this fragment
		float isBelowFrontWave = step(frontWaveY, centeredUV.y);
		float isBelowBackWave  = step(backWaveY, centeredUV.y);

		float backOnlyVisibility = clamp(isBelowBackWave - isBelowFrontWave, 0.0, 1.0);

		if (isBelowFrontWave > 0.5) {
			outputColor = sampledColorFromGradient; //Just assign the color
		} else if (backOnlyVisibility > 0.5) {
			outputColor = vec4(sampledColorFromGradient.rgb * backWaveTint.rgb,sampledColorFromGradient.a * backWaveTint.a);
		}
	}

	COLOR = outputColor;
}