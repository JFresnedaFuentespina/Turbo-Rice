using System.Collections.Generic;
using UnityEngine;

public class RouteGenerator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private CityGenerator cityGenerator;
    void Start()
    {
        cityGenerator = GetComponent<CityGenerator>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public List<Road> GetRoute(Address startAddress, Address endAddress)
    {
        Road startRoad = cityGenerator.FindNearestRoad(startAddress.transform.position);
        Road endRoad = cityGenerator.FindNearestRoad(endAddress.transform.position);

        if (startRoad == null || endRoad == null)
        {
            Debug.LogError("Could not find a road for one of the addresses.");
            return null;
        }

        return FindRoute(startRoad, endRoad);
    }

    List<Road> FindRoute(Road start, Road goal)
    {
        List<Road> openSet = new();
        HashSet<Road> closedSet = new();
        Dictionary<Road, Road> cameFrom = new();
        Dictionary<Road, float> gScore = new();
        Dictionary<Road, float> fScore = new();

        openSet.Add(start);
        gScore[start] = 0;
        fScore[start] = Heuristic(start, goal);

        while (openSet.Count > 0)
        {
            Road current = GetLowestFScore(openSet, fScore);
            if (current == goal)
            {
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (Road neighbour in GetNeighbours(current))
            {
                if (closedSet.Contains(neighbour))
                {
                    continue;
                }

                float tentativeG = gScore[current] + 1f;

                if (!openSet.Contains(neighbour))
                {
                    openSet.Add(neighbour);
                }
                else if (tentativeG >= gScore.GetValueOrDefault(neighbour, Mathf.Infinity))
                {
                    continue;
                }

                cameFrom[neighbour] = current;
                gScore[neighbour] = tentativeG;
                fScore[neighbour] = gScore[neighbour] + Heuristic(neighbour, goal);
            }
        }
        return null;
    }

    Road GetLowestFScore(List<Road> openSet, Dictionary<Road, float> fScore)
    {
        Road best = openSet[0];
        foreach (Road road in openSet)
        {
            if (fScore.GetValueOrDefault(road, Mathf.Infinity) < fScore.GetValueOrDefault(best, Mathf.Infinity))
            {
                best = road;
            }
        }
        return best;
    }
    List<Road> ReconstructPath(Dictionary<Road, Road> cameFrom, Road current)
    {
        List<Road> path = new();

        path.Add(current);

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }

        path.Reverse();

        return path;
    }
    List<Road> GetNeighbours(Road road)
    {
        List<Road> neighbours = new();

        AddNeighbour(road.gridX + 1, road.gridZ, neighbours);
        AddNeighbour(road.gridX - 1, road.gridZ, neighbours);
        AddNeighbour(road.gridX, road.gridZ + 1, neighbours);
        AddNeighbour(road.gridX, road.gridZ - 1, neighbours);

        return neighbours;
    }
    void AddNeighbour(int x, int z, List<Road> neighbours)
    {
        if (x < 0 || x >= cityGenerator.width)
            return;

        if (z < 0 || z >= cityGenerator.height)
            return;

        Road road = cityGenerator.roadGrid[x, z];

        if (road != null)
            neighbours.Add(road);
    }

    float Heuristic(Road a, Road b)
    {
        return Mathf.Abs(a.gridX - b.gridX) + Mathf.Abs(a.gridZ - b.gridZ);
    }
}
