using UnityEngine;

public class EdgeCollision : MonoBehaviour
{
    void OnTriggerEnter(Collider collider){     
        if (collider.gameObject.tag == "Player"){
            collider.transform.parent.gameObject.GetComponent<Unit>().InstantKill();
        } else if (collider.gameObject.tag == "Weapon"){
            Destroy(collider.gameObject);
        }
    }
}
