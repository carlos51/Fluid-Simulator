#ifndef MY_COMPUTE_KERNELS
#define MY_COMPUTE_KERNELS
// ... tus funciones aquí ...
// Kernels SPH 2D (poly6, spiky grad, viscosity laplacian)
static const float PI = 3.14159265358979323846;

// W_poly6 (densidad) 2D
inline float W_Poly6_2D(float r, float h)
{
    if (r < 0.0 || r > h) return 0.0;

    float h2 = h * h;
    float r2 = r * r;
    float factor = 4.0 / (PI * pow(h, 8.0)); // normalización 2D (ajustable)
    float term = h2 - r2;
    return factor * term * term * term; // (h^2 - r^2)^3
}

inline float SpikyKernelPow2(float r, float h)
{
    float SpikyPow2ScalingFactor = 6 / (PI * pow(h, 4));
    if (r < h)
    {
        float v = h - r;
        return v * v * SpikyPow2ScalingFactor;
    }
    return 0;
}

inline float SpikyKernelPow3(float r, float h)
{
    if (r < 0.0 || r > h) return 0.0;
    // constant chosen so that derivative matches Grad_W_Spiky_2D implementation
    float A = 10.0 / (PI * pow(h, 5.0));
    float diff = h - r;
    return A * diff * diff * diff; // (h - r)^3
}

float DerivativeSpikyPow3(float dst, float radius)
{
	float SpikyPow3DerivativeScalingFactor = 30 / (PI * pow(radius, 5));
    if (dst <= radius)
    {
        float v = radius - dst;
        return -v * v * SpikyPow3DerivativeScalingFactor;
    }
    return 0;
}

float DerivativeSpikyPow2(float dst, float radius)
{
	float SpikyPow2DerivativeScalingFactor = 12 / (PI * pow(radius, 4));
    if (dst <= radius)
    {
        float v = radius - dst;
        return -v * SpikyPow2DerivativeScalingFactor;
    }
    return 0;
}



// Laplaciano W_viscosity 2D
inline float Laplacian_W_Viscosity_2D(float r, float h)
{
    if (r < 0.0 || r > h) return 0.0;
    float factor = 40.0 / (PI * pow(h, 5.0)); // normalización 2D (ajustable)
    return factor * (h - r);
}


#endif