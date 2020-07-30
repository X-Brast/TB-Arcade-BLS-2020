/*
 * Auteurs :     Alexandre Monteiro Marques
 * Date :        29 Juin 2020
 *
 * Fichier :     CommunicationDeviceBLS.cs
 * Description : Permet de recupérer les données fourni par le device Arduino
 */

using HoV;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace BlueConnect {
    public class CommunicationDeviceBLS {

        // Ces fonctions de librairie permettent de réaliser une communication Bluetooth
        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_ConnectToDevice(string data);
        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_ReceiveDataFast(string data);
        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_SendDataFast(string data);
        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_SendAndReceiveDataFast(string data);
        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_DisconnectFromDevice();

        private UnityBackgroundWorker dataReceiver;
        private Mutex hm = Mutex.Instance;

        public int idColor {get; set;}
        public Color colorPlayer {get; set;}
        public int idCharacter {get; set;}
        public Sprite characterPlayer {get; set;}
        public string nameDevice {get;}
        public string surnameDevice {get;}
        public Queue<(byte, uint)> data {get; set;}

        public bool isRunning {get; set;} = false;
        public bool isCollectData {get; set;} = false;
        public bool isPlayerDefined {get; set;} = false;
        public bool endGame {get;} = false;

        /**
        * Constructeur
        * @param    name    nom du device
        * @param    surname surnom du device
        */
        public CommunicationDeviceBLS(string name, string surname){
            nameDevice = name;
            surnameDevice = surname;

            data = new Queue<(byte, uint)>();
        }

        /**
        * Initie la communication pour la recolte de donnée. Si la connexion echoue, le device est retiré du jeu
        * @param    caller  Le thread courant
        */
        public void StartGame(MonoBehaviour caller){
            dataReceiver = new UnityBackgroundWorker(caller, ReceiveDataBegin, ReceiveDataProgress, ReceiveDataDone, null);
            isRunning = true;
            try {
                hm.MonitorIn();
                string status;
                do{
                    status = Marshal.PtrToStringAnsi(BTM_ConnectToDevice(nameDevice));
                } while(! status.Contains("Connected"));

                string validation;
                validation = Marshal.PtrToStringAnsi(BTM_ReceiveDataFast(nameDevice));
                if(validation.Contains("I am connected with " + FinderDevicesBLS.NAME_GAME)){
                    do{
                        Marshal.PtrToStringAnsi(BTM_SendDataFast("Start Game"));
                        Thread.Sleep(1200);
                        validation = Marshal.PtrToStringAnsi(BTM_ReceiveDataFast(nameDevice));
                    } while(!validation.Contains("V10"));
                    Marshal.PtrToStringAnsi(BTM_DisconnectFromDevice());      
                    isCollectData = true;
                    dataReceiver.Run();
                }
                else {
                    isRunning = false;
                    FinderDevicesBLS.Instance.RemoveDevice(this);
                }
 
            } catch (Exception e) {
                Debug.Log(e.Message);
            } finally {
                hm.MonitorOut();
                
            }
        }

        /**
        * Permet d'arreter la recolte des données
        */
        public void StopGame() {
            if(isCollectData)
                isCollectData = false;
        }

        /**
        * Permet de dissocier le device à l'application. L'appareil Bluetooth ne sera plus connecté à cette application.
        */
        public void Deconnect(){
            try {
                hm.MonitorIn();
                string status;
                do{
                    status = Marshal.PtrToStringAnsi(BTM_ConnectToDevice(nameDevice));
                } while(! status.Contains("Connected"));

                string validation;
                validation = Marshal.PtrToStringAnsi(BTM_ReceiveDataFast(nameDevice));
                if(validation.Contains("I am connected with " + FinderDevicesBLS.NAME_GAME)){
                    do{
                        Marshal.PtrToStringAnsi(BTM_SendDataFast("End Connection"));
                        Thread.Sleep(1200);
                        validation = Marshal.PtrToStringAnsi(BTM_ReceiveDataFast(nameDevice));
                    } while(!validation.Contains("I am available"));
                    Marshal.PtrToStringAnsi(BTM_DisconnectFromDevice());      
                }
                else {
                    isRunning = false;
                    isCollectData = false;
                }
 
            } catch (Exception e) {
                Debug.Log(e.Message);
            } finally {
                hm.MonitorOut();
            }
        }

        private void SplitValueArduino(string values) {
            string[] digits = values.Split('V');
            foreach(string v in digits){
                if(v.StartsWith("01")){
                    data.Enqueue(((byte)1, UInt32.Parse(v.Substring(2))));
                }
                if(v.StartsWith("02")){
                    data.Enqueue(((byte)2, UInt32.Parse(v.Substring(2))));
                }
                if(v.StartsWith("03")){
                    data.Enqueue(((byte)3, UInt32.Parse(v.Substring(2))));
                }
                if(v.StartsWith("04")){
                    data.Enqueue(((byte)4, UInt32.Parse(v.Substring(2))));
                }
            }
        }

        /**
        * Initie la connexion, recoit les données et se deconnect tant que le jeu est en cours
        * Lorsque le jeu est terminé, stop la recolte de donnée.
        * @param    CustomData  information lié au Bluetooth
        * @param    e           indique au thread qu'il y a eu une modification
        */
        void ReceiveDataBegin(object CustomData, UnityBackgroundWorkerArguments e) {
            while (isCollectData) {
                hm.MonitorIn();
                try {
                    Thread.Sleep(60);
                    string status = Marshal.PtrToStringAnsi(BTM_ConnectToDevice(nameDevice));
                    Debug.Log("Inside " + status);
                    if(status.Contains("Connected")){
                        SplitValueArduino(Marshal.PtrToStringAnsi(BTM_ReceiveDataFast(nameDevice)));
                        e.Progress++;
                        Thread.Sleep(50);
                        Marshal.PtrToStringAnsi(BTM_DisconnectFromDevice());
                    }
                }
                catch (Exception error) {
                    e.HasError = true;
                    e.ErrorMessage = error.Message;
                }
                hm.MonitorOut();
            }
            hm.MonitorIn();     
            while(isRunning) {
                try {
                    Thread.Sleep(200);
                    string status = Marshal.PtrToStringAnsi(BTM_ConnectToDevice(nameDevice));
                    if(status.Contains("Connected")) {
                        string validation;
                        do {
                            validation = Marshal.PtrToStringAnsi(BTM_SendAndReceiveDataFast("End Game"));
                            Thread.Sleep(1500);
                        } while(!validation.Contains("I am connected with " + FinderDevicesBLS.NAME_GAME));
                        isRunning = false;
                        Marshal.PtrToStringAnsi(BTM_DisconnectFromDevice());
                    }
                }
                catch (Exception error) {
                    Debug.Log(error.Message);
                }
            }
            hm.MonitorOut();
        }
        /**
        * Ne fait rien
        */
        void ReceiveDataProgress(object CustomData, int Progress) {}
        /**
        * Vide les données
        */
        void ReceiveDataDone(object CustomData, UnityBackgroundWorkerInformation Information) {
            data.Clear();
        }
    }
}