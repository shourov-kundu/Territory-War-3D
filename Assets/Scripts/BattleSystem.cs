using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.Linq;
using UnityEngine.UIElements;
using System;

public enum BattleState {START, REDTURN, BLUETURN, REDWINS, BLUEWINS, DRAW}
public enum Weapon {GRENADE, PISTOL}
public class BattleSystem : MonoBehaviour
{
    [Range(2, 6)]
    public int playerCount;
    [Header("Keybinds")]
    public KeyCode passKey = KeyCode.P;
    public KeyCode weaponsKey = KeyCode.Q;
    public GameObject mapCamera;
    public GameObject playerCamera;
    public GameObject redTeam;
    public GameObject blueTeam;
    public GameObject redPlayer;
    public GameObject bluePlayer;
    public Vector3[] redSpawns;
    public Vector3[] blueSpawns;
    public GameObject weaponWheel;
    public GameObject powerMeter;
    public GameObject distanceMeter;
    [Header("Weapons")]
    public GameObject grenade;
    BattleState state;
    Coroutine turn = null;
    Coroutine weaponUse = null;
    GameObject weaponObj;
    GameObject[] redPlayers;
    GameObject[] bluePlayers;
    public GameObject currentPlayer;
    public Vector3 startingPoint;
    int r;
    int b;
    KeyCode pressedKey;
    KeyCode[] inputKeys;
    ThirdPersonCamera tpc;
    CinemachineFreeLook cfl;
    bool weaponFired;
    GameObject mc;
    LineRenderer lr;
    float angle;
    float lineLength = 1.7f;
    float powerSpeed = .5f;
    float minPower = 5f;
    float maxPower = 30f;
    ProgressBar powerBar;
    ProgressBar distanceBar;
    void Start(){
        state = BattleState.START;
        currentPlayer = null;
        redPlayers = new GameObject[playerCount];
        bluePlayers = new GameObject[playerCount];
        inputKeys = new KeyCode[] {passKey, weaponsKey};
        tpc = playerCamera.GetComponent<ThirdPersonCamera>();
        cfl = playerCamera.GetComponent<CinemachineFreeLook>();
        mc = GameObject.FindWithTag("MainCamera");
        lr = GetComponent<LineRenderer>();
        lr.enabled = false;
        angle = 0f;
        weaponWheel.GetComponent<UIDocument>().rootVisualElement.style.display = DisplayStyle.None;
        powerMeter.GetComponent<UIDocument>().rootVisualElement.style.display = DisplayStyle.Flex;
        distanceMeter.GetComponent<UIDocument>().rootVisualElement.style.display = DisplayStyle.Flex;
        powerBar = (ProgressBar)powerMeter.GetComponent<UIDocument>().rootVisualElement.Q("PowerMeter");
        distanceBar = (ProgressBar)distanceMeter.GetComponent<UIDocument>().rootVisualElement.Q("DistanceMeter");
        powerBar.style.display = DisplayStyle.None;
        distanceBar.style.display = DisplayStyle.None;
        StartCoroutine(SetupBattle());
    }
    void Update(){
        if (currentPlayer is not null && !currentPlayer.GetComponent<Unit>().isAlive() && turn is not null){
            StartCoroutine(TurnEnd());
        }
    }
    IEnumerator TurnEnd(){
        StopCoroutine(turn);
        turn = null;
        if (weaponObj is not null){
            Destroy(weaponObj);
            weaponObj = null;
        }
        updateWeaponState(false);
        currentPlayer.GetComponent<PlayerMovement>().enabled = false;
        yield return new WaitForSeconds(2f);
        bool redDead = isTeamDead(redPlayers);
        bool blueDead = isTeamDead(bluePlayers);
        if (redDead && blueDead){
            Draw();
        } else if (redDead){
            BlueWins();
        } else if (blueDead){
            RedWins();
        } else if (state == BattleState.REDTURN){
            BlueTurn();
        } else{
            RedTurn();
        }
    }
    IEnumerator SetupBattle(){
        Quaternion redDirection = Quaternion.Euler(0, 0, 0);
        Quaternion blueDirection = Quaternion.Euler(0, 180, 0);
        for (int i = 0; i < playerCount; i++){
            redPlayers[i] = Instantiate(redPlayer, redSpawns[i], redDirection, redTeam.transform);
            bluePlayers[i] = Instantiate(bluePlayer, blueSpawns[i], blueDirection, blueTeam.transform);
            redPlayers[i].GetComponent<Unit>().SetName("Player " + (i+1));
            bluePlayers[i].GetComponent<Unit>().SetName("Player " + (i+1));
        }
        yield return new WaitForSeconds(1f);
        playerCamera.SetActive(true);
        mapCamera.SetActive(false);
        RedTurn();
    }
    void RedTurn(){
        state = BattleState.REDTURN;
        r = (r+1) % playerCount;
        Unit unit = redPlayers[r].GetComponent<Unit>();
        while (!unit.isAlive()){
            r = (r+1) % playerCount;
            unit = redPlayers[r].GetComponent<Unit>();
        }
        turn = StartCoroutine(takeTurn(redPlayers[r]));
    }
    IEnumerator takeTurn(GameObject player){
        weaponFired = false;
        currentPlayer = player;
        // distanceBar.value = 0;
        LoadCameraToPlayer(currentPlayer);
        yield return new WaitForSeconds(1f);
        startingPoint = currentPlayer.transform.position;
        currentPlayer.GetComponent<PlayerMovement>().enabled = true;
        distanceBar.style.display = DisplayStyle.Flex;
        BattleState currentState = state;
        while (state == currentState){
            yield return new WaitUntil(keyPress);
            if (pressedKey == passKey && currentPlayer.GetComponent<PlayerMovement>().enabled){
                StartCoroutine(TurnEnd());
                yield break;
            } else if (pressedKey == weaponsKey && !weaponFired){
                yield return new WaitUntil(() => !Input.GetKey(weaponsKey) && currentPlayer.GetComponent<PlayerMovement>().IsGrounded());
                if (!weaponFired){
                    if (weaponUse is not null){
                        StopCoroutine(weaponUse);
                        weaponUse = null;
                        Destroy(weaponObj);
                    }
                    lr.enabled = false;
                    powerBar.style.display = DisplayStyle.None;
                    bool active = weaponWheel.GetComponent<UIDocument>().rootVisualElement.style.display == DisplayStyle.Flex;
                    updateWeaponState(!active); // turn on/off weapon UI
                    currentPlayer.GetComponent<PlayerMovement>().enabled = active; // player movement
                    distanceBar.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }
        }
    }
    public void selectWeapon(Weapon weapon){
        updateWeaponState(false);
        weaponUse = StartCoroutine(useWeapon(weapon));
    }
    IEnumerator useWeapon(Weapon weapon){
        switch (weapon){
            case Weapon.GRENADE:
                weaponObj = Instantiate(grenade, currentPlayer.transform.position + new Vector3(0f,2f,0f), playerCamera.transform.rotation);
                lr.enabled = true;
                lr.SetPositions(new Vector3[2] {weaponObj.transform.position, weaponObj.transform.position + lineLength*currentPlayer.transform.forward});
                angle = 0;
                Vector3 f = currentPlayer.transform.forward;
                while (true){
                    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetKey(KeyCode.T) || Input.GetKey(KeyCode.R) || f != currentPlayer.transform.forward);
                    if (Input.GetKeyDown(KeyCode.Space))
                        break;
                    if (Input.GetKey(KeyCode.T) && !Input.GetKey(KeyCode.R))
                        angle = Mathf.Min(angle + 3*Time.deltaTime, .95f);
                    if (Input.GetKey(KeyCode.R) && !Input.GetKey(KeyCode.T))
                        angle = Mathf.Max(angle - 3*Time.deltaTime, -.95f);
                    float x = Mathf.Cos(angle)*Vector3.Dot(currentPlayer.transform.forward, Vector3.right);
                    float y = Mathf.Sin(angle);
                    float z = Mathf.Cos(angle)*Vector3.Dot(currentPlayer.transform.forward, Vector3.forward);
                    lr.SetPosition(1, weaponObj.transform.position + lineLength*new Vector3(x,y,z));
                    f = currentPlayer.transform.forward;
                }
                powerBar.style.display = DisplayStyle.Flex;
                bool increasing = true;                
                powerBar.value = powerBar.lowValue;
                float speed = (powerBar.highValue - powerBar.lowValue)*powerSpeed;
                while (Input.GetKey(KeyCode.Space)){
                    yield return new WaitUntil(() => Input.GetKey(KeyCode.Space));
                    if (increasing){
                        powerBar.value = Mathf.Min(powerBar.value + speed*Time.deltaTime, powerBar.highValue);
                        if (powerBar.value == powerBar.highValue)
                            increasing = false;
                    } else {
                        powerBar.value = Mathf.Max(powerBar.value - speed*Time.deltaTime, powerBar.lowValue);
                        if (powerBar.value == powerBar.lowValue)
                            increasing = true;
                    }
                }
                yield return new WaitUntil(() => !Input.GetKey(KeyCode.Space));
                weaponFired = true;
                lr.enabled = false;
                weaponObj.GetComponent<Rigidbody>().isKinematic = false;
                Vector3 throwDirection = (lr.GetPosition(1) - lr.GetPosition(0)).normalized;
                float power = Mathf.Lerp(minPower, maxPower, powerBar.value/powerBar.highValue);
                weaponObj.GetComponent<Rigidbody>().AddForce(throwDirection * power, ForceMode.VelocityChange);
                weaponObj.GetComponent<Grenade>().Activate();
                powerBar.style.display = DisplayStyle.None;
                yield return new WaitUntil(() => weaponObj.GetComponent<Grenade>().IsRemoved());
                StartCoroutine(TurnEnd());
                break;
            case Weapon.PISTOL:
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
                weaponFired = true;
                Debug.Log("Pistol fired");
                yield return new WaitForSeconds(1f);
                StartCoroutine(TurnEnd());
                break;
        }
    }
    void updateWeaponState(bool active){
        weaponWheel.GetComponent<UIDocument>().rootVisualElement.style.display = active ?  DisplayStyle.Flex : DisplayStyle.None;
        UnityEngine.Cursor.visible = active;
        UnityEngine.Cursor.lockState = active ? CursorLockMode.Confined : CursorLockMode.Locked;
        tpc.enabled = !active;
        cfl.enabled = !active;
    }
    void BlueTurn(){
        state = BattleState.BLUETURN;
        b = (b+1) % playerCount;
        Unit unit = bluePlayers[b].GetComponent<Unit>();
        while (!unit.isAlive()){
            b = (b+1) % playerCount;
            unit = bluePlayers[b].GetComponent<Unit>();
        }
        turn = StartCoroutine(takeTurn(bluePlayers[b]));
    }
    bool keyPress(){
        foreach (KeyCode k in inputKeys){   
            if (Input.GetKey(k) && currentPlayer.GetComponent<PlayerMovement>().IsGrounded()){
                pressedKey = k;
                return true;
            }
        }
        return false;
    }
    void BlueWins(){
        state = BattleState.BLUEWINS;
        Debug.Log("Blue Wins!");
    }
    void RedWins(){
        state = BattleState.REDWINS;
        Debug.Log("Red Wins!");
    }
    void Draw(){
        state = BattleState.DRAW;
        Debug.Log("Draw...");
    }
    void LoadCameraToPlayer(GameObject player){
        distanceBar.value = 0;
        tpc.orientation = player.transform.Find("Orientation");
        tpc.player = player.transform;
        tpc.playerObj = player.transform.Find("Player Body");
        tpc.rb = player.GetComponent<Rigidbody>();

        cfl.Follow = player.transform;
        cfl.LookAt = player.transform;
    }
    bool isTeamDead(GameObject[] team){
        foreach (GameObject player in team){
            if (player.GetComponent<Unit>().isAlive())
                return false;
        }
        return true;
    }
}