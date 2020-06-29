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
        public Text objectTarget;
    }

    public class ConnectorDeviceBLS
    {
        private UnityBackgroundWorker dataReceiver;
        private DataCommunicationHelper dataReceiverHelper;
        private Boolean isRunning = false;
        public String nameDevice {get;}
        public String surnameDevice {get;}

        [DllImport("BTManagerLibrary")]
        private static extern bool BTM_IsConnected();

        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_ConnectToDevice(String data);

        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_ReceiveDataFast(String data);

        [DllImport("BTManagerLibrary")]
        private static extern IntPtr BTM_DisconnectFromDevice();

        public ConnectorDeviceBLS(String name, String surname){
            nameDevice = name;
            surnameDevice = surname;

            dataReceiverHelper = new DataCommunicationHelper();
        }

        public void Start(MonoBehaviour caller){
            dataReceiver = new UnityBackgroundWorker(caller, BGW_ReceiveData, BGW_ReceiveData_Progress, BGW_ReceiveData_Done, dataReceiverHelper);

            try {
                //Marshal.PtrToStringAnsi(BTM_DisconnectFromDevice());
                string status = Marshal.PtrToStringAnsi(BTM_ConnectToDevice(nameDevice));
                Debug.Log(status);
                if(status.Contains("Connected")){
                    isRunning = true;
                    dataReceiver.Run();
                }
                
            } catch (Exception e) {}
        }

        public void Stop() {
            Marshal.PtrToStringAnsi(BTM_DisconnectFromDevice());
            if(isRunning){
                //Marshal.PtrToStringAnsi(BTM_DisconnectFromDevice());
                dataReceiver.Abort();
            }
            isRunning = false;
        }

        public void SetObject(Text obj){
            //Debug.Log(obj);
            dataReceiverHelper.objectTarget = obj;
        }

        void BGW_ReceiveData(object CustomData, UnityBackgroundWorkerArguments e)
        {
            try {
                while (true && BTM_IsConnected()) {
                    DataCommunicationHelper temp = (DataCommunicationHelper)CustomData;
                    temp.receivedData = Marshal.PtrToStringAnsi(BTM_ReceiveDataFast(nameDevice));
                    e.Progress++;
                    Thread.Sleep(500);
                }
            }
            catch (Exception error) {
                e.HasError = true;
                e.ErrorMessage = error.Message;
            }
        }
        void BGW_ReceiveData_Progress(object CustomData, int Progress)
        {
            DataCommunicationHelper temp = (DataCommunicationHelper)CustomData;
            if(temp.objectTarget == null)
                Debug.Log(temp.receivedData);
            else
                temp.objectTarget.text = temp.receivedData;
        }
        void BGW_ReceiveData_Done(object CustomData, UnityBackgroundWorkerInformation Information)
        {
            if (Information.Status == UnityBackgroundWorkerStatus.Done)
                Debug.Log("Done Data");
            else if (Information.Status == UnityBackgroundWorkerStatus.Aborted)
                Debug.Log("Aborted");
            else if (Information.Status == UnityBackgroundWorkerStatus.HasError)
                Debug.Log(Information.ErrorMessage);
        }
    }
}