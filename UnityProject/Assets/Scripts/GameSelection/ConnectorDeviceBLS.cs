using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using HoV;

namespace blueConnect {
    class DataCommunicationHelper
    {
        public string receivedData;
    }

    public class ConnectorDeviceBLS
    {
        private UnityBackgroundWorker dataReceiver;
        private DataCommunicationHelper dataReceiverHelper;
        private Boolean isRunning = false;
        private HoareMonitor hm = HoareMonitor.Instance;

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
        private static extern IntPtr BTM_DisconnectFromDevice();

        public ConnectorDeviceBLS(String name, String surname){
            nameDevice = name;
            surnameDevice = surname;

            data = new Queue<int>();

            dataReceiverHelper = new DataCommunicationHelper();
        }

        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_GetDevicesNamesFast();

        public void Start(MonoBehaviour caller){
            Marshal.PtrToStringAnsi(BTM_GetDevicesNamesFast());
            dataReceiver = new UnityBackgroundWorker(caller, BGW_ReceiveData, BGW_ReceiveData_Progress, BGW_ReceiveData_Done, dataReceiverHelper);
            isRunning = true;
            try {
                hm.MonitorIn();
                string status;
                do{
                    status = Marshal.PtrToStringAnsi(BTM_ConnectToDevice(nameDevice));
                    Debug.Log("start " + status);
                } while(! status.Contains("Connected"));

                string validation;
                validation = Marshal.PtrToStringAnsi(BTM_ReceiveDataFast(nameDevice));
                Debug.Log(validation);
                if(validation.Contains("I am connected with " + FinderDevicesBLS.nameGame)){
                    do{
                        Marshal.PtrToStringAnsi(BTM_SendDataFast("Start Game"));
                        Thread.Sleep(1200);
                        validation = Marshal.PtrToStringAnsi(BTM_ReceiveDataFast(nameDevice));
                        Debug.Log(validation);
                    } while(!validation.Contains("V10"));
                    Marshal.PtrToStringAnsi(BTM_DisconnectFromDevice());      
                    dataReceiver.Run();
                }
                else {
                    isRunning = false;
                }
 
            } catch (Exception e) {
                Debug.Log(e.Message);
            } finally {
                hm.MonitorOut();
            }
        }

        public void Stop() {
            if(isRunning)
                isRunning = false;
        }

        private void SplitValueArduino(String values) {
            //Debug.Log(values);
            string[] digits = values.Split('V');
            foreach(string v in digits){
                if(v.StartsWith("01")){
                    //Debug.Log(v);
                    data.Enqueue(1);
                }
                if(v.StartsWith("02")){
                    //Debug.Log(v);
                    data.Enqueue(2);
                }
                if(v.StartsWith("03")){
                    //Debug.Log(v);
                    data.Enqueue(3);
                }
                if(v.StartsWith("04")){
                    //Debug.Log(v);
                    data.Enqueue(4);
                }
            }
        }

        private int counter = 0;

        void BGW_ReceiveData(object CustomData, UnityBackgroundWorkerArguments e)
        {
            while (isRunning) {
                counter++;
                hm.MonitorIn();
                try {
                    DataCommunicationHelper temp = (DataCommunicationHelper)CustomData;
                    Thread.Sleep(100);
                    string status = Marshal.PtrToStringAnsi(BTM_ConnectToDevice(nameDevice));
                    //Debug.Log("Inside " + status);
                    if(status.Contains("Connected")){
                        temp.receivedData = Marshal.PtrToStringAnsi(BTM_ReceiveDataFast(nameDevice));
                        Thread.Sleep(100);
                        e.Progress++;
                        Marshal.PtrToStringAnsi(BTM_DisconnectFromDevice());
                        
                    }
                    
                }
                catch (Exception error) {
                    e.HasError = true;
                    e.ErrorMessage = error.Message;
                }
                hm.MonitorOut();
            }

            bool endGame = true;

            hm.MonitorIn();     
            while(endGame){
                try {
                    Thread.Sleep(200);
                    string status = Marshal.PtrToStringAnsi(BTM_ConnectToDevice(nameDevice));
                    Debug.Log("End " + status);
                    if(status.Contains("Connected")){
                        string validation;
                        do{
                            Marshal.PtrToStringAnsi(BTM_SendDataFast("End Game"));
                            Thread.Sleep(1500);
                            validation = Marshal.PtrToStringAnsi(BTM_ReceiveDataFast(nameDevice));
                        } while(!validation.Contains("I am connected with " + FinderDevicesBLS.nameGame));
                        endGame = false;
                        Marshal.PtrToStringAnsi(BTM_DisconnectFromDevice());
                    }
                }
                catch (Exception error) {
                    Debug.Log(error.Message);
                }
            }
            hm.MonitorOut();
            Debug.Log("endThread");
        }
        void BGW_ReceiveData_Progress(object CustomData, int Progress)
        {
            DataCommunicationHelper temp = (DataCommunicationHelper)CustomData;
            SplitValueArduino(temp.receivedData);
        }
        void BGW_ReceiveData_Done(object CustomData, UnityBackgroundWorkerInformation Information) {
            
        }
    }
}