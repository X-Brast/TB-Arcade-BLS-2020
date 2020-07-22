using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BlueConnect;

public class RaceGameLogic : MonoBehaviour
{
    public Canvas canvas;
    public Camera camera;
    public GameObject loaderScene;
    public Canvas loadCanvas;

    private int nbPlayer;
    private int nbPlayerFinish = 0;

    // Start is called before the first frame update
    void Start() {
        LinkedList<CommunicationDeviceBLS> ldb = FinderDevicesBLS.Instance.GetListDevicesBLS();   

        if(FinderDevicesBLS.Instance.NbDevicesBLS() == 0){
            loaderScene.GetComponent<LoaderScene>().LoadLevelSelection(0);
            return;
        }

        StartCoroutine(LoadPanelPlayerGaming());
    }

    IEnumerator LoadPanelPlayerGaming(){
        LinkedList<CommunicationDeviceBLS> ldb = FinderDevicesBLS.Instance.GetListDevicesBLS();

        // on lance toutes les connexions Si problème on ne va pas créer un panneau pour le joueur
        foreach (var device in ldb) {
            device.StartGame(this);
        }

        nbPlayer = ldb.Count;
        float w = 1.0f/nbPlayer;
        float speed = 0;
        int counter = 0;

        foreach (var device in ldb) {
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

        yield return new WaitForSeconds(3.0f);

        loadCanvas.enabled = false;
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
            LinkedList<CommunicationDeviceBLS> ldb = FinderDevicesBLS.Instance.GetListDevicesBLS();
            foreach (var device in ldb){
                device.StopGame();
            } 
            loaderScene.GetComponent<LoaderScene>().LoadLevelScore(4);
        }
    }
}
