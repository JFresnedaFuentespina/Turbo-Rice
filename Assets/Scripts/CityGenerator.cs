using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityGenerator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int width = 100;
    public int height = 100;
    private float seedX;
    private float seedZ;
    public bool randomSeed = true;
    public int seed;

    float verticalOffsetRoad = 0.02f;
    float verticalOffsetCornerRoad = 0.05f;
    public float noiseScale = 15f;

    public GameObject roadPrefab;
    public GameObject cornerRoadPrefab;
    public List<GameObject> housePrefabs;
    public List<GameObject> buildingPrefabs;
    public List<GameObject> skyscraperPrefabs;

    public Transform streetsTransformParent;
    public Transform housesTransformParent;
    public Transform buildingsTransformParent;
    public Transform skyscrapersTransformParent;
    bool[,] roadMap;
    public Road[,] roadGrid;

    public float cellSize = 6f;
    Dictionary<int, string> verticalStreetNames =
    new Dictionary<int, string>();

    Dictionary<int, string> horizontalStreetNames =
        new Dictionary<int, string>();

    Dictionary<string, int> streetNumbers =
        new Dictionary<string, int>();
    HashSet<string> usedStreetNames =
        new HashSet<string>();
    public Dictionary<Road, List<Address>> roadAddresses =
        new Dictionary<Road, List<Address>>();

    List<Road> roads = new List<Road>();
    public bool cityGenerated = false;
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
        cityGenerated = false;
        roadMap = new bool[width, height];
        roadGrid = new Road[width, height];

        if (randomSeed)
        {
            seed = System.Environment.TickCount;
        }

        Random.InitState(seed);

        seedX = Random.Range(0f, 10000f);
        seedZ = Random.Range(0f, 10000f);

        Debug.Log("Seed: " + seed);

        // 1ª pasada - Determinar mapa de calles
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                bool vertical;
                roadMap[x, z] = IsRoad(x, z, out vertical);
            }

            yield return null;
        }

        // 2ª pasada - Instanciar todas las calles y rellenar roadGrid
        int counter = 0;
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (roadMap[x, z])
                {
                    Vector3 position = new Vector3(x * cellSize, 0, z * cellSize);
                    bool vertical;
                    IsRoad(x, z, out vertical);
                    GenerateStreet(x, z, vertical, position);
                }

                counter++;
                if (counter % 200 == 0)
                    yield return null;
            }
        }

        // 3ª pasada - Instanciar todos los edificios y asignar direcciones (ahora con roadGrid completo)
        counter = 0;
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (!roadMap[x, z] && HasAdjacentRoad(x, z))
                {
                    Vector3 position = new Vector3(x * cellSize, 0, z * cellSize);
                    float xCoord = (float)x / width * noiseScale + seedX;
                    float zCoord = (float)z / height * noiseScale + seedZ;

                    float noiseValue = Mathf.PerlinNoise(xCoord, zCoord);

                    SpawnBuildings(noiseValue, position, x, z);
                }

                counter++;
                if (counter % 200 == 0)
                    yield return null;
            }
        }
        cityGenerated = true;
    }

    bool IsIntersection(int x, int z)
    {
        bool left = x > 0 && roadMap[x - 1, z];
        bool right = x < width - 1 && roadMap[x + 1, z];
        bool up = z < height - 1 && roadMap[x, z + 1];
        bool down = z > 0 && roadMap[x, z - 1];

        int connections = 0;

        if (left) connections++;
        if (right) connections++;
        if (up) connections++;
        if (down) connections++;

        return connections >= 3;
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
        float verticalOffset = IsIntersection(x, z) ? verticalOffsetCornerRoad : verticalOffsetRoad;
        GameObject prefab = IsIntersection(x, z) ? cornerRoadPrefab : roadPrefab;
        GameObject r = Instantiate(prefab, position + Vector3.up * verticalOffset, Quaternion.identity, streetsTransformParent);
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

        if (!IsIntersection(x, z))
        {
            road.transform.rotation =
                Quaternion.Euler(0f, vertical ? 0f : 90f, 0f);
        }
        road.gridX = x;
        road.gridZ = z;
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
        float roadNoiseX = Mathf.PerlinNoise(x * 0.03f + seedX, 100f);
        float roadNoiseZ = Mathf.PerlinNoise(100f, z * 0.03f + seedZ);

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
            GameObject randomHouse = SelectRandomHouse();
            obj = Instantiate(randomHouse, position, rot, housesTransformParent);
        }
        else if (value < 0.7f)
        {
            GameObject randomBuilding = SelectRandomBuilding();
            obj = Instantiate(randomBuilding, position, rot, buildingsTransformParent);
        }
        else
        {
            GameObject randomSkyscraper = SelectRandomSkyscraper();
            obj = Instantiate(randomSkyscraper, position, rot, skyscrapersTransformParent);
        }

        Renderer rend = obj.GetComponent<Renderer>();

        // if (rend != null)
        // {
        //     Vector3 p = obj.transform.position;
        //     p.y += rend.bounds.size.y * 0.5f;
        //     obj.transform.position = p;
        // }

        AssignAddress(obj, position, x, z);
    }

    public GameObject SelectRandomHouse()
    {
        GameObject obj = housePrefabs[Random.Range(0, housePrefabs.Count)];
        return obj;
    }

    public GameObject SelectRandomBuilding()
    {
        GameObject obj = buildingPrefabs[Random.Range(0, buildingPrefabs.Count)];
        return obj;
    }
    public GameObject SelectRandomSkyscraper()
    {
        GameObject obj = skyscraperPrefabs[Random.Range(0, skyscraperPrefabs.Count)];
        return obj;
    }

    void AssignAddress(GameObject obj, Vector3 position, int x, int z)
    {
        Address address = obj.GetComponent<Address>();
        Address[] allAddresses = obj.GetComponentsInChildren<Address>(true);

        if (address == null)
        {
            if (allAddresses.Length > 0)
            {
                address = obj.AddComponent<Address>();
            }
        }

        foreach (var addr in allAddresses)
        {
            if (addr != address && addr != null)
            {
                DestroyImmediate(addr);
            }
        }

        if (address == null)
            return;

        Road nearestRoad = GetAdjacentRoad(x, z);

        if (nearestRoad == null)
            return;

        OrientBuilding(obj, nearestRoad, x, z);

        address.road = nearestRoad;
        string streetName = nearestRoad.streetName;

        address.streetName = streetName;

        address.number =
            GenerateHouseNumber(
                streetName,
                position,
                nearestRoad.transform.position
            );

        obj.name = streetName + " " + address.number;

        if (!roadAddresses.ContainsKey(nearestRoad))
        {
            roadAddresses[nearestRoad] = new List<Address>();
        }

        roadAddresses[nearestRoad].Add(address);
    }
    void OrientBuilding(GameObject obj, Road road, int x, int z)
    {
        if (road == null)
            return;

        int rx = road.gridX;
        int rz = road.gridZ;

        if (rx < x)
            obj.transform.rotation = Quaternion.Euler(0, -90, 0);
        else if (rx > x)
            obj.transform.rotation = Quaternion.Euler(0, 90, 0);
        else if (rz < z)
            obj.transform.rotation = Quaternion.Euler(0, 180, 0);
        else if (rz > z)
            obj.transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    Road GetAdjacentRoad(int x, int z)
    {
        if (x > 0 && roadGrid[x - 1, z] != null)
            return roadGrid[x - 1, z];

        if (x < width - 1 && roadGrid[x + 1, z] != null)
            return roadGrid[x + 1, z];

        if (z > 0 && roadGrid[x, z - 1] != null)
            return roadGrid[x, z - 1];

        if (z < height - 1 && roadGrid[x, z + 1] != null)
            return roadGrid[x, z + 1];

        return null;
    }

    public Road FindNearestRoad(Vector3 position)
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

    public void ResetCity()
    {
        // Eliminar calles
        foreach (Road road in roads)
        {
            Destroy(road.gameObject);
        }

        roads.Clear();

        // Eliminar edificios
        foreach (Transform child in housesTransformParent)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in buildingsTransformParent)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in skyscrapersTransformParent)
        {
            Destroy(child.gameObject);
        }

        // Limpiar datos
        verticalStreetNames.Clear();
        horizontalStreetNames.Clear();
        streetNumbers.Clear();
        usedStreetNames.Clear();
        roadAddresses.Clear();


        // Regenerar ciudad
        StartCoroutine(GenerateCity());
    }

    public Address GetRandomAddress()
    {
        List<Address> addresses = new();

        addresses.AddRange(
            housesTransformParent.GetComponentsInChildren<Address>());

        addresses.AddRange(
            buildingsTransformParent.GetComponentsInChildren<Address>());

        addresses.AddRange(
            skyscrapersTransformParent.GetComponentsInChildren<Address>());

        if (addresses.Count == 0)
            return null;

        return addresses[Random.Range(0, addresses.Count)];
    }
}
