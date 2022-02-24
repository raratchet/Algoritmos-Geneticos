using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage;
    public string owner;

    [SerializeField]
    float speed;

    [SerializeField]
    float lifeTime;

    public Vector3 direction;

    public Enemy enemy;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Life());
    }

    IEnumerator Life()
    {
        yield return new WaitForSeconds(lifeTime);
        Destroy(this.gameObject);
    }

    public void ReportDamage()
    {
        enemy?.ReportDamage(damage);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = transform.position + (direction * speed * Time.deltaTime);
    }
}
