using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeCollision : MonoBehaviour
{
    void OnTriggerEnter(Collider collider){     
        if (collider.gameObject.tag == "Player"){
            StartCoroutine(KillPlayer(collider.transform.parent.gameObject));
        } else if (collider.gameObject.TryGetComponent<IRemovable>(out IRemovable item)){
                item.Remove();
        }
    }
    IEnumerator KillPlayer(GameObject player){
        yield return new WaitForSeconds(1f);
        player.GetComponent<Unit>().InstantKill();
    }
}
