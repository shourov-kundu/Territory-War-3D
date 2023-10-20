using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    public int maxHP;
    public int currentHP;
    [SerializeField]
    Image progressImage;
    [SerializeField]
    Canvas canvas;
    void Start(){
        if (canvas.TryGetComponent<FaceCamera>(out FaceCamera faceCamera))
        {
            faceCamera.Camera = Camera.main;
        }
    }
    public void InstantKill(){
        currentHP = 0;
        gameObject.SetActive(false);
        Debug.Log(name + " died!");
    }
    public void takeDamage(float damage){
        currentHP -= (int)Mathf.Max(damage, 0f);
        progressImage.fillAmount = (float)currentHP/maxHP;
        if (currentHP <= 0)
            InstantKill();
    }
    public bool isAlive(){
        return currentHP > 0;
    }
}
