using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using HoV;

namespace BlueConnect {
    class DataCommunicationHelper
    {
        public string receivedData;
    }

    public class CommunicationDeviceBLS
    {
        private UnityBackgroundWorker dataReceiver;
        private DataCommunicationHelper dataReceiverHelper;
        private HoareMonitor hm = HoareMonitor.Instance;

        public bool isRunning {get; set;} = false;
        public bool isCollectData {get; set;} = false;

        public int idColor {get; set;}
        public Color colorPlayer {get; set;}
        public int idCharacter {get; set;}
        public Sprite characterPlayer {get; set;}
        public bool isPlayerDefined {get; set;} = false;
        public bool endGame {get;} = false;

        public String nameDevice {get;}
        public String surnameDevice {get;}
        public Queue<int> data {get; set;}

        [DllImport("BTManagerLibrary")]
        private static extern bool BTM_IsConnected();

        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_ConnectToDevice(String data);

        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_ReceiveDataFast(String data);

        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_SendDataFast(string data);

        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_SendAndReceiveDataFast(string data);

        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_DisconnectFromDevice();

        public CommunicationDeviceBLS(String name, String surname){
            nameDevice = name;
            surnameDevice = surname;

            data = new Queue<int>();

            dataReceiverHelper = new DataCommunicationHelper();
        }

        public void StartGame(MonoBehaviour caller){
            dataReceiver = new UnityBackgroundWorker(caller, BGW_ReceiveData, BGW_ReceiveData_Progress, BGW_ReceiveData_Done, dataReceiverHelper);
            isRunning = true;
            try {
                hm.MonitorIn();
                string status;
                do{
                    status = Marshal.PtrToStringAnsi(BTM_ConnectToDevice(nameDevice));
                } while(! status.Contains("Connected"));

                string validation;
                validation = Marshal.PtrToStringAnsi(BTM_ReceiveDataFast(nameDevice));
                if(validation.Contains("I am connected with " + FinderDevicesBLS.nameGame)){
                    do{
                        Marshal.PtrToStringAnsi(BTM_SendDataFast("Start Game"));
                        Thread.Sleep(1200);
                        validation = Marshal.PtrToStringAnsi(BTM_ReceiveDataFast(nameDevice));
                        Debug.Log(validation + validation.Length);
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

        public void StopGame() {
            if(isCollectData)
                isCollectData = false;
        }

        public void Deconnect(){
            try {
                hm.MonitorIn();
                string status;
                do{
                    status = Marshal.PtrToStringAnsi(BTM_ConnectToDevice(nameDevice));
                } while(! status.Contains("Connected"));

                string validation;
                validation = Marshal.PtrToStringAnsi(BTM_ReceiveDataFast(nameDevice));
                if(validation.Contains("I am connected with " + FinderDevicesBLS.nameGame)){
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

        private void SplitValueArduino(String values) {
            /*string[] digits = values.Split('V');
            foreach(string v in digits){
                if(v.StartsWith("01")){
                    data.Enqueue(1);
                }
                if(v.StartsWith("02")){
                    data.Enqueue(2);
                }
                if(v.StartsWith("03")){
                    data.Enqueue(3);
                }
                if(v.StartsWith("04")){
                    data.Enqueue(4);
                }
            }*/
            if(values.Contains("V01")){
                data.Enqueue(1);
            }
            if(values.Contains("V02")){
                data.Enqueue(2);
            }
            if(values.Contains("V03")){
                data.Enqueue(3);
            }
            if(values.Contains("V04")){
                data.Enqueue(4);
            }
        }

        void BGW_ReceiveData(object CustomData, UnityBackgroundWorkerArguments e)
        {
            while (isCollectData) {
                hm.MonitorIn();
                try {
                    DataCommunicationHelper temp = (DataCommunicationHelper)CustomData;
                    Thread.Sleep(60);
                    string status = Marshal.PtrToStringAnsi(BTM_ConnectToDevice(nameDevice));
                    Debug.Log("Inside " + status);
                    if(status.Contains("Connected")){
                        temp.receivedData = Marshal.PtrToStringAnsi(BTM_ReceiveDataFast(nameDevice));
                        SplitValueArduino(temp.receivedData);
                        //e.Progress++;
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
            while(isRunning){
                try {
                    Thread.Sleep(200);
                    string status = Marshal.PtrToStringAnsi(BTM_ConnectToDevice(nameDevice));
                    if(status.Contains("Connected")){
                        string validation;
                        do{
                            validation = Marshal.PtrToStringAnsi(BTM_SendAndReceiveDataFast("End Game"));
                            Thread.Sleep(1500);
                            //validation = Marshal.PtrToStringAnsi(BTM_ReceiveDataFast(nameDevice));
                            Debug.Log(validation);
                        } while(!validation.Contains("I am connected with " + FinderDevicesBLS.nameGame));
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
        void BGW_ReceiveData_Progress(object CustomData, int Progress) {
            //DataCommunicationHelper temp = (DataCommunicationHelper)CustomData;
            //SplitValueArduino(temp.receivedData);
        }
        void BGW_ReceiveData_Done(object CustomData, UnityBackgroundWorkerInformation Information) {}
    }
}