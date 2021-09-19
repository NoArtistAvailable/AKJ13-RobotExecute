using System.Collections;
using System.Collections.Generic;
using elZach.Common;
using elZach.Robots;
using UnityEngine;
using UnityEngine.Networking;

using Newtonsoft.Json;
using UnityEngine.Events;

public class NetSynch : MonoBehaviour
{
    public string serverURL = "https://akj13-robot-execute.glitch.me";
    public static GameManager.SerializablePlan[] onlinePlans;

    public UnityEvent<string> OnPingingSuccess, OnDownloadSuccess, OnUploadSuccess, OnConnectionError;
    
    public Button<NetSynch> pingButton = new Button<NetSynch>((x) => x.PingServer());
    public void PingServer() => StartCoroutine(PingServerRoutine());

    public Button<NetSynch> uploadButton = new Button<NetSynch>((x) => x.UploadCurrentPlan());
    public void UploadCurrentPlan()
    {
        StartCoroutine(SendRoutine(GameManager.Instance.Pack()));
    }

    public Button<NetSynch> downloadButton = new Button<NetSynch>((x) => x.DownloadPlans());
    public void DownloadPlans()
    {
        StartCoroutine(GetData());
    }

    private const double maxTime = 120;
    IEnumerator PingServerRoutine()
    {
        string url = serverURL + "/ping";
        //OnStartPinging.Invoke("Waking up Server");
        using (var req = UnityWebRequest.Get(url))
        {
            req.SendWebRequest();

            while (!req.isDone)
            {
                //await UniTask.Delay(250);
                yield return null;
                Debug.Log("[NetSynch] Waiting for Server response.");
            }
            Debug.Log(req.downloadHandler.text);
            if(req.result == UnityWebRequest.Result.Success)
                OnPingingSuccess.Invoke(req.downloadHandler.text);
            else OnConnectionError.Invoke(req.downloadHandler.text);
        }
    }
    
    public IEnumerator SendRoutine(GameManager.SerializablePlan target)
    {
        double timeStarted = Time.unscaledTime;
        WWWForm form = new WWWForm();
        string targetPlan = target.ToJSON();
        form.AddField("mon", targetPlan);
        
        Debug.Log(targetPlan);
        //OnStartUploading.Invoke();
        var req = UnityWebRequest.Post(serverURL + "/dreams/add", form);
        while (!req.isDone)
        {
            yield return req.SendWebRequest();
            double now = Time.unscaledTime;
            if (now - timeStarted > maxTime)
            {
                Debug.Log("Timeout");
                break;
            }
            Debug.Log("sending");
        }
        Debug.Log(req.result);
        if(req.result == UnityWebRequest.Result.Success)
            OnUploadSuccess.Invoke(req.downloadHandler.text);
        else OnConnectionError.Invoke(req.downloadHandler.text);
        //OnUploadFinished.Invoke();
    }
    
    IEnumerator GetData()
    {
        //NetSynch.Instance.OnDownloadStart.Invoke();
        string url = serverURL + "/dreams";
        using (var req = UnityWebRequest.Get(url))
        {
            req.SendWebRequest();

            while (!req.isDone)
            {
                Debug.Log("getting");
                yield return null;
                //Instance.OnDownloadProgress.Invoke(req.downloadProgress);
            }
            
            Debug.Log(req.downloadHandler.text);
            if (req.result != UnityWebRequest.Result.Success)
            {
                OnConnectionError.Invoke(req.downloadHandler.text);
            }
            else
            {
                onlinePlans = JsonConvert.DeserializeObject<GameManager.SerializablePlan[]>(req.downloadHandler.text);
                Debug.Log($"Downloaded {onlinePlans.Length} plans.");
                //NetSynch.Instance.OnDownloadFinished.Invoke(d.monList.Length.ToString("0000"));
                if (req.result == UnityWebRequest.Result.Success) OnDownloadSuccess.Invoke(req.downloadHandler.text);
            }
        }
            
    }
}
