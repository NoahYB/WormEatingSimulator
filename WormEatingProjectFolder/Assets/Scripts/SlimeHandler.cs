using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeHandler : MonoBehaviour
{
    public ComputeShader computeTexture;

    public ComputeShader computeParticle;

    public RenderTexture renderTexture;

    public int resolution;

    public int n;

    public float speed;

    public int size;

    public int sizeOfFood;

    public int numberOfFood;

    public int lengthOfFoodTrail;

    public int timeBeforeTurn;

    public float noseStrength;

    ComputeBuffer particleBuffer;

    ComputeBuffer particleBoardBuffer;

    ComputeBuffer particleTrailBuffer;

    int[] particleBoard;

    int[] particleTrails;

    Particle[] particleArray;

    struct Particle
    {
        public Vector2 position;
        public float speed;
        public float heading;
        public float t;
        public int changeheading;
        public uint foundfood;
        public uint currentindex;
        public uint state;

    }
    private void Update()
    {
        computeParticle.Dispatch(0, Mathf.Max(1,Mathf.CeilToInt(n / 64)), 1, 1);
    }
    private void Start()
    {

        //Set texture if texture = null
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(resolution, resolution, 24);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
        }
        //Create buffers for texture array and particle array
        CreateBuffers();
        computeParticle.SetBuffer(0, "particleArray", particleBoardBuffer);
        computeParticle.SetBuffer(0, "particles", particleBuffer);
        computeParticle.SetBuffer(0, "trailPositions", particleTrailBuffer);
        //Compute particles set variables
        computeParticle.SetInt("n", n);
        computeParticle.SetInt("size", size);
        computeParticle.SetFloat("resolution", resolution);
        computeParticle.SetFloat("noseStrength", noseStrength);
        computeParticle.SetInt("trailLength", lengthOfFoodTrail);
        computeParticle.SetInt("timeBeforeTurn", timeBeforeTurn);


        //pass buffer to shader
        computeTexture.SetFloat("resolution", resolution);
        computeTexture.SetFloat("n", n);
        computeTexture.SetBuffer(0, "particleArray", particleBoardBuffer);
        computeTexture.SetInt("lengthOfFoodTrail", lengthOfFoodTrail);


    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //Set data in compute shader and dispatch shader each draw      
        computeTexture.SetTexture(0, "Result", renderTexture);
        computeTexture.Dispatch(0, renderTexture.width, renderTexture.height, 1);
        computeTexture.SetBuffer(0, "particlePositions", particleBuffer);
        //Blit the texture to the screen
        Graphics.Blit(renderTexture, destination);

    }
    private void CreateBuffers()
    {
        //Create the particle buffer
        particleArray = CreateParticles();
        int totalSize = sizeof(float) * 2 + sizeof(float) * 3 + sizeof(int) + sizeof(uint) *3;
        particleBuffer = new ComputeBuffer
            (particleArray.Length, totalSize);
        particleBuffer.SetData(particleArray);

        //Create the array to hold positions buffer
        particleBoard = new int[resolution * resolution];
        PlaceFood(resolution, numberOfFood, sizeOfFood, ref particleBoard);
        particleBoardBuffer = new ComputeBuffer
            (particleBoard.Length, 4);
        particleBoardBuffer.SetData(particleBoard);

        //Create the array to hold positions buffer
        particleTrails = new int[resolution * resolution];
        particleTrailBuffer = new ComputeBuffer
            (particleBoard.Length, 4);
        particleTrailBuffer.SetData(particleTrails);
    }
    //Creates n particles placed at center of the board;
    private Particle[] CreateParticles()
    {
        Particle[] particlePositions = new Particle[n];
        for (int i = 0; i < n; i++)
        {
            particlePositions[i].position = new Vector2(resolution / 2, resolution / 2);
            particlePositions[i].speed = speed;
            particlePositions[i].t = 1;
            particlePositions[i].heading = Random.Range(0f, Mathf.PI * 2f);
            particlePositions[i].changeheading = 0;
            particlePositions[i].currentindex = 0;
            particlePositions[i].state = 0;
        }
        return particlePositions;
    }
    //Place food in square formation on board
    private void PlaceFood(int resolution, int n, int size, ref int[] pBoard)
    {
        for(int i = 0; i < n; i++) {
            int randSpot = Random.Range(0, resolution * resolution);
            if(randSpot + size * resolution + size > resolution * resolution)
            {
                randSpot -= size * resolution + size;
            }
            if (randSpot - size * resolution + size < 0)
            {
                randSpot += size * resolution + size;
            }
            pBoard[randSpot] = 2;
            for (int j = 1; j < size; j++)
            {
                int iterations = 8 * j;
                int currentIndex = randSpot + resolution * j - j;
                int distance = j * 2;
                int dir = 1;
                for(int p = 0; p < 4; p++)
                {
                    for(int q = 0; q < distance; q++)
                    {
                        if (dir == 1)
                            pBoard[currentIndex += 1] = 2;
                        else if (dir == 0)
                            pBoard[currentIndex -= resolution] = 2;
                        else if (dir == -1)
                            pBoard[currentIndex -= 1] = 2;
                        else if (dir == -2)
                           pBoard[currentIndex += resolution] = 2;
                    }
                    dir -= 1; 
                    
                }
            }
        }

    }
}
