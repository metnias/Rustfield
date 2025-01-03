void AddNoise_float(float Noise, float Strength, float IN, out float OUT)
{
    OUT = IN + (Noise - 0.5) * Strength;
}