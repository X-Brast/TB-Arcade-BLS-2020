/*
 * Auteurs :     Alexandre Monteiro Marques
 * Date :        29 Juin 2020
 *
 * Fichier :     SearchPlayer.cs
 * Description : Permet de créer un panel par joueur connecté au jeu
 */

using BlueConnect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameSelection {
    public class SearchPlayer : MonoBehaviour
    {
        // Definit dans l'editeur Unity
        public GameObject panelPlayer;
        public Canvas canvas; // Permet de définir le parent du panelPlayer

        private const float DELAY = 1.0f;

        private Color[] colorsPlayer = {
            new Color(0.91f, 0.72f, 0.71f), 
            new Color(0.09f, 0.37f, 0.29f),
            new Color(0f, 0.85f, 0.61f),
            new Color(1f, 0.69f, 0.36f),
            new Color(1f, 0.62f, 0.52f),
            new Color(0.62f, 0.45f, 1f)};
        private bool[] colorUsed = {false,false,false,false,false,false};
        private string[] spritePlayer = {
            "PlayerCharacter/Bear",
            "PlayerCharacter/Dog",
            "PlayerCharacter/Duck",
            "PlayerCharacter/Panda",
            "PlayerCharacter/Pig",
            "PlayerCharacter/Rabbit",
            "PlayerCharacter/Tiger"
        };
        private bool[] spriteUsed = {false,false,false,false,false,false,false};

        private int nbPlayer = 0;
        private float timeCurrent = 0.0f;

        private LinkedList<CommunicationDeviceBLS> ldbFinished;
        private FinderDevicesBLS fdb;
        private CheckDeviceBLSConnected cdbc;

        /**
        * Crée le panel du joueur si il est déjà existant(restart d'une partie) sinon cherche de nouveau joueur.
        */
        void Start() {
            ldbFinished = new LinkedList<CommunicationDeviceBLS>();
            fdb = FinderDevicesBLS.Instance;
            cdbc = CheckDeviceBLSConnected.Instance;

            foreach(var device in fdb.GetListDevicesBLS())
                CreateCharacterPlayer(device);

            StartCoroutine(FindingDevices());
        }

        /**
        * Crée le panel utilisateur
        * @param    device  Le device qui contient les informations de l'arduino
        */
        private void CreateCharacterPlayer(CommunicationDeviceBLS device){
            ldbFinished.AddFirst(device);

            int x = (150 + (nbPlayer / 2) * 300) * (nbPlayer % 2 == 0 ? 1 : -1);
            Vector3 position = new Vector3(x, -375, 0);

            GameObject go = Instantiate(panelPlayer, position, Quaternion.identity);
            go.transform.SetParent(canvas.transform, false);
            
            if(!device.isPlayerDefined){
                int idColor;
                int idCharacter;
                do{
                    idColor = Random.Range(0, colorUsed.Length);
                } while(colorUsed[idColor]);
                do{
                    idCharacter = Random.Range(0, spriteUsed.Length);
                } while(spriteUsed[idCharacter]);

                colorUsed[idColor] = true;
                spriteUsed[idCharacter] = true;
                device.idColor = idColor;
                device.idCharacter = idCharacter;
                device.colorPlayer = colorsPlayer[idColor];
                device.characterPlayer = Resources.Load<Sprite>(spritePlayer[idCharacter]);
                device.isPlayerDefined = true;
            } else {
                colorUsed[device.idColor] = true;
                spriteUsed[device.idCharacter] = true;
            }

            GameObject imagePlayer  = go.transform.GetChild(0).gameObject;
            GameObject textPlayer   = go.transform.GetChild(1).gameObject;

            go.GetComponent<Image>().color              = device.colorPlayer;
            imagePlayer.GetComponent<Image>().sprite    = device.characterPlayer;
            textPlayer.GetComponent<Text>().text        = device.surnameDevice;

            nbPlayer++;
        }

        /**
        * Cherche de nouveau joueur -> Verifie si les joueurs sont toujours connecté -> Créer panel si nouveau joueur
        */
        IEnumerator FindingDevices(){

            while(true){
                fdb.FindDevices(this);

                while(fdb.IsRunning()){
                    yield return new WaitForSeconds(0.3f);
                }

                cdbc.Start(this);

                while(cdbc.IsRunning()){
                    yield return new WaitForSeconds(0.3f);
                }

                LinkedList<CommunicationDeviceBLS> ldb = fdb.GetListDevicesBLS();

                foreach(var device in ldb){
                    if(!ldbFinished.Contains(device)){
                        CreateCharacterPlayer(device);
                    }
                }
            }
        }
    }
}
