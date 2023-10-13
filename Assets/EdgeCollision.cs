using UnityEngine;

public class EdgeCollision : MonoBehaviour
{
    void OnTriggerEnter(Collider collider){        
        collider.transform.parent.gameObject.GetComponent<Unit>().InstantKill();
    }
}
