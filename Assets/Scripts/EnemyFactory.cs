using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFactory : MonoBehaviour
{
    [SerializeField]
    int CountPerWave;    
    [SerializeField]
    int mutation;
    [SerializeField]
    GameObject prefab;

    [SerializeField]
    MapGenerator map;

    Genetic genetics;


    List<Enemy> enemyPool;

    public List<Enemy> activeEnemies;

    // Start is called before the first frame update
    void Start()
    {
        genetics = new Genetic(CountPerWave,mutation);
        enemyPool = new List<Enemy>();
        activeEnemies = new List<Enemy>();
        map = FindObjectOfType<MapGenerator>();
        PopulatePool();
    }

    void PopulatePool()
    { 
      for(int i = 0; i <  CountPerWave;  i++)
        {
            Enemy e = Instantiate(prefab,transform).GetComponent<Enemy>();
            enemyPool.Add(e);
            e.gameObject.SetActive(false);
        }
    }


    public Enemy GetClosestEnemy(Vector3 pos)
    {
        Enemy closestE = activeEnemies.Count > 0 ? activeEnemies[0] : null;
        if (closestE == null) return null;
        float distance = Vector3.Distance(closestE.transform.position, pos);

        foreach(var e  in activeEnemies)
        {
            if(Vector3.Distance(e.transform.position,pos) < distance)
            {
                closestE = e;
                distance = Vector3.Distance(e.transform.position, pos);
            }
        }

        return closestE;
    }

    void GenerateWave()
    {
        foreach(var e in enemyPool)
        {
            var chromo = genetics.GetChromosome();

            e.chromo = chromo;


            float y = e.transform.position.y;
            Vector3 spawnPos = map.GetSpawnPosition();
            spawnPos.y = y;
            e.transform.position = spawnPos;

            e.gameObject.SetActive(true);
            activeEnemies.Add(e);
        }

        enemyPool.Clear();
    }

    void GenerateEnemy()
    {
        Enemy e = enemyPool[0];
        enemyPool.RemoveAt(0);

        e.gameObject.SetActive(true);

    }

    public void EnemyDeath(object enemy)
    {
        Enemy e = (Enemy)enemy;
        e.gameObject.SetActive(false);
        enemyPool.Add(e);
        activeEnemies.Remove(e);
        var aptitude = genetics.CalculateAptitude(e.damageDone,e.timeAlive,e.timeInSamePos,e.shootAlLeastOnce);
        genetics.AddToChromosomePool(e.chromo,aptitude);

        if (enemyPool.Count == CountPerWave)
        {
            genetics.Mutate();
            GenerateWave();
        }
    }

    void  TestWave()
    {
        foreach(var e in FindObjectsOfType<Enemy>())
        {
            e.gameObject.SetActive(false);
            enemyPool.Add(e);
            activeEnemies.Remove(e);
            bool yes = Random.Range(0,2) == 1 ? true : false;
            var aptitude = genetics.CalculateAptitude(Random.Range(0,100), Random.Range(15,150),  Random.Range(0,12),yes);
            genetics.AddToChromosomePool(e.chromo, aptitude);

        }

        genetics.Mutate();
        GenerateWave();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            GenerateWave();
        if (Input.GetKeyDown(KeyCode.Q))
            TestWave();
    }

}
