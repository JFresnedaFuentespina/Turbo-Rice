using System;
using UnityEngine;

public class ChunkLoader : MonoBehaviour
{
    public CityGenerator cityGenerator;
    public VehicleEnter vehicleEnter;
    public Transform player;
    public int renderDistance = 2;

    private Vector2Int currentChunk = new(-1, -1);

    void Update()
    {
        Transform player = vehicleEnter.CurrentTransform;

        Vector2Int newChunk = new(
            Mathf.FloorToInt(player.position.x / (cityGenerator.cellSize * cityGenerator.chunkSize)),
            Mathf.FloorToInt(player.position.z / (cityGenerator.cellSize * cityGenerator.chunkSize))
        );

        if (newChunk == currentChunk)
            return;

        currentChunk = newChunk;

        UpdateChunks();
    }
    void UpdateChunks()
    {
        foreach (var pair in cityGenerator.chunks)
        {
            int dx = Mathf.Abs(pair.Key.x - currentChunk.x);
            int dz = Mathf.Abs(pair.Key.y - currentChunk.y);

            bool active = dx <= renderDistance && dz <= renderDistance;

            if (pair.Value.gameObject.activeSelf != active)
            {
                pair.Value.gameObject.SetActive(active);
            }
        }
    }

}
