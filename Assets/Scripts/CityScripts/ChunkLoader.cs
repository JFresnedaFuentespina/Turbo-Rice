using System;
using UnityEngine;

public class ChunkLoader : MonoBehaviour
{
    public CityGenerator cityGenerator;
    public VehicleEnter vehicleEnter;
    private Transform player;
    public int renderDistance = 2;



    void Update()
    {
        FindPlayer();

        int px = Mathf.FloorToInt(player.position.x / cityGenerator.cellSize);
        int pz = Mathf.FloorToInt(player.position.z / cityGenerator.cellSize);

        int chunkX = px / cityGenerator.chunkSize;
        int chunkZ = pz / cityGenerator.chunkSize;

        foreach (var pair in cityGenerator.chunks)
        {
            int dx = Math.Abs(pair.Key.x - chunkX);
            int dz = Math.Abs(pair.Key.y - chunkZ);

            bool active = dx <= renderDistance && dz <= renderDistance;

            if (pair.Value.gameObject.activeSelf != active)
            {
                pair.Value.gameObject.SetActive(active);
            }
        }
    }

    void FindPlayer()
    {
        if (vehicleEnter.inCar)
        {
            player = GameObject.FindWithTag("Car").transform;
        }
        else
        {
            player = GameObject.FindWithTag("Player").transform;
        }
    }
}
