using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;

public class SPH_Manager : MonoBehaviour
{
    public int population;
    public float particleSize;
    public float range;
    public int subSteps; // number of substeps to perform per frame for stability
    public Vector3 BoxMin;
    public Vector3 BoxMax;
    public float H; // kernel radius
    public float k; // stiffness constant
    public float kNear; // near stiffness constant
    public float targetDensity;
    public float viscosity;
    public float G; // gravity constant
    [Range(0f, 1f)]
    public float damping; // velocity damping factor to prevent explosion
    public Material material;



    private ComputeHelper helper;
    private ComputeBuffer argsBuffer;
    public ComputeShader computeShader;

    public Mesh mesh;
    private Bounds bounds;
    private int integrateKernel;
    private int forceKernel;
    private int densityKernel;
    private int updateKernel;
    private int sortPairsKernel;
    private int setHashIdKernel;
    private int setIndexKernel;
    private int clearedIndexKernel;
    private const float deltaTime = 0.016f;




    private struct hashId
        {
            public int keyHash;
            public int id;
        }

    private hashId[] someArray = new hashId[8];
    private int[] ints = new int[8];
    private Vector4[] forces = new Vector4[2048];

    // MeshProperties removed — no longer used for per-instance data from CPU.

    private void Setup()
    {
        
        // Boundary surrounding the meshes we will be drawing.  Used for occlusion.
        bounds = new Bounds(transform.position, Vector3.one * (range + 1));
        
        InitializeBuffers();
    }

    private void InitializeBuffers()
    {
        string [] names = {"Integrate", "CalculateForce", "CalculateDensity", "UpdatePositions", "SortPairs", "SetHashId", "SetIndex",
        "ClearIndex"};

        helper = new ComputeHelper(computeShader);
        helper.SetKernels(names);

        integrateKernel = computeShader.FindKernel("Integrate");
        densityKernel = computeShader.FindKernel("CalculateDensity");
        forceKernel = computeShader.FindKernel("CalculateForce");
        updateKernel = computeShader.FindKernel("UpdatePositions");
        sortPairsKernel = computeShader.FindKernel("SortPairs");
        setHashIdKernel = computeShader.FindKernel("SetHashId");
        setIndexKernel = computeShader.FindKernel("SetIndex");
        clearedIndexKernel = computeShader.FindKernel("ClearIndex");

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
        float[] nearDensities = new float[population];
        Vector4[] colors = new Vector4[population];
        //int [] hashIds = new int[population * 2];
        int [] hashIndex = new int[population];

        for (int i = 0; i < population; i++)
        {
            hashIndex[i] = -1;
        }

        //SpawnGrid(positions, velocities, colors, forces);
        SpawnRandom(positions, velocities, colors, forces);

        helper.CreateAndSetBuffer("_Colors", population, sizeof(float) * 4);
        helper.SetBufferData("_Colors", colors);

        helper.CreateAndSetBuffer("_Forces", population, sizeof(float) * 4);
        helper.SetBufferData("_Forces", forces);
     
        helper.CreateAndSetBuffer("_Velocities", population, sizeof(float) * 4);
        helper.SetBufferData("_Velocities", velocities);

        helper.CreateAndSetBuffer("_Positions", population, sizeof(float) * 4);
        helper.SetBufferData("_Positions", positions);

        helper.CreateAndSetBuffer("_Densities", population, sizeof(float));
        helper.SetBufferData("_Densities", densities);

        helper.CreateAndSetBuffer("_NearDensities", population, sizeof(float));
        helper.SetBufferData("_NearDensities", nearDensities);



        helper.CreateAndSetBuffer("_PredictedPositions", population, sizeof(float)*4);

        helper.CreateAndSetBuffer("_HashIds", population, sizeof(int) * 2);

        helper.CreateAndSetBuffer("_HashIndex", population, sizeof(int));
        helper.SetBufferData("_HashIndex", hashIndex);

        ComputeBuffer pos = helper.GetBuffer("_Positions");
        ComputeBuffer den = helper.GetBuffer("_Densities");
        ComputeBuffer col = helper.GetBuffer("_Colors");

        material.SetBuffer("_Positions", pos);
        material.SetFloat("_ParticleSize", particleSize);
        material.SetBuffer("_Colors", col);
        material.SetFloat("_Size", particleSize);
        material.SetBuffer("_Densities", pos);



        helper.SetVector("_BoxMin", BoxMin);
        helper.SetVector("_BoxMax", BoxMax);
        helper.SetInt("_NumParticles", population);
        helper.SetFloat("_H", H);
        helper.SetFloat("_K", k);
        helper.SetFloat("_kNear", kNear);
        helper.SetFloat("_TargetDensity", targetDensity);
        helper.SetFloat("_G", G);
        helper.SetFloat("_Viscosity", viscosity);
        helper.SetFloat("_Damping", damping);
        helper.SetFloat("_DeltaTime", deltaTime);



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
            float x = (col + 0.5f) * spacingX;
            float y = (row + 0.5f) * spacingY;
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
            float x = UnityEngine.Random.Range(0, range);
            float y = UnityEngine.Random.Range(0, range);
            positions[i] = new Vector4(x, y, 0f, 0f);
            velocities[i] = Vector4.zero;
            colors[i] = Color.Lerp(Color.red, Color.blue, UnityEngine.Random.value);
            forces[i] = Vector4.zero;
        }
    }

    public void Sort(int numElements)
    {
        int numPairs = NextPowerOfTwo(numElements);
        int numStages = (int)Log(numPairs * 2, 2);

        for(int stageIndex = 0; stageIndex < numStages; stageIndex++)
        {
            for (int stepIndex = 0; stepIndex < stageIndex + 1; stepIndex++)
            {
                int groupWidth = 1 << (stageIndex - stepIndex);
                int groupHeight = 2 * groupWidth - 1;
                helper.SetInt("groupWidth", groupWidth);
                helper.SetInt("groupHeight", groupHeight);
                helper.SetInt("stepIndex", stepIndex);

                helper.Dispatch(sortPairsKernel, numPairs,1,1);
            }
        }
    }

    private void Update()
    {
        // update shader/material parameters from inspector
        UpdateShaderParams();

        for (int i = 0; i < subSteps; i++)
        {
            Step();
        }
        

        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer);
    }

    private void Step()
    {
        helper.Dispatch(updateKernel, population / 64 + 1, 1, 1);
        helper.Dispatch(setHashIdKernel, population / 64 + 1, 1, 1);

        Sort(population);

        helper.Dispatch(clearedIndexKernel, population / 64 + 1, 1, 1);

        helper.Dispatch(setIndexKernel, population / 64 + 1, 1, 1);

        helper.Dispatch(densityKernel, population / 64 + 1, 1, 1);

        helper.Dispatch(forceKernel, population / 64 + 1, 1, 1);
        helper.Dispatch(0, population / 64 + 1, 1, 1);
    }

    // Set compute shader and material parameters so inspector changes take effect at runtime
    private void UpdateShaderParams()
    {
        helper.SetVector("_BoxMin", BoxMin);
        helper.SetVector("_BoxMax", BoxMax);
        helper.SetInt("_NumParticles", population);
        helper.SetFloat("_H", H);
        helper.SetFloat("_K", k);
        helper.SetFloat("_kNear", kNear);
        helper.SetFloat("_TargetDensity", targetDensity);
        helper.SetFloat("_G", G);
        helper.SetFloat("_Viscosity", viscosity);
        helper.SetFloat("_Damping", damping);

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

        helper.ReleaseBuffers();
        argsBuffer.Release();

    }




}
