﻿/*
 * Auteurs :     Alexandre Monteiro Marques
 * Date :        13 Juillet 2020
 *
 * Fichier :     RaceDisplayScore.cs
 * Description : Il permet d'afficher les resultats de chaque joueur
 */

using BlueConnect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace RaceHeart {
    public class RaceDisplayScore : MonoBehaviour
    {
        // Definit dans l'editeur Unity
        public GameObject panelScorePlayer;
        public Canvas canvas; // Canvas principale. Utile pour position les panel correctement
        
        private LoaderScene loaderScene; // Permet de changer de Scene 
        private List<GameObject> buttons; // Contient tous les boutons d'itéreations 

        private CheckDeviceBLSConnected cdbc = CheckDeviceBLSConnected.Instance;
        private int     idButtonSelect = 0;
        private bool    isFinishLoopCheck = false;

        /**
        * Start est appelé avant la première actualisation de la frame
        * Il va créer un panel pour joueur pour que ceci puissent voir son score.
        */
        void Start() {
            loaderScene = GameObject.Find("LoaderScene").GetComponent<LoaderScene>();

            buttons = new List<GameObject>();
            buttons.Add(GameObject.Find("RestartGame")); 
            buttons.Add(GameObject.Find("SelectionGameButton"));
            buttons.Add(GameObject.Find("QuitButton"));
            EventSystem.current.SetSelectedGameObject(buttons[idButtonSelect], null);

            LinkedList<CommunicationDeviceBLS> ldb = FinderDevicesBLS.Instance.GetListDevicesBLS();

            int nbPlayer = ldb.Count;

            if(nbPlayer == 0) {
                loaderScene.LoadLevelSelection(0);
                return;
            }

            // Vérification que tous les devices se sont arretés
            bool isDeviceStopped = true;
            while (isDeviceStopped) {
                isDeviceStopped = false;
                foreach(var device in ldb) {
                    isDeviceStopped |= device.isRunning;
                }
            }

            int widthCanvas = (int)canvas.GetComponent<RectTransform>().rect.width;
            int sizeXPanel = widthCanvas / nbPlayer;
            int x = (widthCanvas - sizeXPanel) / -2;
            int y = (int)panelScorePlayer.transform.position.y;
            int z = (int)panelScorePlayer.transform.position.z;

            foreach (var device in ldb) {
                Vector3 position = new Vector3(x, y, z);
                x += sizeXPanel;

                GameObject go = Instantiate(panelScorePlayer, position, Quaternion.identity);
                go.transform.SetParent(canvas.transform, false);
                
                GameObject namePlayer       = go.transform.GetChild(0).gameObject;
                GameObject timePlayer       = go.transform.GetChild(2).gameObject;
                GameObject goodHitPlayer    = go.transform.GetChild(5).gameObject;
                GameObject badHitPlayer     = go.transform.GetChild(7).gameObject;
                GameObject comboPlayer      = go.transform.GetChild(9).gameObject;
                GameObject imagePlayer      = go.transform.GetChild(10).gameObject;
                
                go.GetComponent<Image>().color          = device.colorPlayer;
                namePlayer.GetComponent<Text>().text    = device.surnameDevice;
                goodHitPlayer.GetComponent<Text>().text = PlayerPrefs.GetInt("RaceGoodHit" + device.surnameDevice) + "";
                badHitPlayer.GetComponent<Text>().text  = PlayerPrefs.GetInt("RaceBadHit" + device.surnameDevice) + "";
                comboPlayer.GetComponent<Text>().text   = PlayerPrefs.GetInt("RaceHighstreak" + device.surnameDevice) + "";
                imagePlayer.GetComponent<Image>().sprite= device.characterPlayer;
                timePlayer.GetComponent<Text>().text = PlayerPrefs.GetInt("RaceFinish" + device.surnameDevice) == 1 ? ConvertSecondToMS((int)PlayerPrefs.GetFloat("RaceScore" + device.surnameDevice)) : "Undefined";
            }

            isFinishLoopCheck = true;
        }

        /**
        * Verification que les devices sont toujours connecté
        */
        void Update() {
            if(isFinishLoopCheck) {
                StartCoroutine(CheckDevices());
                isFinishLoopCheck = false;
            }
        }

        /**
        * Verifie que les devices sont connecté et si l'utilisateur a selectionné un bouton.
        */
        IEnumerator CheckDevices() {
            cdbc.Start(this);

            while(cdbc.IsRunning())
                yield return new WaitForSeconds(.3f);

            if(cdbc.GetIsSelect()){
                EventSystem.current.currentSelectedGameObject.GetComponent<Button>().onClick.Invoke();
                yield break;
            }

            if(cdbc.GetIsNeedMove()) {
                idButtonSelect = (++idButtonSelect) % buttons.Count;
                EventSystem.current.SetSelectedGameObject(buttons[idButtonSelect], null);
            }

            isFinishLoopCheck = true;

            if(FinderDevicesBLS.Instance.NbDevicesBLS() == 0)
                loaderScene.LoadLevelSelection(0);
        }

        /**
        * Converti des seconds en heure minute second 
        * @param    second  les seconds à convertir
        * @return   Chaine de caractère qui contient minute second
        */
        private string ConvertSecondToMS(int second) {
            Debug.Log(second);
            string hours = "";
            int s = second % 60;
            int m = second / 60;

            if(m > 0)
                hours += m + "'";
            hours += s + "''";

            return hours;
        }
    }
}
