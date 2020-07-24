/*
 * Auteurs :     Alexandre Monteiro Marques
 * Date :        2 Juillet 2020
 *
 * Fichier :     CheckDeviceBLSConnected.cs
 * Description : Permet de vérifier que les divices sont toujours connectés à l'application
 *               Basé sur le modèle Singleton
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
    public class CheckDeviceBLSConnected {
        // contient des informations lié à la communication bluetooth
        class DataCommunicationHelper {
            public string receivedData; // donnée reçu
            public CommunicationDeviceBLS device; // le device ayant recu les données
        }

        // Ces fonctions de librairie permettent de réaliser une communication Bluetooth
        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_ConnectToDevice(string data);
        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_ReceiveDataFast(string data);
        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_DisconnectFromDevice();

        private static CheckDeviceBLSConnected instance = null;

        private Boolean isRunning = false;
        private HoareMonitor hm = HoareMonitor.Instance;
        private FinderDevicesBLS fdb =  FinderDevicesBLS.Instance;
        private bool isNeedMove = false;
        private bool isSelect = false;

        private UnityBackgroundWorker dataReceiver;
        private DataCommunicationHelper dataReceiverHelper;

        /**
        * Constructeur privé
        */
        private CheckDeviceBLSConnected(){
            dataReceiverHelper = new DataCommunicationHelper();
        }

        /**
        * Permet de recupérer l'instance unique de cette objet. Singleton
        */
        public static CheckDeviceBLSConnected Instance {
            get {
                if( instance == null)
                    instance = new CheckDeviceBLSConnected();
                return instance;
            }
        }

        /**
        * Permet de commencer la vérification de communication entre l'application et les devices.
        * @param    caller  Le thread courant
        */
        public void Start(MonoBehaviour caller){
            dataReceiver = new UnityBackgroundWorker(caller, CheckDeviceBegin, CheckDeviceProgress, CheckDeviceDone, dataReceiverHelper);
            try {
                isRunning = true;
                dataReceiver.Run();                
            } catch (Exception e) {}
        }

        /**
        * Retourne vrai si la vérification est en cours
        */
        public bool IsRunning(){
            return isRunning;
        }

        /**
        * Retourne vrai si un joueur a demandé de changer de position du curseur
        */
        public bool GetIsNeedMove(){
            if(isNeedMove){
                isNeedMove = false;
                return true;
            }
            return false;
        }

        /**
        * Retourne vrai si un joueur a demandé à selectionner
        */
        public bool GetIsSelect(){
            if(isSelect){
                isSelect = false;
                return true;
            }
            return false;
        }

        /**
        * parcours la liste des devices et effectu un connexion en recuperant son contenu.
        * @param    CustomData  information lié au Bluetooth
        * @param    e           indique au thread qu'il y a eu une modification
        */
        void CheckDeviceBegin(object CustomData, UnityBackgroundWorkerArguments e)
        {
            LinkedList<CommunicationDeviceBLS> ldb = new LinkedList<CommunicationDeviceBLS>(fdb.GetListDevicesBLS());
            string data;
            hm.MonitorIn();
            foreach(var device in ldb){
                if(!device.isRunning){
                    try {
                        Thread.Sleep(50);
                        string status = Marshal.PtrToStringAnsi(BTM_ConnectToDevice(device.nameDevice));;
                        if(status.Contains("Connected")){
                            DataCommunicationHelper temp = (DataCommunicationHelper)CustomData;
                            // on va vider le buffer totalement du module bluetooth 
                            // BTM_ReceiveDataFast ne recupère que les 127 permiers charactères
                            data = "";
                            do{
                                data += Marshal.PtrToStringAnsi(BTM_ReceiveDataFast(device.nameDevice));
                            } while(data.Length % 127 == 0);

                            temp.receivedData = data; 
                            temp.device = device; 
                            e.Progress++;
                        Thread.Sleep(50);
                        Marshal.PtrToStringAnsi(BTM_DisconnectFromDevice());
                        }
                    }
                    catch (Exception error) {
                        e.HasError = true;
                        e.ErrorMessage = error.Message;
                        Debug.Log(error.Message);
                    }
                }
            }
            hm.MonitorOut();

        }

        /**
        * Supprime si l'appareil n'est pas connecté et sauvegarde les actions des joueurs.
        * @param    CustomData  information lié au Bluetooth
        * @param    e           indique au thread qu'il y a eu une modification
        */
        void CheckDeviceProgress(object CustomData, int Progress) {
            DataCommunicationHelper temp = (DataCommunicationHelper)CustomData;
            if(!temp.receivedData.Contains("I am connected with " + FinderDevicesBLS.nameGame)){
                Debug.Log("delete device");
                fdb.RemoveDevice(temp.device);
            }
            if(temp.receivedData.Contains("MOVE")){
                isNeedMove = true;
                Debug.Log("Move");
            }
            if(temp.receivedData.Contains("PRESS")) {
                isSelect = true;
                Debug.Log("PRESS");
            }
        }
        /**
        * Cloture la fin de la vérification
        */
        void CheckDeviceDone(object CustomData, UnityBackgroundWorkerInformation Information) {
            isRunning = false;
        }
    }
}