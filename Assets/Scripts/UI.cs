using UnityEngine;
using UnityEngine.UIElements;

public class UI : MonoBehaviour
{
    public BattleSystem battleSystem;
    void Start(){
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        
        Button grenadeButton = root.Q<Button>("Grenade");
        Button pistolButton = root.Q<Button>("Pistol");

        grenadeButton.clicked += () => battleSystem.selectWeapon(Weapon.GRENADE);
        pistolButton.clicked += () => battleSystem.selectWeapon(Weapon.PISTOL);
    }
}
