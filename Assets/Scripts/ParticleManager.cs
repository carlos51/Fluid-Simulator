using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{

    public float h = 2;
    public float k = 500;
    
    public int cantidad = 10; // Cuántos vectores queremos
    public float min = -5f;   // Valor mínimo para cada componente
    public float max = 5f;    // Valor máximo para cada componente
    public float targetDensity = 30;
    public float G = 4;
    public Kernels Kernels;

    private List<Particle> particles = new List<Particle>();
    private List<Particle> gosthParticles = new List<Particle>();
    public GameObject particlePre;
    public Vector2[] particleBuffer;

    void Start()
    {
        //int filas = Mathf.CeilToInt(Mathf.Sqrt(cantidad));  // número de filas
        //int columnas = Mathf.CeilToInt((float)cantidad / filas); // número de columnas

        float espaciado = 0.5f; // distancia entre partículas en la grilla
        Vector2 origen = new Vector2(min, min);

        particleBuffer = new Vector2[cantidad*cantidad];
        for (int i = 0; i < cantidad * cantidad; i++)
        {
            int fila = i / cantidad;
            int columna = i % cantidad;

            Vector2 v = origen + new Vector2(columna * espaciado, fila * espaciado);

            GameObject particle = Instantiate(particlePre, v, Quaternion.identity);

            Particle p = particle.GetComponent<Particle>();
            p.id = i;

            particleBuffer[i] = p.r;

            particles.Add(p);

        }

        
    }

    // Update is called once per frame
    void Update()
    {
        //float dt = 0.005f * 2;
        float dt = Time.deltaTime;


        for (int i = 0; i < particles.Count; i++)
        {
            // Calculamos la dencidad
            particles[i].rho = dencidad(i);

        }

        for (int i = 0; i < particles.Count; i++)
        {
            // Calculamos la presion
            particles[i].CalculatePresure(targetDensity,k);

        }


        for (int i = 0; i < particles.Count; i++)
        {
            // Calculamos la fuerza de presion
            Vector2 ff = f_press(particles[i]);
            particles[i].f = ff;
        }

        // Calculamos los dezplazamientos

        for (int i = 0; i < particles.Count; i++)
        {

            particles[i].applyForce(dt, Vector2.down * G);
            particles[i].ConfineParticle(Vector2.one * min, Vector2.one * max);
    
        }
    }

 
    float dencidad(int id)
    {
        
        float p = 0f;
        float r;
        for (int i = 0; i < particles.Count; i++)
        {
            if(particles[i].id != id)
            {
                //p += W(particles[id].r - particles[i].r, h);
                r = (particles[id].r - particles[i].r).magnitude;
                p += Kernels.Poly6(r, h);
            }
            
        }
        return Mathf.Max(0.001f, p);
    } 

    Vector2 f_press(Particle par)
    {
        Vector2 force = Vector2.zero;
        for (int i = 0; i < particles.Count; i++)
        {
            if (particles[i].id != par.id)
            {
                float c = (par.presure - particles[i].presure) / (2 * particles[i].rho);
                force += c * Kernels.Poly6Gradient(par.r - particles[i].r,h) ;
            }
        }
        return force;
    }

    Vector2 despazamientos(Vector2 rij, float P_i, float dt)
    {
        float r = rij.magnitude;
        float val = dt*dt*P_i*Kernels.lineal(rij,h);

        return (val / r) * rij;
    }

    

}
