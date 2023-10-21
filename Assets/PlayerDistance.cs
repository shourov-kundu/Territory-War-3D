using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerDistance : MonoBehaviour
{
    public BattleSystem battleSystem;
    ProgressBar distanceBar;
    void Start()
    {
        distanceBar = (ProgressBar)GetComponent<UIDocument>().rootVisualElement.Q("DistanceMeter");
    }
    void Update()
    {  
        if (battleSystem.currentPlayer is not null && battleSystem.currentPlayer.GetComponent<PlayerMovement>().enabled)
            distanceBar.value = Vector3.Distance(battleSystem.currentPlayer.transform.position, battleSystem.startingPoint)*3;
    }
}
