using UnityEngine;
using System.Collections;
using System.Threading;
using System.Collections.Generic;

/*
           !! WARNING !!
 YOU WILL NOT GET ANY ERROR MESSAGES FROM INSIDE THE THREAD.
 WHEN TESTING, ALWAYS ATTEMPT A MONOTHREAD RUN OF YOUR CODE.

 The new thread doesn't get write-access to anything in the Unity main thread.
 I don't know if the read is 'safe' either.
 
 However, pushing to a 'static' space works -- Monobehaviours can access this reservoir.

    */

static class ThreadRunner {
    static Dictionary<int, object> mThreadData = new Dictionary<int, object>();
    static Dictionary<int, ThreadedTask> mThreadStack = new Dictionary<int, ThreadedTask>();

    /*
        INTERESTING FUNCTIONS
    */
    public static void ExportData(object o)
    {
        int id = Thread.CurrentThread.ManagedThreadId;
        if (!mThreadData.ContainsKey(id))
        {
            Debug.Log("[THREADRUNNER] Did not find ID " + id + " for export -- are you exporting from main?");
            return;
        }

        mThreadData[id] = o;
        // Debug.Log("[THREADRUNNER] " + id + " has been exported.");
    }

    public static void MarkComplete()
    {
        int id = Thread.CurrentThread.ManagedThreadId;
        if (!mThreadStack.ContainsKey(id))
        {
            Debug.Log("[THREADRUNNER] Did not find ID " + id + " for export -- are you exporting from main?");
            return;
        }

        if (mThreadStack[id].ThreadStatus == ThreadStatus.Complete)
        {
            Debug.Log("[THREADRUNNER] Could not mark ID + " + id + ", as it is already completed."); // should never occur naturally.
        }


        // Debug.Log("[THREADRUNNER] " + id + " has been marked complete.");
        mThreadStack[id].MarkComplete();
    }
    /*
        END OF INTERESTING FUNCTIONS
    */

    public static void StartThread(int ThreadID)
    {
        if (!mThreadStack.ContainsKey(ThreadID))
        {
            Debug.Log("[THREADRUNNER] Did not find ID " + ThreadID + " in the stacks.");
            return;
        }
        mThreadStack[ThreadID].Execute();
    }

    public static void AbortThread (int ThreadID) {
        if (!mThreadStack.ContainsKey(ThreadID)) {
            Debug.Log("[THREADRUNNER] Did not find ID " + ThreadID + " in the stacks.");
            return;
        }
        mThreadStack[ThreadID].Abort();
        
        mThreadStack.Remove(ThreadID);
    }

    public static void AbortAllThreads() {
        Debug.Log("[THREADRUNNER] Shutting down all threads...");
        int n = 0;
        foreach (ThreadedTask t in mThreadStack.Values) {
            t.Abort();
            n++;
        }
        Debug.Log("[THREADRUNER] " + n + " threads closed.");
        mThreadStack.Clear();
    }

    public static void AddThread (ThreadedTask t) {
        // Debug.Log("[THREADRUNNER] Enrolled thread and prepared storage, for ID # " + t.ManagedThreadId);
        mThreadStack.Add(t.ManagedThreadId, t);
        mThreadData.Add(t.ManagedThreadId, null);
    }

    public static Thread GetThread (int ThreadID) {
        if (!mThreadStack.ContainsKey(ThreadID)) {
            Debug.Log("[THREADRUNNER] Did not find ID " + ThreadID + " in the stacks.");
            return null;
        }
        return mThreadStack[ThreadID].getThread();
    }

    public static bool isComplete (int ThreadID) {
        if (!mThreadStack.ContainsKey(ThreadID)) {
            Debug.Log("[THREADRUNNER] Did not find ID " + ThreadID + " in the stacks.");
            return false;
        }

        // Debug.Log("[THREADRUNNER] ID " + ThreadID + " state: " + mThreadStack[ThreadID].ThreadState);
        return mThreadStack[ThreadID].ThreadState == ThreadState.Stopped;
    }
    
    public static object FetchData (int ThreadID) {
        if (!mThreadData.ContainsKey(ThreadID)) {
            Debug.Log("[THREADRUNNER:FETCH] Did not find thread data: " + ThreadID);
            return null;
        }

        // Debug.Log("[THREADRUNNER:FETCH] Fetching data: " + ThreadID);
        return mThreadData[ThreadID];
    }

    public static bool isDataReady (int ThreadID) {
        if (!mThreadData.ContainsKey(ThreadID)) return false;
        if (!mThreadStack.ContainsKey(ThreadID)) return false;
        if (!mThreadStack[ThreadID].isDataReady()) return false;
        return true;
    }

    public static int CreateThread(ParameterizedThreadStart pThreadStart, object LoadData) {
        ThreadedTask newTask = new ThreadedTask(pThreadStart, LoadData);
        ThreadRunner.AddThread(newTask);
        return newTask.ManagedThreadId;
    }

    public static void ClearTask (int ThreadID) {
        AbortThread(ThreadID);
        mThreadStack.Remove(ThreadID);
    }

    public static void ClearData(int ThreadID) {
        mThreadData.Remove(ThreadID);
    }

    public static void FlushData () {
        mThreadData.Clear();
    }
    
}


public enum ThreadStatus {
    Unstarted,
    Running,
    Complete,
    Aborted
}

// new ThreadedTask( new ParameterizedThreadStart(TestWithObject), (object)s)
public class ThreadedTask {

    public static string s;
    object mMethod;
    object mParameters;

    Thread mThread;
    public Thread getThread () { return mThread; }
    public ThreadState ThreadState { get { return mThread.ThreadState; } }
    public int ManagedThreadId { get { return mThread.ManagedThreadId; } }


    ThreadStatus mStatus;
    public ThreadStatus ThreadStatus { get { return mStatus; } }

    object mReturnData;

    public int TaskID { get { return mThread.ManagedThreadId; } }

    public ThreadedTask (ParameterizedThreadStart Task, object o = null) {
        mParameters = o;
        mThread = new Thread(Task);
        mStatus = ThreadStatus.Unstarted;
    }

    public bool isDataReady () {

        if (mStatus == ThreadStatus.Complete) return true;
        return false;

    }

    public void MarkComplete() {
        mStatus = ThreadStatus.Complete;
    }

    public int Execute () {
        mThread.Start(mParameters);
        mStatus = ThreadStatus.Running;
        return mThread.ManagedThreadId;
    }

    public void Abort () {
        mThread.Abort();
        mStatus = ThreadStatus.Aborted;
    }

    public object GetParameters () {
        return mParameters;
    }
}
