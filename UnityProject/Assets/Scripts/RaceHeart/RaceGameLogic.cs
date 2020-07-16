using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using BlueConnect;

public class RaceGameLogic : MonoBehaviour
{
    public Canvas canvas;
    public Camera camera;

    private LinkedList<ConnectorDeviceBLS> ldb = new LinkedList<ConnectorDeviceBLS>();
    private int nbPlayer;
    private int nbPlayerFinish = 0;

    // Start is called before the first frame update
    void Start() {
        //LinkedList<ConnectorDeviceBLS> ldb = FinderDevicesBLS.Instance.GetListDevicesBLS();
        ldb.AddFirst(new ConnectorDeviceBLS("BLS2020HC05HESAV01", "Monstro"));
        ldb.First.Value.colorPlayer = new Color(0.8f, 0.92f, 0.88f);
        ldb.AddFirst(new ConnectorDeviceBLS("BLS2020HC05HESAV01", "Bobo"));
        ldb.First.Value.colorPlayer = new Color(0.09f, 0.37f, 0.29f);
        ldb.AddFirst(new ConnectorDeviceBLS("BLS2020HC05HESAV01", "Carole"));
        ldb.First.Value.colorPlayer = new Color(0f, 0.85f, 0.61f);
        ldb.AddFirst(new ConnectorDeviceBLS("BLS2020HC05HESAV01", "Inconnu"));
        ldb.First.Value.colorPlayer = new Color(0.95f, 0.2f, 0f);
        ldb.AddFirst(new ConnectorDeviceBLS("BLS2020HC05HESAV01", "Richard"));
        ldb.First.Value.colorPlayer = new Color(1f, 0.62f, 0.52f);
        ldb.AddFirst(new ConnectorDeviceBLS("BLS2020HC05HESAV01", "Jessica"));
        ldb.First.Value.colorPlayer = new Color(0.62f, 0.45f, 1f);
        
        nbPlayer = ldb.Count;

        if(nbPlayer == 0){
            SceneManager.LoadScene(1);
            return;
        }

        float w = 1.0f/nbPlayer;
        float speed = 0.1f * nbPlayer;
        int counter = 0;
        foreach (var device in ldb) {
            //device.StartGame(this);

            int layer = LayerMask.NameToLayer("Player" + (counter+1).ToString());
            
            PlayerPrefs.SetInt("RaceHighstreak" + device.surnameDevice, 0);
            PlayerPrefs.SetInt("RaceScore" + device.surnameDevice, (int)Time.fixedTime);
            PlayerPrefs.SetInt("RaceStreak" + device.surnameDevice, 0);
            PlayerPrefs.SetInt("RaceGoodHit" + device.surnameDevice, 0);
            PlayerPrefs.SetInt("RaceBadHit" + device.surnameDevice, 0);
            PlayerPrefs.SetInt("RaceMult" + device.surnameDevice, 0);

            Camera cam = Instantiate(camera, new Vector3(0, 0,-10), Quaternion.identity);
            cam.rect = new Rect(0.0f, w * counter, 1.0f, w);
            cam.cullingMask = (1 << layer) + (1 << LayerMask.NameToLayer("UI"));

            Canvas can = Instantiate(canvas, new Vector3(0, 0, 0), Quaternion.identity);
            can.renderMode = RenderMode.ScreenSpaceCamera;
            can.worldCamera = cam;

            can.transform.GetChild(0).gameObject.layer = layer;
            can.transform.GetChild(0).gameObject.GetComponent< Image >().color = device.colorPlayer;
            can.transform.GetChild(0).GetChild(0).gameObject.layer = layer;
            can.transform.GetChild(0).GetChild(1).gameObject.layer = layer;
            can.transform.GetChild(0).GetChild(1).gameObject.GetComponent< Ambulance >().speed = speed;
            can.transform.GetChild(0).GetChild(1).gameObject.GetComponent< Ambulance >().device = device;

            ++counter;
        }
    }

    public void GoodStreak(string nameDevice){
        PlayerPrefs.SetInt("RaceGoodHit" + nameDevice, PlayerPrefs.GetInt("RaceGoodHit" + nameDevice)+1);
        PlayerPrefs.SetInt("RaceStreak" + nameDevice, PlayerPrefs.GetInt("RaceStreak" + nameDevice)+1);

        int streak = PlayerPrefs.GetInt("RaceStreak" + nameDevice);
        int multiplier = PlayerPrefs.GetInt("RaceMult" + nameDevice);

        if(streak > 50)
            multiplier = 10;
        else if(streak > 40)
            multiplier = 5;
        else if(streak > 30)
            multiplier = 4;
        else if(streak > 20)
            multiplier = 3;
        else if(streak > 10)
            multiplier = 2;
        else
            multiplier = 1;

        PlayerPrefs.SetInt("RaceMult" + nameDevice, multiplier);

        if(streak > PlayerPrefs.GetInt("RaceHighstreak" + nameDevice))
            PlayerPrefs.SetInt("RaceHighstreak" + nameDevice, streak);
    }

    public void BadStreak(string nameDevice){
        PlayerPrefs.SetInt("RaceBadHit" + nameDevice, PlayerPrefs.GetInt("RaceBadHit" + nameDevice)+1);
        PlayerPrefs.SetInt("RaceMult" + nameDevice, 1);
        PlayerPrefs.SetInt("RaceStreak" + nameDevice, 0);
    }

    public int GetMult(string nameDevice){
        return PlayerPrefs.GetInt("RaceMult" + nameDevice);
    }

    private void DisplayScore(GameObject parent, int position){
        string namePosition;
        switch (position) {
            case 1:
                namePosition = "Premier";
                break;
            case 2:
                namePosition = "Second";
                break;
            case 3:
                namePosition = "Troisième";
                break;
            case 4:
                namePosition = "Quatrième";
                break;
            case 5:
                namePosition = "Cinquième";
                break;
            case 6:
                namePosition = "Sixième";
                break;
            default:
                namePosition = "Inconnu";
                break;
        }
        parent.transform.GetChild(0).gameObject.active = true;
        parent.transform.GetChild(0).gameObject.GetComponent< Text >().text = namePosition;
    }

    public void RaceFinish(GameObject parent, string nameDevice){
        ++nbPlayerFinish;
        DisplayScore(parent, nbPlayerFinish);
        PlayerPrefs.SetInt("RaceScore" + nameDevice, (int)Time.fixedTime - PlayerPrefs.GetInt("RaceScore" + nameDevice));
        if(PlayerPrefs.GetInt("RaceScore" + nameDevice) > PlayerPrefs.GetInt("RaceHighscore" + nameDevice));
            PlayerPrefs.SetInt("RaceHighscore" + nameDevice, PlayerPrefs.GetInt("RaceScore" + nameDevice));

        if(nbPlayerFinish == nbPlayer){
            //LinkedList<ConnectorDeviceBLS> ldb = FinderDevicesBLS.Instance.GetListDevicesBLS();
            foreach (var device in ldb){
                //device.StopGame();
            } 
            SceneManager.LoadScene("RaceHeartClassement");
        }
    }
}
