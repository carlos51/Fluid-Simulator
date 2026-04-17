using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{
    //public Sprite sprite;

    [HideInInspector] public float mass = 1f;
    //[HideInInspector] 
    public Vector2 v = Vector2.zero;
    //[HideInInspector] 
    public  float rho;
    //[HideInInspector] 
    public Vector2 f = Vector2.zero;
    //[HideInInspector] 
    public Vector2 a;
    public float w;
    //public float h;
    //[HideInInspector] 
    public float presure;
    //[HideInInspector] 
    public int id;

    public Vector2 r
    {
        get { return transform.position; }
        set { transform.position = value; }
    }
    public void applyG(Vector2 G)
    {
        a += G;
    }

    public void applyForce(float dt, Vector2 G)
    {
        a = f/rho;
        //applyG(Vector2.down * G);
        a += G;
        v += a * dt;
        r += v * dt;
    }

    public void ConfineParticle(Vector3 minBounds, Vector3 maxBounds, float bounce = 0.8f)
    {
        Vector3 pos = r;
        Vector3 vel = v;

        // Eje X
        if (pos.x < minBounds.x)
        {
            pos.x = minBounds.x;   // reposicionar dentro
            vel.x *= -bounce;      // rebote con amortiguación
        }
        else if (pos.x > maxBounds.x)
        {
            pos.x = maxBounds.x;
            vel.x *= -bounce;
        }

        // Eje Y
        if (pos.y < minBounds.y)
        {
            pos.y = minBounds.y;
            vel.y *= -bounce;
        }
        else if (pos.y > maxBounds.y)
        {
            pos.y = maxBounds.y;
            vel.y *= -bounce;
        }

        // Eje Z
        if (pos.z < minBounds.z)
        {
            pos.z = minBounds.z;
            vel.z *= -bounce;
        }
        else if (pos.z > maxBounds.z)
        {
            pos.z = maxBounds.z;
            vel.z *= -bounce;
        }

        // Actualizamos la partícula
        r = pos;
        v = vel;
    }

    public void CalculatePresure(float targetDensity, float k)
    {
        presure = (rho - targetDensity) * k;
    }

}
