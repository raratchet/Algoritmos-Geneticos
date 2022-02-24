using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IAttacker
{
    NavMeshAgent agent;

    [SerializeField]
    Dictionary<State, float> health_probs;
    [SerializeField]
    Dictionary<State, float> ammo_probs;

    public float timeAlive;
    public float damageDone;

    float startTimer;

    public Chromosome chromo;

    public Dictionary<State, float> Health_probs { get { return health_probs; } set { health_probs = value; } }
    public Dictionary<State, float> Ammo_probs { get { return ammo_probs; } set { ammo_probs = value; } }

    public GameObject objective;

    [SerializeField]
    GameObject bullet;
    [SerializeField]
    float health;
    [SerializeField]
    float stopDistance;
    [SerializeField]
    float range;
    [SerializeField]
    int ammo;

    public bool shootAlLeastOnce = false;
    public float timeInSamePos;
    Vector3 lastPos;

    MapGenerator map;

    public float Health { get { return health; } set { health = value; } }
    public int Ammo { get { return ammo; } set { ammo = value; } }

    // Start is called before the first frame update

    private void Awake()
    {
        Health_probs = new Dictionary<State, float>();
        Ammo_probs = new Dictionary<State, float>();
    }
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        map = FindObjectOfType<MapGenerator>();
        agent.stoppingDistance = stopDistance;
    }

    public void SetProbs(Dictionary<State, float> h, Dictionary<State, float> a)
    {
        Health_probs = h;
        Ammo_probs = a;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ReportDamage(float damage)
    {
        damageDone += damage;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnEnable()
    {
        StartCoroutine(Cicle());
        StartCoroutine(TimeInSamePos());
        startTimer = Time.time;
        timeInSamePos = 0;
        shootAlLeastOnce = false;
        damageDone = 0;
        health = 100;
        ammo = 30;
    }

    IEnumerator Cicle()
    {
        while(true)
        {
            yield return new WaitForSeconds(1.5f);
            UpdateState();
        }
    }

    IEnumerator TimeInSamePos()
    {
        while(true)
        {

            lastPos = transform.position;

            yield return new WaitForSeconds(0.5f);

            if (transform.position == lastPos)
                timeInSamePos += 0.5f;
        }
    }

    public void UpdateState()
    {

        Actions action  = Fuzzifier.Instance.GetAction(Health,Ammo,chromo);

        if (action == Actions.ATTACK)
            Attack();
        else if (action == Actions.RELOAD)
            Reload();
        else if (action == Actions.HEAL)
            Heal();
    }

    void Attack()
    {
        Debug.Log("Enemy  Attack");
        agent.stoppingDistance = 0;
        float distanceToPlayer = Vector3.Distance(transform.position, map.player.transform.position);
        if (distanceToPlayer <= range)
            Shoot();
        agent.SetDestination(map.player.transform.position);
    }

    void Reload()
    {
        Debug.Log("Enemy  Reload");
        agent.stoppingDistance = stopDistance;
        agent.SetDestination(map.FindNearestStation(transform.position, StationType.Ammo));
    }

    void Heal()
    {
        Debug.Log("Enemy  Heal");
        agent.stoppingDistance = stopDistance;
        agent.SetDestination(map.FindNearestStation(transform.position, StationType.Health));
    }

    void Die()
    {
        timeAlive = Time.time - startTimer;
        SendMessageUpwards("EnemyDeath", this);
    }

    public void Shoot() 
    {
        Vector3 direction = map.player.transform.position - transform.position;

        if (ammo <= 0) return;

        shootAlLeastOnce = true;

        var bullet = Instantiate(this.bullet,transform.position,Quaternion.identity).GetComponent<Bullet>();

        bullet.damage = 10f;
        bullet.owner = "Enemy";
        bullet.enemy = this;
        bullet.direction = direction.normalized;
        ammo--;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.tag.Equals("Bullet")) return;

        Bullet bullet = other.gameObject.GetComponent<Bullet>();

        if(bullet.owner == "Player")
        {
            health -= bullet.damage;
            if (health <= 0)
                Die();

            Destroy(bullet.gameObject);
        }

    }
}


public interface IAttacker
{
    public float Health { get; set; }
    public int Ammo { get; set; }

    public void Shoot();
}