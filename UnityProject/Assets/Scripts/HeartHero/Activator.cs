﻿/*
 * Auteurs :     Alexandre Monteiro Marques
 * Date :        29 Mai 2020
 *
 * Fichier :     Activator.cs
 * Description : Il permet réaliser les opérations sur la main d'un joueur
 */

using BlueConnect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeartHero {
    public class Activator : MonoBehaviour
    {
        // Definit dans l'editeur Unity
        public CommunicationDeviceBLS device; // l'arduino associé
        
        private GameObject      note;
        private HeroGameLogic   hgm;
        private Color           old; // coleur original de la main
        private SpriteRenderer  sr; // l'image de la main

        private bool active     = false; // Savoir si un collision est en cours
        private bool noteExist  = false;

        void Awake(){
            sr = GetComponent<SpriteRenderer>();
            hgm = GameObject.Find("GameLogic").GetComponent<HeroGameLogic>();
        }

        void Start() {
            old = sr.color;
        }

        /**
        * boucle infini. retranscrit les données reçu par l'arduino en action de jeu.
        */
        void Update() {
            if(device != null && device.data.Count > 0) {
                StartCoroutine(Pressed());
                int value = device.data.Dequeue();

                if(active) {
                    noteExist = false;
                    hgm.AddScore(device.surnameDevice, value);
                    hgm.AddStreak(device.surnameDevice);
                    Destroy(note);
                }
                else {
                    hgm.ResetStreak(device.surnameDevice);
                }
                
            }
            /*if(Input.GetKeyDown(KeyCode.F)) {
                StartCoroutine(Pressed());
                int value = 1;

                if(active) {
                    noteExist = false;
                    hgm.AddScore(device.surnameDevice, value);
                    hgm.AddStreak(device.surnameDevice);
                    Destroy(note);
                }
                else
                    hgm.ResetStreak(device.surnameDevice);
            }*/
        }

        /**
        * Evenement de l'entrée en colision de l'ambulance
        * @param    col  L'objet ayant collisioné avec l'ambulance
        */
        void OnTriggerEnter2D(Collider2D col){
            if(col.gameObject.tag=="Note"){ 
                noteExist = true;
                active = true;
                note=col.gameObject;
            }
        }

        /**
        * Evenement de la sortie de colision de l'ambulance
        * @param    col  L'objet ayant collisioné avec l'ambulance
        */
        void OnTriggerExit2D(Collider2D col){
            active = false;
            if(col.gameObject.tag=="Final"){
                hgm.EndGame();
            }
            if(noteExist){
                hgm.ResetStreak(device.surnameDevice);
                noteExist = false;
            }
        }

        /**
        * Change la couleur de la main afin d'indiquer que le joueur à effectuer un hit
        */
        IEnumerator Pressed(){
            sr.color = new Color(0,0,0);
            yield return new WaitForSeconds(0.05f);
            sr.color = old;
        }
    }
}
