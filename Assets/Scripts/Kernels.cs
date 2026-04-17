using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kernels : MonoBehaviour
{
    // Constantes precomputadas para eficiencia
    // Ten en cuenta que pi = 3.14159265359f
    static float PI = Mathf.PI;

    // Poly6 Kernel (densidad)
    public static float Poly6(float r, float h)
    {
        if (r >= 0 && r <= h)
        {
            float hr2 = (h * h - r * r);
            return (315f / (64f * PI * Mathf.Pow(h, 9))) * hr2 * hr2 * hr2;
        }
        return 0f;
    }

    // Gradiente del Spiky Kernel (para presion)
    public static Vector2 SpikyGradient(Vector2 rij, float h)
    {
        float r = rij.magnitude;
        if (r > 0 && r <= h)
        {
            float factor = -45f / (PI * Mathf.Pow(h, 6)) * Mathf.Pow(h - r, 2) / r;
            return factor * rij;
        }
        return Vector2.zero;
    }

    // Laplaciano del Viscosity Kernel (para viscosidad)
    public static float ViscosityLaplacian(float r, float h)
    {
        if (r >= 0 && r <= h)
        {
            return 45f / (PI * Mathf.Pow(h, 6)) * (h - r);
        }
        return 0f;
    }

    // Gradiente del Poly6 (si llegas a necesitarlo para fuerzas de superficie)
    public static Vector2 Poly6Gradient(Vector2 rij, float h)
    {
        float r = rij.magnitude;
        if (r > 0 && r <= h)
        {
            float factor = -945f / (32f * PI * Mathf.Pow(h, 9)) * Mathf.Pow(h * h - r * r, 2);
            return factor * rij;
        }
        return Vector2.zero;
    }

    // kernel para correccion de posisciones
    public static float lineal(Vector2 rij, float h)
    {
        float r = rij.magnitude;
        float val = (1 - r) / h;

        return Mathf.Max(0,val);
    }
}
