using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class HoareMonitor 
{
    private static HoareMonitor instance = null;

    public class Condition
    {
        public Condition(){
            waitingSem = new Semaphore(0, 1);
            nbWaiting = 0;
        }

        public Semaphore waitingSem;
        public int nbWaiting;
    }

    private HoareMonitor(){
        monitorMutex = new Semaphore(1, 1);
        monitorSignaled = new Semaphore(0, 1);
        monitorNbSignaled = 0;
    }

    public static HoareMonitor Instance{
        get {
            if( instance == null)
                instance = new HoareMonitor();
            return instance;
        }
    }

    public void MonitorIn(){
        monitorMutex.WaitOne();
    }

    public void MonitorOut(){
        if(monitorNbSignaled > 0)
            monitorSignaled.Release();
        else
            monitorMutex.Release();
    }

    public void Wait(Condition cond){
        cond.nbWaiting += 1;
        if(monitorNbSignaled > 0)
            monitorSignaled.Release();
        else
            monitorMutex.Release();
        cond.waitingSem.WaitOne();
        cond.nbWaiting -= 1;
    }

    public void Signal(Condition cond){
        if(cond.nbWaiting > 0){
            monitorNbSignaled += 1;
            cond.waitingSem.Release();
            monitorSignaled.WaitOne();
            monitorNbSignaled -= 1;
        }
    }

    private Semaphore monitorMutex;
    private Semaphore monitorSignaled;
    private int monitorNbSignaled;
}