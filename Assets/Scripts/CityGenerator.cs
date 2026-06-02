using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class CityGenerator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int width = 100;
    public int height = 100;

    public float noiseScale = 15f;

    public GameObject roadPrefab;
    public GameObject housePrefab;
    public GameObject buildingPrefab;
    public GameObject skyscraperPrefab;

    public Transform streetsTransformParent;
    public Transform housesTransformParent;
    public Transform buildingsTransformParent;
    public Transform skyscrapersTransformParent;
    bool[,] roadMap;
    Road[,] roadGrid;

    public float cellSize = 6f;
    Dictionary<int, string> verticalStreetNames =
    new Dictionary<int, string>();

    Dictionary<int, string> horizontalStreetNames =
        new Dictionary<int, string>();

    Dictionary<string, int> streetNumbers =
        new Dictionary<string, int>();
    HashSet<string> usedStreetNames =
        new HashSet<string>();

    List<Road> roads = new List<Road>();
    string[] streetTypes =
    {
        "Carrer de",
        "Camí de",
        "Passeig de",
        "Avinguda de",
        "Costa de",
        "Plaça de"
    };
    string[] nouns =
    {
        "ses Ensaimades",
        "es Guiris",
        "sa Sangria",
        "es Alemany Torrat",
        "ses Xancles",
        "sa Calor Inhumana",
        "es Lloguer Vacacional",
        "sa Crema Solar",
        "es Beach Club",
        "ses Paelles Congelades",
        "es Turista Vermell",
        "ses Bicicletes",
        "es Mojitos a 15 Euros",
        "sa Platja Massificada",
        "es Airbnb Il·legal",
        "ses Gandules Reservades",
        "sa Ressaca",
        "es DJ Internacional",
        "ses Selfies",
        "sa Palma Saturada"
    };
    void Start()
    {
        StartCoroutine(GenerateCity());
    }

    IEnumerator GenerateCity()
    {
        roadMap = new bool[width, height];
        roadGrid = new Road[width, height];

        // 1ª pasada
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                bool vertical;
                roadMap[x, z] = IsRoad(x, z, out vertical);
            }

            yield return null;
        }

        // 2ª pasada
        int counter = 0;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 position = new Vector3(x * cellSize, 0, z * cellSize);

                if (roadMap[x, z])
                {
                    bool vertical;
                    IsRoad(x, z, out vertical);
                    GenerateStreet(x, z, vertical, position);
                }
                else if (HasAdjacentRoad(x, z))
                {
                    float xCoord = (float)x / width * noiseScale;
                    float zCoord = (float)z / height * noiseScale;

                    float noiseValue = Mathf.PerlinNoise(xCoord, zCoord);

                    SpawnBuildings(noiseValue, position, x, z);
                }

                counter++;

                if (counter % 200 == 0)
                    yield return null;
            }
        }
    }

    bool HasAdjacentRoad(int x, int z)
    {
        // izquierda
        if (x > 0 && roadMap[x - 1, z])
            return true;

        // derecha
        if (x < width - 1 && roadMap[x + 1, z])
            return true;

        // abajo
        if (z > 0 && roadMap[x, z - 1])
            return true;

        // arriba
        if (z < height - 1 && roadMap[x, z + 1])
            return true;

        return false;
    }

    void GenerateStreet(int x, int z, bool vertical, Vector3 position)
    {
        GameObject r = Instantiate(roadPrefab, position + Vector3.up * 0.02f, Quaternion.identity, streetsTransformParent);
        Road road = r.GetComponent<Road>();
        roadGrid[x, z] = road;
        roads.Add(road);
        int key = vertical ? x : z;
        if (vertical)
        {
            if (!verticalStreetNames.ContainsKey(key))
            {
                verticalStreetNames[key] = GenerateStreetName();
            }

            road.streetName = verticalStreetNames[key];
        }
        else
        {
            if (!horizontalStreetNames.ContainsKey(key))
            {
                horizontalStreetNames[key] = GenerateStreetName();
            }

            road.streetName = horizontalStreetNames[key];
        }
    }
    string GenerateStreetName()
    {
        string result;

        do
        {
            string type = streetTypes[Random.Range(0, streetTypes.Length)];
            string place = nouns[Random.Range(0, nouns.Length)];

            result = type + " " + place;
        } while (usedStreetNames.Contains(result));

        usedStreetNames.Add(result);

        return result;
    }
    bool IsRoad(int x, int z, out bool vertical)
    {
        float roadNoiseX = Mathf.PerlinNoise(x * 0.03f, 100f);
        float roadNoiseZ = Mathf.PerlinNoise(100f, z * 0.03f);

        int offsetX = Mathf.FloorToInt(roadNoiseX * 2);
        int offsetZ = Mathf.FloorToInt(roadNoiseZ * 2);

        bool verticalRoad =
            (x + offsetX) % 8 == 0;

        bool horizontalRoad =
            (z + offsetZ) % 8 == 0;

        vertical = verticalRoad;

        return verticalRoad || horizontalRoad;
    }

    void SpawnBuildings(float value, Vector3 position, int x, int z)
    {
        Quaternion rot = Quaternion.Euler(0, Random.Range(0, 4) * 90, 0);

        Vector3 randomOffset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        position += randomOffset;

        GameObject obj;

        if (value < 0.4f)
        {
            obj = Instantiate(housePrefab, position, rot, housesTransformParent);
        }
        else if (value < 0.7f)
        {
            obj = Instantiate(buildingPrefab, position, rot, buildingsTransformParent);
        }
        else
        {
            obj = Instantiate(skyscraperPrefab, position, rot, skyscrapersTransformParent);
        }

        AssignAddress(obj, position, x, z);
    }

    void AssignAddress(GameObject obj, Vector3 position, int x, int z)
    {
        Address address =
            obj.GetComponent<Address>();

        if (address == null)
            return;

        Road nearestRoad =
            GetRoadFromGrid(x, z);

        if (nearestRoad == null)
            return;

        string streetName =
            nearestRoad.streetName;

        address.streetName =
            streetName;

        address.number =
            GenerateHouseNumber(
                streetName,
                position,
                nearestRoad.transform.position
            );

        obj.name =
            streetName + " " + address.number;
    }

    Road FindNearestRoad(Vector3 position)
    {
        Road nearestRoad = null;

        float minDistance = Mathf.Infinity;

        foreach (Road road in roads)
        {
            float dist = Vector3.Distance(position, road.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestRoad = road;
            }
        }

        return nearestRoad;
    }

    Road GetRoadFromGrid(int x, int z)
    {
        Road best = null;
        float bestDist = Mathf.Infinity;

        int radius = 1; // vecinos inmediatos

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dz = -radius; dz <= radius; dz++)
            {
                int nx = x + dx;
                int nz = z + dz;

                if (nx < 0 || nx >= width || nz < 0 || nz >= height)
                    continue;

                Road r = roadGrid[nx, nz];

                if (r == null)
                    continue;

                float dist = dx * dx + dz * dz;

                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = r;
                }
            }
        }

        return best;
    }

    int GenerateHouseNumber(string streetName, Vector3 buildingPos, Vector3 roadPos)
    {
        if (!streetNumbers.ContainsKey(streetName))
        {
            streetNumbers[streetName] = 2;
        }

        int currentNumber =
            streetNumbers[streetName];

        bool evenSide =
            buildingPos.x > roadPos.x;

        int finalNumber;

        if (evenSide)
        {
            finalNumber = currentNumber;
        }
        else
        {
            finalNumber = currentNumber + 1;
        }

        streetNumbers[streetName] += 2;

        return finalNumber;
    }
}
