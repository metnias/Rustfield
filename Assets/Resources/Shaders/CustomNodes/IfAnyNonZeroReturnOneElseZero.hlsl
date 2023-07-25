void IfAnyNonZeroReturnOneElseZero_float(float A, float B, float C, float D, out float OUT)
{
    OUT = 0;
    if (A != 0) OUT = 1;
    if (B != 0) OUT = 1;
    if (C != 0) OUT = 1;
    if (D != 0) OUT = 1;
}

void IfAnyNonZeroReturnOneElseZero_half(half A, half B, half C, half D, out half OUT)
{
    OUT = 0;
    if (A != 0) OUT = 1;
    if (B != 0) OUT = 1;
    if (C != 0) OUT = 1;
    if (D != 0) OUT = 1;
}