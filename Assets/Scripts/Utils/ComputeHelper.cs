using System.Collections.Generic;
using UnityEngine;

public class ComputeHelper
{
    ComputeShader computeShader;
    List<int> kernelIDs = new List<int>();

    // Cambiamos la Lista por un Diccionario para mayor control
    Dictionary<string, ComputeBuffer> buffers = new Dictionary<string, ComputeBuffer>();

    public ComputeHelper(ComputeShader computeShader)
    {
        this.computeShader = computeShader;
    }

    public void SetKernels(string[] kernelNames)
    {
        foreach (var kernelName in kernelNames)
        {
            int kernelID = computeShader.FindKernel(kernelName);

            if (kernelID != -1)
            {
                kernelIDs.Add(kernelID);
            }
            else
            {
                Debug.LogError($"Kernel '{kernelName}' no encontrado.");
            }
        }
    }

    /// <summary>
    /// Crea o actualiza un buffer y lo asigna automáticamente a todos los kernels registrados.
    /// </summary>
    public void CreateAndSetBuffer(string name, int count, int stride)
    {
        // 1. Gestión de memoria: Si ya existía un buffer con ese nombre, lo liberamos
        if (buffers.ContainsKey(name))
        {
            buffers[name].Release();
        }

        // 2. Creamos el nuevo buffer
        ComputeBuffer newBuffer = new ComputeBuffer(count, stride);
        buffers[name] = newBuffer;

        // 3. Lo vinculamos a todos los kernels
        foreach (var kernelID in kernelIDs)
        {
            computeShader.SetBuffer(kernelID, name, newBuffer);
        }
    }

    public void SetBufferData(string name, System.Array data)
    {
        if (buffers.TryGetValue(name, out ComputeBuffer buffer))
        {
            buffer.SetData(data);
        }
        else
        {
            Debug.LogError($"Buffer '{name}' no encontrado para SetData.");
        }
    }

    // Si ya tienes un buffer externo y solo quieres vincularlo
    public void SetBuffer(string bufferName, ComputeBuffer buffer)
    {
        foreach (var kernelID in kernelIDs)
        {
            computeShader.SetBuffer(kernelID, bufferName, buffer);
        }
    }

    public void Dispatch(int kernelIndex, int threadGroupsX, int threadGroupsY, int threadGroupsZ)
    {
        // Usamos el índice de la lista de kernels registrados

        if (kernelIndex < kernelIDs.Count)
        {
            computeShader.Dispatch(kernelIndex, threadGroupsX, threadGroupsY, threadGroupsZ);
        }
        else
        {
            Debug.Log("No existe el kernel");
        }
    }

    public void SetInt(string name, int value) => computeShader.SetInt(name, value);
    public void SetFloat(string name, float value) => computeShader.SetFloat(name, value);
    public void SetVector(string name, Vector2 value) => SetVector(name, (Vector4)value);
    public void SetVector(string name, Vector3 value) => SetVector(name, (Vector4)value);
    public void SetVector(string name, Vector4 value) => computeShader.SetVector(name, value);

    // Método para obtener un buffer si necesitas hacer GetData o SetData desde fuera
    public ComputeBuffer GetBuffer(string name)
    {
        if (buffers.TryGetValue(name, out ComputeBuffer b)) return b;
        return null;
    }

    public void ReleaseBuffers()
    {
        foreach (var buffer in buffers.Values)
        {
            if (buffer != null) buffer.Release();
        }
        buffers.Clear();
    }

    public void GetBufferData(string name, System.Array data)
    {
        if (buffers.TryGetValue(name, out ComputeBuffer buffer))
        {
            buffer.GetData(data);
        }
        else
        {
            Debug.LogError($"Buffer '{name}' no encontrado para GetData.");
        }
    }
}