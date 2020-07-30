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
        private RaceCreatePlayer rcp;

        private bool    isCorrectTempo = true;
        private bool    isGoodTempo = false;
        private bool    isFinish = false;

        private const float   MIN_DELAY = 0.48f; // temps minimal d'attente d'une compression (125 par minute)
        private const float   MAX_DELAY = 0.66f; // temps maximal d'attente d'une compression (95 par minute)
        private const float   MIN_GOOD_DELAY = 0.52f; // temps minimal d'attente d'une bonne compression (115 par minute)
        private const float   MAX_GOOD_DELAY = 0.57f; // temps maximal d'attente d'une bonne compression (105 par minute)

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
            rcp = GameObject.Find("CreatePlayer").GetComponent<RaceCreatePlayer>();
        }

        private uint lastTime = 0;

        /**
        * boucle infini. retranscrit les données reçu par l'arduino en action de jeu.
        */
        void Update() {
            if(device != null && device.data.Count > 0 && !isFinish) {
                (byte value, uint time) = device.data.Dequeue();

                if(!rcp.isLoading)
                    StartCoroutine(Hit(value, time));  
            }
        }

        /**
        * Défini si le hit est bon ou mauvaise
        * @param    value  La precision du hit
        * @param    time   Le temps où a été enregistré le hit
        */
        IEnumerator Hit(byte value, uint time){
            if(lastTime == 0){
                rgl.GoodStreak(device.surnameDevice);
                lastTime = time;
                isCorrectTempo = true; 
                Moving(value); 
                yield break;
            }

            float intervalTime = (time - lastTime) / 1000.0f;
            Debug.Log(" delay between " + intervalTime);
            isCorrectTempo = intervalTime >= MIN_DELAY && intervalTime  <= MAX_DELAY;
            isGoodTempo = intervalTime >= MIN_GOOD_DELAY && intervalTime  <= MAX_GOOD_DELAY;
            lastTime = time;
            if(isCorrectTempo)
                rgl.GoodStreak(device.surnameDevice);
            else
                rgl.BadStreak(device.surnameDevice);

            Moving(value); 

            yield return new WaitForSeconds(0.1f);

            rb.velocity = new Vector2(speed, 0);
        }

        /**
        * Reinitialise les informations du joueur
        * @param    value  precision du hit du joueur. (1 Parfait; 2 Excellent; 3 Bon; 4 Correct)
        */
        private void Moving(byte value){
            float mult = rgl.GetMult(device.surnameDevice);
            float newSpeed = speed;
            switch(value) {
                case 1:
                    newSpeed += 1.8f * mult;
                    break;
                case 2:
                    newSpeed += 1.4f * mult;
                    break;
                case 3:
                    newSpeed += 1.0f * mult;
                    break;
                case 4:
                    newSpeed += 0.8f * mult;
                    break;
                default:
                    newSpeed += 0;
                    break;
            }
            newSpeed *= (isGoodTempo ? 2f : (isCorrectTempo ? 1.5f : 1f));
            Debug.Log("new Speed " + newSpeed);
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