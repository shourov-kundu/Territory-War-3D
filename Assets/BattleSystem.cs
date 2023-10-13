using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.Linq;

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
    public BattleState state;
    Coroutine turn = null;

    GameObject[] redPlayers;
    GameObject[] bluePlayers;
    GameObject currentPlayer;
    int r;
    int b;
    KeyCode pressedKey;
    KeyCode[] inputKeys;
    ThirdPersonCamera tpc;
    CinemachineFreeLook cfl;
    // Start is called before the first frame update
    void Start(){
        state = BattleState.START;
        currentPlayer = null;
        redPlayers = new GameObject[playerCount];
        bluePlayers = new GameObject[playerCount];
        inputKeys = new KeyCode[] {passKey, weaponsKey};
        tpc = playerCamera.GetComponent<ThirdPersonCamera>();
        cfl = playerCamera.GetComponent<CinemachineFreeLook>();
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
            redPlayers[i].name = "Player " + (i+1);
            bluePlayers[i].name = "Player " + (i+1);
        }
        yield return new WaitForSeconds(1f);
        // playerCamera.SetActive(true);
        // mapCamera.SetActive(false);
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
        playerCamera.SetActive(true);
        mapCamera.SetActive(false);
        currentPlayer = player;
        LoadCameraToPlayer(currentPlayer);
        yield return new WaitForSeconds(1f);
        Vector3 startingPoint = currentPlayer.transform.position;
        currentPlayer.GetComponent<PlayerMovement>().enabled = true;
        BattleState currentState = state;
        while (state == currentState){
            yield return new WaitUntil(keyPress);
            switch (pressedKey){
                case var v when v == passKey:
                    StartCoroutine(TurnEnd());
                    break;
                case var v when v == weaponsKey:
                    yield return new WaitUntil(() => !Input.GetKey(weaponsKey));
                    updateWeaponState(!weaponWheel.activeSelf);
                    currentPlayer.GetComponent<PlayerMovement>().enabled = !weaponWheel.activeSelf;
                    break;
            }
        }
    }

    public void useWeapon(Weapon weapon){
        updateWeaponState(false);
    }
    void updateWeaponState(bool active){
        weaponWheel.SetActive(active);
        Cursor.visible = active;
        if (active){
            Cursor.lockState = CursorLockMode.Confined;
        } else{
            Cursor.lockState = CursorLockMode.Locked;
        }
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
