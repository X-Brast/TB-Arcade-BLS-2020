/*
 * Auteurs :     Alexandre Monteiro Marques
 * Date :        13 Juillet 2020
 *
 * Fichier :     RaceGameLogic.cs
 * Description : Il permet conserver les informations du jeu Race Heart des joueurs. Ces informations peuvent être reutiliser
                 à chaque nouvelle partie des joueurs. Chaque Joueurs a ces propres informations.
                 Information sur PlayerPrefs : https://docs.unity3d.com/ScriptReference/PlayerPrefs.html
 */

using BlueConnect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace RaceHeart {
    public class RaceGameLogic : MonoBehaviour
    {
        // Definit dans l'editeur Unity
        public GameObject loaderScene; // Permet de changer de Scene 
        
        private int nbPlayerFinish = 0; // nombre de joueur ayant fini la course.

        /**
        * Indique la position du joueur dans la course
        * @param    parent    C'est le GameObjet qui contient l'objet courant (RacePanelPlayer) 
        * @param    position  La position du joueur
        */
        private void DisplayScore(GameObject parent, int position) {
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

        /**
        * Reinitialise les informations du joueur
        * @param    nameDevice  Le nom du device à réaliser l'opération
        */
        public void ResetDataPlayer(string nameDevice) {
            if(!PlayerPrefs.HasKey("RaceHighstreak" + nameDevice))
                PlayerPrefs.SetFloat("RaceHighstreak" + nameDevice, 0);
            PlayerPrefs.SetFloat("RaceScore" + nameDevice, Time.fixedTime);
            PlayerPrefs.SetInt("RaceStreak" + nameDevice, 0);
            PlayerPrefs.SetInt("RaceGoodHit" + nameDevice, 0);
            PlayerPrefs.SetInt("RaceBadHit" + nameDevice, 0);
            PlayerPrefs.SetInt("RaceMult" + nameDevice, 0);
        }

        /**
        * Indique que le joueur a fait un bon hit 
        * @param    nameDevice  Le nom du device qui a fait ce hit
        */
        public void GoodStreak(string nameDevice) {
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

        /**
        * Indique que le joueur a fait un mauvais hit
        * @param    nameDevice  Le nom du device qui a fait ce hit
        */
        public void BadStreak(string nameDevice) {
            PlayerPrefs.SetInt("RaceBadHit" + nameDevice, PlayerPrefs.GetInt("RaceBadHit" + nameDevice)+1);
            ResetStreak(nameDevice);
        }

        /**
        * Reinitialise les hit et mult du joueur
        * @param    nameDevice  Le nom du device qui a fait ce hit
        */
        public void ResetStreak(string nameDevice) {
            PlayerPrefs.SetInt("RaceMult" + nameDevice, 1);
            PlayerPrefs.SetInt("RaceStreak" + nameDevice, 0);
        }

        /**
        * Donne le multiplicateur courant du joueur 
        * @param    nameDevice  Le nom du device qui a fait ce hit
        * @return   Le multiplicateur du joueur
        */
        public int GetMult(string nameDevice) {
            return PlayerPrefs.GetInt("RaceMult" + nameDevice);
        } 

        /**
        * Indique que le joueur a terminé sa course
        * @param    parent      C'est le GameObjet attribué au joueur (RacePanelPlayer) 
        * @param    nameDevice  Le nom du device qui a fait ce hit
        */
        public void RaceFinish(GameObject parent, string nameDevice) {
            ++nbPlayerFinish;
            DisplayScore(parent, nbPlayerFinish);
            PlayerPrefs.SetFloat("RaceScore" + nameDevice, Time.fixedTime - PlayerPrefs.GetFloat("RaceScore" + nameDevice));
            if(PlayerPrefs.GetFloat("RaceScore" + nameDevice) > PlayerPrefs.GetFloat("RaceHighscore" + nameDevice));
                PlayerPrefs.SetFloat("RaceHighscore" + nameDevice, PlayerPrefs.GetFloat("RaceScore" + nameDevice));

            if(nbPlayerFinish == FinderDevicesBLS.Instance.NbDevicesBLS()) {
                LinkedList<CommunicationDeviceBLS> ldb = FinderDevicesBLS.Instance.GetListDevicesBLS();
                foreach (var device in ldb)
                    device.StopGame();
                loaderScene.GetComponent<LoaderScene>().LoadLevelScore(4);
            }
        }
    }
}
