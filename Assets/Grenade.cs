using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    [Range(0,600)]
    public float force;
    [Range(0,7)]
    public float radius;

    public GameObject explosionEffect;
    Rigidbody rb;
    void Start(){
        rb = GetComponent<Rigidbody>();    
    }

    public void Activate(){
        StartCoroutine(Trigger());
    }
    IEnumerator Trigger(){
        yield return new WaitForSeconds(1f);
        Instantiate(explosionEffect, gameObject.transform.position, gameObject.transform.rotation);
        Collider[] things = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider collider in things){
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            if (collider.gameObject.tag == "Player"){
                collider.transform.parent.gameObject.GetComponent<Rigidbody>().AddExplosionForce(force, transform.position, radius);
            }
        }
        Destroy(gameObject);
    }
}
