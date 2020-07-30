/*
 * Auteurs :     Alexandre Monteiro Marques
 * Date :        23 Juillet 2020
 *
 * Fichier :     HeroCreatePlayer.cs
 * Description : Il va permettre de créer pour chaque joueur un panel et une camera qui lui seront dédié.
                 Il y a un vérification pour savoir si l'arduino du joueur est toujours connecté
                 Si aucun joueur n'est connecté, retourne à la selection de jeu
 */

using BlueConnect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace HeartHero {
    public class HeroCreatePlayer : MonoBehaviour
    {
        // Definit dans l'editeur Unity
        public Canvas canvas; // Le canvas qui contient les objets nécéssaire au joueur.
        public Camera ownCamera; // La caméra destiné au joueur
        public Canvas loadCanvas; // Canvas d'attente avant le démarage du jeu

        public bool isLoading = true; // Permet de savoir si les panels sont entrain d'être créer

        /**
        * Verifie si aucun joueur est connecté
        * @return True si aucun joueur n'est connecté
        */
        private bool IsNoPlayer() {
            if(FinderDevicesBLS.Instance.NbDevicesBLS() == 0) {
                GameObject.Find("LoaderScene").GetComponent<LoaderScene>().LoadLevelSelection(0);
                return true;
            }
            return false;
        }

        void Start() {
            if(IsNoPlayer())
                return;
            
            StartCoroutine(LoadPanelPlayerGaming());
        }

        /**
        * Créer les panels pour chacun des joueurs
        */
        IEnumerator LoadPanelPlayerGaming(){
            LinkedList<CommunicationDeviceBLS> ldb = FinderDevicesBLS.Instance.GetListDevicesBLS();
            
            // on lance toutes les connexions Si problème on ne va pas créer un panneau pour le joueur
            foreach (var device in ldb) {
                device.StartGame(this);
            }

            if(IsNoPlayer())
                yield return null;

            HeroGameLogic hgl = GameObject.Find("GameLogic").GetComponent<HeroGameLogic>();

            ldb = FinderDevicesBLS.Instance.GetListDevicesBLS();
            int nbPlayer = ldb.Count;
            float w = 1.0f/nbPlayer;
            int counter = 0;

            foreach(var device in ldb) {
                int layer = LayerMask.NameToLayer("Player" + (counter+1).ToString());

                hgl.ResetDataPlayer(device.surnameDevice);

                Camera cam = Instantiate(ownCamera, new Vector3(0, 0,-10), Quaternion.identity);
                cam.rect = new Rect(w * counter, 0.0f, w, 1.0f);
                cam.backgroundColor = device.colorPlayer;
                cam.cullingMask = (1 << layer) + (1 << LayerMask.NameToLayer("UI"));

                Canvas can = Instantiate(canvas, new Vector3(0, 0, 0), Quaternion.identity);
                can.renderMode = RenderMode.ScreenSpaceCamera;
                can.worldCamera = cam;

                can.transform.GetChild(0).gameObject.GetComponent< PPText >().name = "HeroScore" + device.surnameDevice;
                can.transform.GetChild(1).gameObject.GetComponent< Text >().text = device.surnameDevice;

                can.SendMessage("InitDevice", device);
                can.SendMessage("TheStart", layer);

                ++counter;
            }

            // Temps d'attente pour que le joueur se prepare pour jouer
            yield return new WaitForSeconds(3.0f);

            loadCanvas.enabled = false;
            isLoading = false;
        }
    }
}
