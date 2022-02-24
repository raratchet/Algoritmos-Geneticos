using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField]
    GameObject mapTilePrefab;
    [SerializeField]
    GameObject wallPrefab;
    [SerializeField]
    int gridSize;
    [SerializeField]
    int HealStations;
    [SerializeField]
    int AmmoStations;
    [SerializeField]
    GameObject HstationPrefab;
    [SerializeField]
    GameObject AstationPrefab;
    [SerializeField]
    GameObject paredParent;
    [SerializeField]
    GameObject pisoParent;
    [SerializeField]
    public GameObject player;
    [SerializeField]
    int wallThreshold;
    [SerializeField]
    int floorThreshold;
    [SerializeField]
    int automataRepetition;

    public List<Stations> avalivableStations;
    public List<Vector2Int> validSpawnPoints;

    Dictionary<Vector2Int,GameObject> mapGrid;
    bool[,] mapGridType;// True=piso False =  pared

    // Start is called before the first frame update
    void Start()
    {
        mapGrid = new Dictionary<Vector2Int, GameObject>();
        mapGridType = new bool[gridSize, gridSize];
        avalivableStations = new List<Stations>();
        GenerateGrid();
        AutomataGrid();
        FloodFill();
        Perimetro();
        GenerateStations();
        SpawnPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SpawnPlayer()
    {
        float y = player.transform.position.y;
        Vector3 spawnPos = GetSpawnPosition();
        spawnPos.y += y;
        player = Instantiate(player, spawnPos, Quaternion.identity);
    }

    public Vector3 GetSpawnPosition()
    {
        int ran = Random.Range(0, validSpawnPoints.Count);
        Vector3 pos = mapGrid[validSpawnPoints[ran]].transform.position;
        return pos;
    }

    public Vector3 FindNearestStation(Vector3 fromPos, StationType type)
    {
        float distance = 99999f;
        Vector3 destination = Vector3.zero;

        foreach(var station in avalivableStations)
        {
            if (!station.type.Equals(type)) continue;

            float d = Vector3.Distance(fromPos , station.gameObject.transform.position);

            if(d < distance)
            {
                distance = d;
                destination = station.gameObject.transform.position;
            }
        }

        return destination;
    }

    void GenerateStations()
    {
        for(int i = 0; i < HealStations; i++)
        {
            int ran = Random.Range(0, validSpawnPoints.Count);
            var point = validSpawnPoints[ran];
            Vector3 spawnPos = mapGrid[point].transform.position;
            var station = Instantiate(HstationPrefab, spawnPos, Quaternion.identity, transform);
            validSpawnPoints.RemoveAt(ran);
            avalivableStations.Add(station.GetComponent<Stations>());
        }

        for (int i = 0; i < AmmoStations; i++)
        {
            int ran = Random.Range(0, validSpawnPoints.Count);
            var point = validSpawnPoints[ran];
            Vector3 spawnPos = mapGrid[point].transform.position;
            var station = Instantiate(AstationPrefab, spawnPos, Quaternion.identity, transform);
            validSpawnPoints.RemoveAt(ran);
            avalivableStations.Add(station.GetComponent<Stations>());
        }

    }

    void Perimetro()
    {

        for(int i  = 0; i < gridSize; i++)
        {
            float x = transform.position.x + i * mapTilePrefab.transform.localScale.x;
            float z = transform.position.z + i * mapTilePrefab.transform.localScale.z;

            Vector3 ins_pos_x = new Vector3(x, transform.position.y, transform.position.z + gridSize * mapTilePrefab.transform.localScale.z);
            Vector3 ins_pos_z = new Vector3(transform.position.x + gridSize * mapTilePrefab.transform.localScale.x, transform.position.y, z);
            Vector3 ins_pos_x_1 = new Vector3(x, transform.position.y, -2.5f);
            Vector3 ins_pos_z_1 = new Vector3(-2.5f, transform.position.y, z);
            Instantiate(wallPrefab, ins_pos_x, Quaternion.identity, paredParent.transform);
            Instantiate(wallPrefab, ins_pos_z, Quaternion.identity, paredParent.transform);
            Instantiate(wallPrefab, ins_pos_x_1, Quaternion.identity, paredParent.transform);
            Instantiate(wallPrefab, ins_pos_z_1, Quaternion.identity, paredParent.transform);
        }
    }

    void FloodFill()
    {
        var floors = GetAllFloors();
        var biggest = GetBiggest(floors);
        floors.Remove(biggest);
        Do_FloodFill(floors);

        foreach (var spot in biggest)
        {
            validSpawnPoints.Add(spot);
        }
    }

    void Do_FloodFill(List<List<Vector2Int>> floors)
    {
        foreach(var flist in floors)
        {
            foreach(var pos in flist)
            {
                mapGridType[pos.x, pos.y] = false;
            }
        }

        ChangeMapGridType();
    }

    List<Vector2Int> GetBiggest(List<List<Vector2Int>> floors)
    {
        List<Vector2Int> big = floors[0];

        foreach(var l in  floors)
        {
            if (big.Count < l.Count)
                big = l;
        }

        return big;
    }

    List<List<Vector2Int>> GetAllFloors()
    {
        List<Vector2Int> visited = new List<Vector2Int>();
        List<List<Vector2Int>> floors = new List<List<Vector2Int>>();

        Queue<Vector2Int> toVisit = new Queue<Vector2Int>();

        Vector2Int tmp = Vector2Int.zero;


        Vector2Int current;

        for (int i = 0; i < gridSize; i++)
            for (int j = 0; j < gridSize; j++)
            {
                tmp.Set(i, j);
                if (visited.Contains(tmp)) continue;
                toVisit.Enqueue(tmp);

                List<Vector2Int> currentFloor = new List<Vector2Int>();

                int gg = 0;

                while (toVisit.Count > 0)
                {
                    current = toVisit.Peek();

                    if (visited.Contains(current))
                    {
                        toVisit.Dequeue();
                        continue;
                    }

                    visited.Add(current);

                    if (mapGridType[current.x, current.y])
                    {
                        currentFloor.Add(current);

                        foreach (var nei in GetNeighbors(current.x, current.y))
                        {
                            if (mapGridType[nei.x, nei.y])
                                toVisit.Enqueue(nei);
                        }

                    }

                    toVisit.Dequeue();
                    if (gg > gridSize*gridSize) break;
                    gg++;
                }
                if(currentFloor.Count > 0)
                 floors.Add(currentFloor);
            }


        return floors;
    }

    void AutomataGrid()
    {
        for(int rep = 0; rep < automataRepetition; rep++)
        {
            //Copia del grid
            bool[,] mapGridTypeC = (bool[,])mapGridType.Clone();

            for (int i = 0; i < gridSize; i++)
                for (int j = 0; j < gridSize; j++)
                {
                    int wallCount = 0;
                    int floorCount = 0;

                    foreach (var nei in GetNeighborsMoore(i, j))
                    {
                        if (mapGridType[nei.x, nei.y] == false)
                            wallCount++;
                        else
                            floorCount++;
                    }

                    if (wallCount >= wallThreshold)
                        mapGridTypeC[i, j] = true;
                    if (floorCount >= floorThreshold)
                        mapGridTypeC[i, j] = false;
                }

            mapGridType = mapGridTypeC;
        }

        //ChangeMapGridType();
    }
    
    List<Vector2Int> GetNeighborsMoore(int x, int z)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        Vector2Int key = Vector2Int.zero;

        for(int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
            {
                key.x = x + i;
                key.y = z + j;

                if (key.x == x && key.y == z)
                    continue;

                var neighbor = mapGrid.ContainsKey(key);

                if (neighbor)
                    neighbors.Add(key);
            }

        return neighbors;
    }

    List<Vector2Int> GetNeighbors(int x, int z)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        Vector2Int top = Vector2Int.zero;
        Vector2Int right = Vector2Int.zero;
        Vector2Int down = Vector2Int.zero;
        Vector2Int left = Vector2Int.zero;

        top.Set(x, z + 1);
        right.Set(x + 1, z);
        down.Set(x, z - 1);
        left.Set(x - 1 , z);

        if (mapGrid.ContainsKey(top))
            neighbors.Add(top);
        if (mapGrid.ContainsKey(right))
            neighbors.Add(right);
        if (mapGrid.ContainsKey(down))
            neighbors.Add(down);
        if (mapGrid.ContainsKey(left))
            neighbors.Add(left);

        return neighbors;
    }

    void GenerateGrid()
    {
        for(int i = 0; i < gridSize; i++)
           for(int j  = 0; j < gridSize; j++)
           {
                float x = transform.position.x + i * mapTilePrefab.transform.localScale.x;
                float z = transform.position.z + j * mapTilePrefab.transform.localScale.z;
                Vector3 ins_pos = new Vector3(x,transform.position.y,z);

                Vector2Int key = Vector2Int.zero;
                key.x = i;
                key.y = j;

                mapGrid[key] = Instantiate(mapTilePrefab,ins_pos,Quaternion.identity,transform);
                NoiseAtGrid(i,j);
           }

        //ChangeMapGridType();
    }
    void NoiseAtGrid(int x, int z)
    {

        float noise = Random.Range(0.0F, 1.0F);

        if (noise <= 0.5f)
            mapGridType[x, z] = false;
        else
            mapGridType[x, z] = true;
    }

    void ChangeMapGridType()
    {
        for (int i = 0; i < gridSize; i++)
            for (int j = 0; j < gridSize; j++)
            {
                Vector2Int key = Vector2Int.zero;
                key.x = i;
                key.y = j;

                Vector3 pos = mapGrid[key].transform.position;
                //Destroy(mapGrid[key]);

                if (mapGridType[i, j])
                {
                    // mapGrid[key] = Instantiate(mapTilePrefab, pos, Quaternion.identity, pisoParent.transform);
                    mapGrid[key].SetActive(false);
                }
                else
                {
                    mapGrid[key] = Instantiate(wallPrefab, pos, Quaternion.identity, paredParent.transform);
                    mapGrid[key].SetActive(true);
                }
            }
    }
}
