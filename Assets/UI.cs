using UnityEngine;
using UnityEngine.UIElements;

public class UI : MonoBehaviour
{
    public BattleSystem battleSystem;
    void Start(){
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        
        Button grenadeButton = root.Q<Button>("Grenade");
        Button pistolButton = root.Q<Button>("Pistol");

        grenadeButton.clicked += () => StartCoroutine(battleSystem.useWeapon(Weapon.GRENADE));
        pistolButton.clicked += () => StartCoroutine(battleSystem.useWeapon(Weapon.PISTOL));
    }
}
