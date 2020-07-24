/*
 * Auteurs :     Alexandre Monteiro Marques
 * Date :        13 Juillet 2020
 *
 * Fichier :     Ambulance.cs
 * Description : Il permet réaliser les opérations sur l'ambualnce d'un joueur.
 */

using BlueConnect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RaceHeart {
    public class Ambulance : MonoBehaviour
    {
        // Definit dans l'editeur Unity
        public CommunicationDeviceBLS device; // l'arduino associé
        public float speed;

        private RaceGameLogic rgl;
        private Rigidbody2D   rb;

        private bool    isCorrectTempo = true;
        private bool    isFinish = false;
        private float   delay = 0.55f; // temps idéal entre chaque compression
        private float   timeCurrent = 0.0f; // Temps courant
        private float   badDelay = 1.1f; // temps maximal d'attente d'une compression

        /**
        * Awake est appelé pendant que le script est en cours de chargement
        */
        void Awake() {
            rgl = GameObject.Find("GameLogic").GetComponent<RaceGameLogic>();
            rb = GetComponent<Rigidbody2D>();
        }

        /**
        * Start est appelé avant la première actualisation de la frame
        */
        void Start() {
            rb.velocity = new Vector2(speed, 0);
        }

        /**
        * boucle infini. retranscrit les données reçu par l'arduino en action de jeu.
        */
        void Update() {
            if(device != null && device.data.Count > 0) {
                int value = device.data.Dequeue();

                if(isCorrectTempo) {
                    rgl.GoodStreak(device.surnameDevice);
                }
                else {
                    rgl.BadStreak(device.surnameDevice);
                }  

                Moving(value); 
                isCorrectTempo = false;   
            }
            if(timeCurrent + badDelay < Time.fixedTime && !isFinish) {
                isCorrectTempo = false;
                rb.velocity = new Vector2(speed, 0);
                rgl.ResetStreak(device.surnameDevice);
            }
            if(timeCurrent + delay < Time.fixedTime && !isFinish) {
                isCorrectTempo = true;
                rb.velocity = new Vector2(speed, 0);
                timeCurrent = Time.fixedTime; 
            }
        }

        /**
        * Reinitialise les informations du joueur
        * @param    value  precision du hit du joueur. (1 Parfait; 2 Excellent; 3 Bon; 4 Correct)
        */
        private void Moving(int value){
            int mult = rgl.GetMult(device.surnameDevice);
            float newSpeed = speed;
            switch(value) {
                case 1:
                    newSpeed += 1f * mult;
                    break;
                case 2:
                    newSpeed += 0.7f * mult;
                    break;
                case 3:
                    newSpeed += 0.5f * mult;
                    break;
                case 4:
                    newSpeed += 0.2f * mult;
                    break;
                default:
                    newSpeed += 0;
                    break;
            }
            newSpeed /= (isCorrectTempo ? 1 : 2);
            rb.velocity = new Vector2(newSpeed , 0);
        }

        /**
        * Evenement de la sortie de colision de l'ambulance
        * @param    col  L'objet ayant collisioné avec l'ambulance
        */
        void OnTriggerExit2D(Collider2D col) {
            if(col.gameObject.tag == "FinishLine" && !isFinish) {
                isFinish = true;
                rb.velocity = new Vector2(0, 0);
                rgl.RaceFinish(this.transform.parent.gameObject, device.surnameDevice);
            }
        }
    }
}