using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public SPH_Manager sph;
    public GameObject sph_menu;
    public GameObject ia_menu;
    [Header("Imágenes del Botón")]
    [SerializeField] private Sprite imagenPlay;    // Arrastra aquí la imagen con el símbolo de Play
    [SerializeField] private Sprite imagenPause;

    [Header("Referencias de UI")]
    [SerializeField] private Image imagenDelBoton;

    public void Play()
    {
        sph.Running = !sph.Running;
        ActualizarImagenDelBoton();
    }

    public void Pause()
    {
        sph.Running = false;
    }
    
    public void ChooseSim(int index)
    {
        if (index == 0)
        {
            sph_menu.SetActive(true);
            ia_menu.SetActive(false);
        }
        else if (index == 1)
        {
            sph_menu.SetActive(false);
            ia_menu.SetActive(true);
        }
    }

    private void ActualizarImagenDelBoton()
    {
        // Verificamos el estado y asignamos el Sprite correspondiente
        if (sph.Running)
        {
            imagenDelBoton.sprite = imagenPause; // Si estamos en Play, mostramos el icono de Pause
        }
        else
        {
            imagenDelBoton.sprite = imagenPlay;  // Si estamos en Pause, mostramos el icono de Play
        }
    }

    public void SetViscosity(float value)
    {
        sph.viscosity = value;
    }

    public void SetRestDensity(string value)
    {
        if (float.TryParse(value, out float result))
        {
            sph.targetDensity = result;
        }
    }

    public void SetGravity(string value)
    {
        if (float.TryParse(value, out float result))
        {
            sph.G = result;
        }
    }

    public void SetPopulation(string value)
    {
        if (int.TryParse(value, out int result))
        {
            sph.population = result;
        }
    }

    public void SetParticleSize(string value)
    {
        if (float.TryParse(value, out float result))
        {
            sph.particleSize = result;
        }
    }

    public void SetSubSteps(string value)
    {
        if (int.TryParse(value, out int result))
        {
            sph.subSteps = result;
        }
    }

    public void SetKernelRadius(float value)
    {
        sph.H = value;
    }

    public void SetStiffness(string value)
    {
        if (float.TryParse(value, out float result))
        {
            sph.k = result;
        }
    }

    public void SetNearStiffness(string value)
    {
        if (float.TryParse(value, out float result))
        {
            sph.kNear = result;
        }
    }

    public void SetDamping(float value)
    {
        sph.damping = value;
    }

    public void NextStep()
    {
        
        if (!sph.Running)
        {
            Debug.Log("Next Step");
            sph.StepOnce();
        }
    }

    public void ResetSimulation()
    {
        Debug.Log("Reset Simulation");
        sph.Restart();
    }
}

