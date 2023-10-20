using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour, IRemovable
{
    [Range(0,600)]
    public float force;
    [Range(0,7)]
    public float radius;
    public GameObject explosionEffect;
    Rigidbody rb;
    int bounces;
    bool removed;
    void Start(){
        rb = GetComponent<Rigidbody>();    
    } 
    void OnCollisionEnter(Collision collision){
        bounces += 1;
    }
    public void Activate(){
        StartCoroutine(Trigger());
    }
    IEnumerator Trigger(){
        bounces = 0;
        yield return new WaitUntil(() => bounces == 4);
        Instantiate(explosionEffect, gameObject.transform.position, gameObject.transform.rotation);
        Collider[] things = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider collider in things){
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            if (collider.gameObject.tag == "Player"){
                GameObject p = collider.transform.parent.gameObject;
                p.GetComponent<Unit>().takeDamage(50-50/radius*Vector3.Distance(transform.position, p.transform.position));
                p.GetComponent<Rigidbody>().AddExplosionForce(force, transform.position, radius);
            }
        }
        removed = true;
        Destroy(gameObject);
    }
    public void Remove(){
        removed = true;
    }
    public bool IsRemoved(){
        return removed;
    }

}
