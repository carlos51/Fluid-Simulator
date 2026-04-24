using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPH_Manager : MonoBehaviour
{
    public int population;
    public float particleSize;
    public float range;
    public Vector3 BoxMin;
    public Vector3 BoxMax;
    public float H; // kernel radius
    public float k; // stiffness constant
    public float targetDensity;
    public float viscosity;
    public float G; // gravity constant
    public Material material;



    
    private ComputeBuffer argsBuffer;
    private ComputeBuffer velocitiesBuffer;
    private ComputeBuffer positionsBuffer;
    public ComputeShader computeShader;
    private ComputeBuffer colorsBuffer;
    private ComputeBuffer forcesBuffer;
    private ComputeBuffer densitiesBuffer;
    

    public Mesh mesh;
    private Bounds bounds;
    private int integrateKernel;
    private int forceKernel;
    private int densityKernel;

    // MeshProperties removed — no longer used for per-instance data from CPU.

    private void Setup()
    {
        

        // Boundary surrounding the meshes we will be drawing.  Used for occlusion.
        bounds = new Bounds(transform.position, Vector3.one * (range + 1));
        
        InitializeBuffers();
    }

    private void InitializeBuffers()
    {
        integrateKernel = computeShader.FindKernel("Integrate");
        densityKernel = computeShader.FindKernel("CalculateDensity");
        forceKernel = computeShader.FindKernel("CalculateForce");
        // Argument buffer used by DrawMeshInstancedIndirect.
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        // Arguments for drawing mesh.
        // 0 == number of triangle indices, 1 == population, others are only relevant if drawing submeshes.
        args[0] = (uint)mesh.GetIndexCount(0);
        args[1] = (uint)population;
        args[2] = (uint)mesh.GetIndexStart(0);
        args[3] = (uint)mesh.GetBaseVertex(0);
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        // Initialize buffers for the given population.
        Vector4[] velocities = new Vector4[population];
        Vector4[] positions = new Vector4[population];
        Vector4[] forces = new Vector4[population];

        float[] densities = new float[population];
        Vector4[] colors = new Vector4[population];


        // Arrange particles in a centered 2D grid covering [-range, range]
        // fill particle arrays (grid by default)
        //SpawnGrid(positions, velocities, colors, forces);
        SpawnRandom(positions, velocities, colors, forces);


        colorsBuffer = new ComputeBuffer(population, sizeof(float) * 4);
        colorsBuffer.SetData(colors);
        material.SetBuffer("_Colors", colorsBuffer);
        computeShader.SetBuffer(integrateKernel, "_Colors",colorsBuffer);

        forcesBuffer = new ComputeBuffer(population, sizeof(float) * 4);
        forcesBuffer.SetData(forces);
        computeShader.SetBuffer(integrateKernel, "_Forces", forcesBuffer);
        computeShader.SetBuffer(forceKernel, "_Forces", forcesBuffer);
        computeShader.SetBuffer(densityKernel, "_Forces", forcesBuffer);


        velocitiesBuffer = new ComputeBuffer(population, sizeof(float) * 4);
        velocitiesBuffer.SetData(velocities);
        computeShader.SetBuffer(integrateKernel, "_Velocities", velocitiesBuffer);
        computeShader.SetBuffer(forceKernel, "_Velocities", velocitiesBuffer);
        computeShader.SetBuffer(densityKernel, "_Velocities", velocitiesBuffer);

        positionsBuffer = new ComputeBuffer(population, sizeof(float) * 4);
        positionsBuffer.SetData(positions);
        computeShader.SetBuffer(integrateKernel, "_Positions", positionsBuffer);
        computeShader.SetBuffer(forceKernel, "_Positions", positionsBuffer);
        computeShader.SetBuffer(densityKernel, "_Positions", positionsBuffer);

        densitiesBuffer = new ComputeBuffer(population, sizeof(float));
        computeShader.SetBuffer(integrateKernel, "_Densities", densitiesBuffer);
        computeShader.SetBuffer(forceKernel, "_Densities", densitiesBuffer);
        computeShader.SetBuffer(densityKernel, "_Densities", densitiesBuffer);

        material.SetBuffer("_Positions", positionsBuffer);
        material.SetFloat("_ParticleSize", particleSize);
        material.SetBuffer("_Colors", colorsBuffer);
        material.SetFloat("_Size", particleSize);
        material.SetBuffer("_Densities",densitiesBuffer);

        computeShader.SetVector("_BoxMin", BoxMin);
        computeShader.SetVector("_BoxMax", BoxMax);
        computeShader.SetInt("_NumParticles", population);
        computeShader.SetFloat("_H", H);
        computeShader.SetFloat("_K", k);
        computeShader.SetFloat("_TargetDensity", targetDensity);
        computeShader.SetFloat("_G", G);
        computeShader.SetFloat("_Viscosity", viscosity);




    }


    private void Start()
    {
        Setup();
    }

    // Fill arrays with particles arranged in a centered grid
    private void SpawnGrid(Vector4[] positions, Vector4[] velocities, Vector4[] colors, Vector4[] forces)
    {
        int cols = Mathf.CeilToInt(Mathf.Sqrt(population));
        int rows = Mathf.CeilToInt((float)population / cols);

        float spacingX = (cols > 1) ? (2f * range) / (cols) : 0f;
        float spacingY = (rows > 1) ? (2f * range) / (rows) : 0f;

        for (int i = 0; i < population; i++)
        {
            int col = i % cols;
            int row = i / cols;
            float x = -range + (col + 0.5f) * spacingX;
            float y = -range + (row + 0.5f) * spacingY;
            positions[i] = new Vector4(x, y, 0f, 0f);
            velocities[i] = Vector4.zero;
            colors[i] = Color.Lerp(Color.red, Color.blue, UnityEngine.Random.value);
            forces[i] = Vector4.zero;
        }
    }

    // Fill arrays with particles in random positions within [-range, range]
    private void SpawnRandom(Vector4[] positions, Vector4[] velocities, Vector4[] colors, Vector4[] forces)
    {
        for (int i = 0; i < population; i++)
        {
            float x = UnityEngine.Random.Range(-range, range);
            float y = UnityEngine.Random.Range(-range, range);
            positions[i] = new Vector4(x, y, 0f, 0f);
            velocities[i] = Vector4.zero;
            colors[i] = Color.Lerp(Color.red, Color.blue, UnityEngine.Random.value);
            forces[i] = Vector4.zero;
        }
    }

    private void Update()
    {
        // update shader/material parameters from inspector
        UpdateShaderParams();

        computeShader.SetFloat("_DeltaTime", Time.deltaTime);

        computeShader.Dispatch(densityKernel, population / 64 + 1, 1, 1);
        computeShader.Dispatch(forceKernel, population / 64 + 1, 1, 1);
        computeShader.Dispatch(integrateKernel, population / 64 + 1, 1, 1);
        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer);
    }

    // Set compute shader and material parameters so inspector changes take effect at runtime
    private void UpdateShaderParams()
    {
        computeShader.SetVector("_BoxMin", BoxMin);
        computeShader.SetVector("_BoxMax", BoxMax);
        computeShader.SetInt("_NumParticles", population);
        computeShader.SetFloat("_H", H);
        computeShader.SetFloat("_K", k);
        computeShader.SetFloat("_TargetDensity", targetDensity);
        computeShader.SetFloat("_G", G);
        computeShader.SetFloat("_Viscosity", viscosity);

        // update material properties
        if (material != null)
        {
            material.SetFloat("_ParticleSize", particleSize);
            material.SetFloat("_Size", particleSize);
        }
    }

    private void OnDisable()
    {
        // Release gracefully.

        if (argsBuffer != null)
        {
            argsBuffer.Release();
        }
        argsBuffer = null;
        if (velocitiesBuffer != null)
        {
            velocitiesBuffer.Release();
        }
        velocitiesBuffer = null;
        if (positionsBuffer != null)
        {
            positionsBuffer.Release();
        }
        positionsBuffer = null;
        if (colorsBuffer != null)
        {
            colorsBuffer.Release();
        }
        colorsBuffer = null;
        if (forcesBuffer != null)
        {
            forcesBuffer.Release();
        }
        forcesBuffer = null;
        if (densitiesBuffer != null)
        {
            densitiesBuffer.Release();
        }
        densitiesBuffer = null;

    }


}
