/*
 * Auteurs :     Alexandre Monteiro Marques
 * Date :        29 Mai 2020
 *
 * Fichier :     HeroGameLogic.cs
 * Description : Il permet conserver les informations du jeu Heart Hero des joueurs. Ces informations peuvent être reutiliser
                 à chaque nouvelle partie des joueurs. Chaque Joueurs a ces propres informations.
                 Information sur PlayerPrefs : https://docs.unity3d.com/ScriptReference/PlayerPrefs.html
 */

using BlueConnect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace HeartHero {
    public class HeroGameLogic : MonoBehaviour
    {
        private bool isNotCloseGame = true;

        /**
        * Reinitialise les informations du joueur
        * @param    nameDevice  Le nom du device à réaliser l'opération
        */
        public void ResetDataPlayer(string nameDevice) {
			if(!PlayerPrefs.HasKey("HeroHighscore" + nameDevice))
                PlayerPrefs.SetFloat("HeroHighscore" + nameDevice, 0);
            PlayerPrefs.SetInt("HeroScore" + nameDevice, 0);
            PlayerPrefs.SetInt("HeroNotesHit" + nameDevice, 0);
            PlayerPrefs.SetInt("HeroStreak" + nameDevice, 0);
			PlayerPrefs.SetFloat("HeroHighstreak" + nameDevice, 0);
            PlayerPrefs.SetInt("HeroMult" + nameDevice, 1);
        }

        /**
        * Ajoute un hit au joueur. Met à jour le multiplicateur
        * @param    nameDevice  Le nom du device qui a fait ce hit
        */
        public void AddStreak(string nameDevice) {
            PlayerPrefs.SetInt("HeroStreak" + nameDevice, PlayerPrefs.GetInt("HeroStreak" + nameDevice)+1);
            PlayerPrefs.SetInt("HeroNotesHit" + nameDevice, PlayerPrefs.GetInt("HeroNotesHit" + nameDevice)+1);
            
            int streak = PlayerPrefs.GetInt("HeroStreak" + nameDevice);
            int multiplier = PlayerPrefs.GetInt("HeroMult" + nameDevice);

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

            PlayerPrefs.SetInt("HeroMult" + nameDevice, multiplier);

            if(streak > PlayerPrefs.GetInt("HeroHighStreak" + nameDevice))
                PlayerPrefs.SetInt("HeroHighstreak" + nameDevice, streak);
        }

        /**
        * Reinitialise les hit et mult du joueur
        * @param    nameDevice  Le nom du device qui a fait ce hit
        */
        public void ResetStreak(string nameDevice) {
            PlayerPrefs.SetInt("HeroMult" + nameDevice, 1);
            PlayerPrefs.SetInt("HeroStreak" + nameDevice, 0);
        }

        /**
        * Indique que le jeu est terminé
        */
        public void EndGame() {
            if(isNotCloseGame) {
                isNotCloseGame = false;
                LinkedList<CommunicationDeviceBLS> ldb = FinderDevicesBLS.Instance.GetListDevicesBLS();
                foreach (var device in ldb){
                    device.StopGame();
                    if(PlayerPrefs.GetInt("HeroScore" + device.surnameDevice) > PlayerPrefs.GetInt("HeroHighscore" + device.surnameDevice));
                        PlayerPrefs.SetInt("HeroHighscore" + device.surnameDevice, PlayerPrefs.GetInt("HeroScore" + device.surnameDevice));
                } 
                GameObject.Find("LoaderScene").GetComponent<LoaderScene>().LoadLevelScore(2);
            }
        }

        /**
        * Ajoute un score au score actuel du joueur
        * @param    nameDevice      Le nom du device
        * @param    typePrecision   precision du hit du joueur. (1 Parfait; 2 Excellent; 3 Bon; 4 Correct)
        */
        public void AddScore(string nameDevice, int typePrecision) {
            int score = 0;
            switch (typePrecision) {
                case 1:
                    score = 200;
                    break;
                case 2:
                    score = 150;
                    break;
                case 3:
                    score = 100;
                    break;
                case 4:
                    score = 75;
                    break;
                default:
                    score = 0;
                    break;
            }
            score *= PlayerPrefs.GetInt("HeroMult" + nameDevice);
            PlayerPrefs.SetInt("HeroScore" + nameDevice, PlayerPrefs.GetInt("HeroScore"+ nameDevice) + score);
        }
    }
}