/*
 * Auteurs :     Alexandre Monteiro Marques
 * Date :        8 Juillet 2020
 *
 * Fichier :     HeroDisplayScore.cs
 * Description : Il permet d'afficher les resultats de chaque joueur
 */

using BlueConnect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace HeartHero {
    public class HeroDisplayScore : MonoBehaviour
    {
        // Definit dans l'editeur Unity
        public GameObject panelScorePlayer;
        public Canvas canvas; // Canvas principale. Utile pour position les panel correctement
        
        private LoaderScene loaderScene; // Permet de changer de Scene
        private List<GameObject> buttons;

        private CheckDeviceBLSConnected cdbc = CheckDeviceBLSConnected.Instance;
        private int     idButtonSelect = 0;
        private bool    isFinishLoopFindCheck = false;

        /**
        * Start est appelé avant la première actualisation de la frame
        * Il va créer un panel pour joueur pour que ceci puissent voir son score.
        */
        void Start() {
            loaderScene = GameObject.Find("LoaderScene").GetComponent<LoaderScene>();

            buttons = new List<GameObject>();
            buttons.Add(GameObject.Find("RestartButton")); 
            buttons.Add(GameObject.Find("SelectionGameButton"));
            buttons.Add(GameObject.Find("QuitButton"));
            EventSystem.current.SetSelectedGameObject(buttons[idButtonSelect], null);

            LinkedList<CommunicationDeviceBLS> ldb = FinderDevicesBLS.Instance.GetListDevicesBLS();

            if(ldb.Count == 0){
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

            float widthCanvas = canvas.GetComponent<RectTransform>().rect.width;
            float sizeXPanel = widthCanvas / ldb.Count;
            float x = (widthCanvas - sizeXPanel) / -2;
            float y = panelScorePlayer.transform.position.y;
            float z = panelScorePlayer.transform.position.z;

            foreach (var device in ldb) {
                Vector3 position = new Vector3(x, y, z);
                x += sizeXPanel;

                GameObject go = Instantiate(panelScorePlayer, position, Quaternion.identity);
                go.transform.SetParent(canvas.transform, false);
                
                GameObject namePlayer   = go.transform.GetChild(0).gameObject;
                GameObject scorePlayer  = go.transform.GetChild(2).gameObject;
                GameObject hitPlayer    = go.transform.GetChild(4).gameObject;
                GameObject maxHit       = go.transform.GetChild(6).gameObject;
                GameObject comboPlayer  = go.transform.GetChild(8).gameObject;
                GameObject imagePlayer  = go.transform.GetChild(9).gameObject;
                
                go.GetComponent<Image>().color          = device.colorPlayer;
                namePlayer.GetComponent<Text>().text    = device.surnameDevice;
                scorePlayer.GetComponent<Text>().text   = PlayerPrefs.GetInt("HeroScore" + device.surnameDevice) + "";
                hitPlayer.GetComponent<Text>().text     = PlayerPrefs.GetInt("HeroNotesHit" + device.surnameDevice) + "";
                maxHit.GetComponent<Text>().text        = PlayerPrefs.GetInt("HeroNotesMax") + "";
                comboPlayer.GetComponent<Text>().text   = PlayerPrefs.GetInt("HeroHighstreak" + device.surnameDevice) + "";
                imagePlayer.GetComponent<Image>().sprite= device.characterPlayer;
            }

            isFinishLoopFindCheck = true;
        }

        /**
        * Verification que les devices sont toujours connecté
        */
        void Update(){
            if(isFinishLoopFindCheck){
                StartCoroutine(CheckDevices());
                isFinishLoopFindCheck = false;
            }
        }

        /**
        * Verifie que les devices sont connecté et si l'utilisateur a selectionné un bouton.
        */
        IEnumerator CheckDevices(){
            cdbc.Start(this);

            while(cdbc.IsRunning()){
                yield return new WaitForSeconds(.3f);
            }

            if(cdbc.GetIsSelect()){
                EventSystem.current.currentSelectedGameObject.GetComponent<Button>().onClick.Invoke();
            }
            if(cdbc.GetIsNeedMove()){
                idButtonSelect = (++idButtonSelect) % buttons.Count;
                EventSystem.current.SetSelectedGameObject(buttons[idButtonSelect], null);
            }

            isFinishLoopFindCheck = true;

            if(FinderDevicesBLS.Instance.NbDevicesBLS() == 0)
                loaderScene.LoadLevelSelection(0);
        }
    }
}
