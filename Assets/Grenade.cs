using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
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
        Destroy(gameObject);
        Instantiate(explosionEffect, gameObject.transform.position, gameObject.transform.rotation);
    }
}
