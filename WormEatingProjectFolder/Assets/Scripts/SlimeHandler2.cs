using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeHandler2 : MonoBehaviour
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

    public float trailFade;

    public float noseStrength;

    ComputeBuffer particleBuffer;

    ComputeBuffer particleBoardBuffer;

    ComputeBuffer particleTrailBuffer;

    ComputeBuffer trailBoardBuffer;

    int[] particleBoard;

    int[] particleTrails;

    int[] trailBoard;

    Particle[] particleArray;

    struct Particle
    {
        public Vector2 position;
        public float heading;
    }
    private void Update()
    {
        computeParticle.Dispatch(0, Mathf.Max(1, Mathf.CeilToInt(n / 64)), 1, 1);
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
        computeParticle.SetBuffer(0, "trailArray", trailBoardBuffer);
        //Compute particles set variables
        computeParticle.SetInt("n", n);
        computeParticle.SetInt("size", size);
        computeParticle.SetFloat("resolution", resolution);
        computeParticle.SetFloat("noseStrength", noseStrength);
        computeParticle.SetFloat("speed", speed);
        computeParticle.SetInt("timeBeforeTurn", timeBeforeTurn);
        //pass buffer to shader
        computeTexture.SetFloat("resolution", resolution);
        computeTexture.SetFloat("n", n);
        computeTexture.SetBuffer(0, "particleArray", particleBoardBuffer);
        computeTexture.SetInt("lengthOfFoodTrail", lengthOfFoodTrail);
        computeTexture.SetFloat("fade", trailFade);
        computeTexture.SetBuffer(0, "trailArray", trailBoardBuffer);


    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //Set data in compute shader and dispatch shader each draw      
        computeTexture.SetTexture(0, "Result", renderTexture);
        computeTexture.Dispatch(0, renderTexture.width/16, renderTexture.height/16, 1);
        computeTexture.SetBuffer(0, "particlePositions", particleBuffer);
        //Blit the texture to the screen
        Graphics.Blit(renderTexture, destination);

    }
    private void CreateBuffers()
    {
        //Create the particle buffer
        particleArray = CreateParticles();
        int totalSize = sizeof(float) * 2 + sizeof(float);
        particleBuffer = new ComputeBuffer
            (particleArray.Length, totalSize);
        particleBuffer.SetData(particleArray);

        //Create the array to hold positions buffer
        particleBoard = new int[resolution * resolution];
        PlaceFood(resolution, numberOfFood, sizeOfFood, ref particleBoard);
        particleBoardBuffer = new ComputeBuffer
            (particleBoard.Length, 4);
        particleBoardBuffer.SetData(particleBoard);

        //Create the array to hold trail place in board
        trailBoard = new int[resolution * resolution];
        trailBoardBuffer = new ComputeBuffer
            (trailBoard.Length, 4);
        trailBoardBuffer.SetData(trailBoard);
    }
    //Creates n particles placed at center of the board;
    private Particle[] CreateParticles()
    {
        Particle[] particlePositions = new Particle[n];
        for (int i = 0; i < n; i++)
        {
            particlePositions[i].position = new Vector2(Random.Range(50,resolution), Random.Range(50, resolution));
            particlePositions[i].heading = Random.Range(-Mathf.PI * 2f, Mathf.PI * 2f);
        }
        return particlePositions;
    }
    //Place food in square formation on board
    private void PlaceFood(int resolution, int n, int size, ref int[] pBoard)
    {
        for (int i = 0; i < n; i++)
        {
            int randSpot = Random.Range(0, resolution * resolution);
            if (randSpot + size * resolution + size > resolution * resolution)
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
                for (int p = 0; p < 4; p++)
                {
                    for (int q = 0; q < distance; q++)
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
