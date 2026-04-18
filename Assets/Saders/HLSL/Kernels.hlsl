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

inline float W_Poly6_2D_vec(float2 rVec, float h)
{
    return W_Poly6_2D(length(rVec), h);
}

// grad W_spiky (presión) 2D
inline float2 Grad_W_Spiky_2D(float2 rVec, float h)
{
    float r = length(rVec);
    if (r <= 0.0 || r > h) return float2(0.0, 0.0);
    float factor = -30.0 / (PI * pow(h, 5.0)); // normalización 2D (ajustable)
    float coeff = factor * (h - r) * (h - r) / r; // (h-r)^2 / r
    return coeff * rVec;
}

// Laplaciano W_viscosity 2D
inline float Laplacian_W_Viscosity_2D(float r, float h)
{
    if (r < 0.0 || r > h) return 0.0;
    float factor = 40.0 / (PI * pow(h, 5.0)); // normalización 2D (ajustable)
    return factor * (h - r);
}

#endif