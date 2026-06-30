using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChunkLoader : MonoBehaviour
{
    public CityGenerator cityGenerator;
    public VehicleEnter vehicleEnter;
    public Transform player;
    public int renderDistance = 2;
    private Vector2Int currentChunk = new(-1, -1);

    public bool initialized = false;
    private readonly HashSet<Vector2Int> desiredChunks = new();
    private readonly HashSet<Vector2Int> activeChunks = new();


    IEnumerator Start()
    {
        yield return new WaitUntil(() => cityGenerator.cityGenerated);

        foreach (Transform chunk in cityGenerator.chunks.Values)
        {
            chunk.gameObject.SetActive(false);
        }

        initialized = true;

        Transform player = vehicleEnter.CurrentTransform;

        currentChunk = new Vector2Int(
            Mathf.FloorToInt(player.position.x / (cityGenerator.cellSize * cityGenerator.chunkSize)),
            Mathf.FloorToInt(player.position.z / (cityGenerator.cellSize * cityGenerator.chunkSize))
        );

        UpdateChunks();
    }
    void Update()
    {
        if (!initialized) return;

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
        GetDesiredChunks();

        foreach (Vector2Int chunk in activeChunks)
        {
            if (!desiredChunks.Contains(chunk))
            {
                cityGenerator.chunks[chunk].gameObject.SetActive(false);
            }
        }

        foreach (Vector2Int chunk in desiredChunks)
        {
            if (!activeChunks.Contains(chunk))
            {
                cityGenerator.chunks[chunk].gameObject.SetActive(true);
            }
        }

        activeChunks.Clear();
        activeChunks.UnionWith(desiredChunks);
    }
    void GetDesiredChunks()
    {
        desiredChunks.Clear();

        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int z = -renderDistance; z <= renderDistance; z++)
            {
                Vector2Int chunk = new Vector2Int(
                    currentChunk.x + x,
                    currentChunk.y + z
                );

                Debug.Log($"Buscando {chunk} -> {cityGenerator.chunks.ContainsKey(chunk)}");

                if (cityGenerator.chunks.ContainsKey(chunk))
                {
                    desiredChunks.Add(chunk);
                }
            }
        }
    }

}
