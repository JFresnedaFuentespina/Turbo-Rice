using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkLoader : MonoBehaviour
{
    public CityGenerator cityGenerator;
    public VehicleEnter vehicleEnter;
    public Transform player;
    public int renderDistance = 2;
    private Vector2Int currentChunk = new(-1, -1);
    private HashSet<Vector2Int> activeChunks = new();

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
        HashSet<Vector2Int> desired = GetDesiredChunks();

        // Desactivar los que ya no deberían estar
        foreach (Vector2Int chunk in activeChunks)
        {
            if (!desired.Contains(chunk))
            {
                cityGenerator.chunks[chunk].gameObject.SetActive(false);
            }
        }

        // Activar los nuevos
        foreach (Vector2Int chunk in desired)
        {
            if (!activeChunks.Contains(chunk))
            {
                cityGenerator.chunks[chunk].gameObject.SetActive(true);
            }
        }

        activeChunks = desired;
    }

    HashSet<Vector2Int> GetDesiredChunks()
    {
        HashSet<Vector2Int> desired = new();

        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int z = -renderDistance; z <= renderDistance; z++)
            {
                Vector2Int chunk = new Vector2Int(
                    currentChunk.x + x,
                    currentChunk.y + z
                );

                if (cityGenerator.chunks.ContainsKey(chunk))
                {
                    desired.Add(chunk);
                }
            }
        }

        return desired;
    }

}
