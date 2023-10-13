using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public int maxHP;
    public int currentHP;
    public void InstantKill(){
        currentHP = 0;
        gameObject.SetActive(false);
        Debug.Log(name + " died!");
    }
    void takeDamage(int damage){
        currentHP -= damage;
        if (currentHP <= 0)
            InstantKill();
    }
    public bool isAlive(){
        return currentHP > 0;
    }
}
