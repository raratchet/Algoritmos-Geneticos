using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IAttacker
{
    [SerializeField]
    float health;
    [SerializeField]
    float Range;
    [SerializeField]
    float Speed;
    [SerializeField]
    int ammo;
    [SerializeField]
    GameObject bullet;

    EnemyFactory factory;

    public float Health { get { return health; } set { health = value; } }
    public int Ammo { get { return ammo; } set { ammo = value; } }

    // Start is called before the first frame update
    void Start()
    {
        factory = FindObjectOfType<EnemyFactory>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Shoot();

        if (Input.GetKey(KeyCode.W))
            transform.Translate(Vector3.forward * Speed *  Time.deltaTime);
        if (Input.GetKey(KeyCode.A))
            transform.Translate(Vector3.left * Speed * Time.deltaTime);
        if (Input.GetKey(KeyCode.D))
            transform.Translate(Vector3.right * Speed * Time.deltaTime);
        if (Input.GetKey(KeyCode.S))
            transform.Translate(Vector3.back * Speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.tag.Equals("Bullet")) return;

        Bullet bullet = other.gameObject.GetComponent<Bullet>();

        if (bullet.owner == "Enemy")
        {
            health -= bullet.damage;
            //if (health <= 0)
                //Die();

            Destroy(bullet.gameObject);
        }

    }

    public void Shoot()
    {
        Enemy e = factory.GetClosestEnemy(transform.position);
        if (e == null) return;
        if (Vector3.Distance(e.transform.position, transform.position) > Range) return;

        Vector3 direction = e.transform.position - transform.position;

        var bullet = Instantiate(this.bullet, transform.position, Quaternion.identity).GetComponent<Bullet>();

        bullet.damage = 50f;
        bullet.owner = "Player";
        bullet.direction = direction.normalized;
    }
}
