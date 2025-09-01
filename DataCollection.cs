using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
//using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
public class DataCollection : MonoBehaviour
{
    StreamWriter writer;
    string path = "";
    public float TimeFirst;
    public float reactionTime; 
    SpawnManager SM_Script;
    private float timer;
    public bool savingData;
    public bool isWriting = true;
    TouchedBar indexFingerScript;
    TouchedBar thumbFingerScript;
    public bool barCaught;
    AudioSource myAS;
    public AudioClip myAC;
    public AudioClip EndSound;
    public TextMeshProUGUI ReactionTimeText;
    public List<float> reactiontimeList = new List<float>();
    public List<int> trialList = new List<int>();
    public GameObject SpawnManagerObj;
    public List<TextMeshProUGUI> RTsTexts;
    public bool barHasFallen;
    public TextMeshProUGUI BarHasFallenControl;
    public TextMeshProUGUI AvgText;
    public TextMeshProUGUI StdText;
    public float BarFalledTimePoint;
    public GameObject AreaIndex;
    public GameObject AreaThumb;
    FM_OBJ IndexScript;
    TH_OBJ ThumbScript;
    public float SensoryRT;
    public TextMeshProUGUI SensoryRTText;
    public bool FingersLeftAreas;
    public GameObject DataProcessedObj;
    MeshRenderer DP_Rend;
    public bool Calc_SRT;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //Dropbox
    private string dropboxUploadUrl = "https://content.dropboxapi.com/2/files/upload";
    private string accessToken = "sl.u.AF9wNMdmkl62UEZw40N9fEk2jrTDv9OuHAI06-er1t5dhk5r_fRJ82pUgjE4zVsVG20D97eFw3UVuFv7yG34Ejz0-p6gC2DJFAiU4jA8Z6ey7IIjHVC7X2RmKcyNutYrIvXCfCJ3cbQp-ANvrjnxZU9Sra8e8BkRMvwzb0WnESYpgnqp54d2qFXRvdwVydSxKZ4YM0W4M22E9xXRnnA6KYCNVWNMHOFRZGEjJa1uE_K-sCGgTBH94rJwNJ2aeVWdtrI-IherfEcJ2YcoRF3XfO4zS-MNUDrkttDiJHKk5duZBUsehLV3xMffdX-tiz09Kc2-IGNTnIBkdt3hCxBsl2P9UQihXgNYUkkJzQtg0Fq3qAre0xW1TfwTaqohMkXGKxehasiqgK6yol57ygCItSFmGROusrq8BCcznCbfOJlM3V6YOHYRcLIJ2z20UM9GtoMBsdTkBWmW0vD1uXYfWL42UQBgoXxzLB1bqiig933DFu7xreV5lTmzLEnxNyA41tgJN7EuwYmxe_u6ghGjk3ZWVQlhSVPz5O5lFwx0o6WMfn5UFy-F9bzVEF0nw_bQ6iPHupWm-6pitjkOpctiTvkQPUp6VkxyAMVNFhuIigUU8jpLR5F6Lw3zOCPM1HJ5sTNDLpgH6F9GWtD4xZIvvSs1DGZkVq28Hae5vJ_mqIYGZ0TCVT6wU4zWAcoWUQp8WfOOlF81N7FyjATchtyozm9Bdcwlfva1WcQBeZrhzANLDjgJAG3kEqYMHHJ0vx5kKl7YvOBzxXLX3N1PQMsMKAdq0IfnHdnD_p8bRUQ6cIG776QZfqJvHnjl-DP2qH6J0YuPAReXrk7rKB0vWmaEv0AQxUBFNcoVh8XZ8dT6Xhc2bp3Ap9n0rZFkhA2R_h6lYrLqK1g0mEfDUYE4JFlZnmLezPxsuR2YSZsT-W7DbDaCJvBXipFIwyZROuSgDPSgx14FDGY3whsIfdFWx0owqTcpm-H3dIN448R_eB_Ysr31drG7p_Pnc_7XFiZ3TRNEd0LGSn3IhZLlZwf_tP03DHDoZYf17jb-GxnoRLaxYe0hR7pXnxmhKFqVLTzpgCg6mtutCqOZqp5udiOGmGoKU4UVG4SWg-MV0P71uOyJX0OjOBIFaxzOKoU9bw1PEcLmiwBeeaCtsXyv2-O4y6mRjVQoJPBSoTVrQneJYfxqpRF-qpUPVQYNRi0XbCwrJ9RN-_Hm49WTMA6e1kekxFKyXxjl0gIi5EiMgBNYT-WMMii9FeQniMVt5f8YjFSM3e_dTy4";
    private string targetPath;
    void Start()
    {
        isWriting = false;
        Calc_SRT = false;
        barHasFallen = false;
        FingersLeftAreas = false;
        BarFalledTimePoint = 0.0f;
        DP_Rend = DataProcessedObj.GetComponent<MeshRenderer>();
        DP_Rend.enabled = false;
        string fileName = "Participant_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
        path = Path.Combine(Application.persistentDataPath, fileName);
        targetPath = "/Apps/Sesshomaru/" + fileName;

        var indexObj = GameObject.Find("IndexCol");
        if (indexObj != null)
        {
            indexFingerScript = indexObj.GetComponent<TouchedBar>();
            if (indexFingerScript == null)
                Debug.LogError("IndexCol hat kein TouchedBar-Component!");
        }
        else
        {
            Debug.LogError("GameObject IndexCol nicht gefunden!");
        }

        var thumbObj = GameObject.Find("ThumbCol");
        if (thumbObj != null)
        {
            thumbFingerScript = thumbObj.GetComponent<TouchedBar>();
            if (thumbFingerScript == null)
                Debug.LogError("ThumbCol hat kein TouchedBar-Component!");
        }
        else
        {
            Debug.LogError("GameObject ThumbCol nicht gefunden!");
        }

        myAS = GetComponent<AudioSource>();
        if (myAS == null)
        {
            Debug.LogError("AudioSource-Component fehlt auf diesem GameObject!");
        }

        SM_Script = SpawnManagerObj.GetComponent<SpawnManager>();
        IndexScript = AreaIndex.GetComponent<FM_OBJ>();
        ThumbScript = AreaThumb.GetComponent<TH_OBJ>();
    }
    void Update()
    {
        timer += Time.deltaTime;
        BarHasFallenControl.text = "FLA: " + FingersLeftAreas.ToString(); // Controlling Finger Movement 
        SensoryRTText.text = "Sensory RT: " + SensoryRT.ToString("F4"); // Time Control

        if (indexFingerScript != null && thumbFingerScript != null)
        {
            if (indexFingerScript.barTouched && thumbFingerScript.barTouched && barHasFallen) // Test
            {
                barCaught = true;
                myAS.PlayOneShot(myAC, 1f);
                reactionTime = Time.time - TimeFirst;
                reactiontimeList.Add(reactionTime);
                ReactionTimeText.text = "Reaction Time: " + reactionTime.ToString("F2") + " s";
                if (FingersLeftAreas && !Calc_SRT)
                {
                    SensoryRT = Time.time - BarFalledTimePoint;
                    Calc_SRT = true;
                }
                //SensoryRTText.text = "Sensory RT: " + BarFalledTimePoint.ToString("F4");
                RTsTexts[SM_Script.trial - 1].text = SM_Script.trial.ToString("F0") + ". "+ reactionTime.ToString("F2") + " s";
                indexFingerScript.barTouched = false;
                thumbFingerScript.barTouched = false;
                //barHasFallen = false;
            }
        }

        if (SM_Script.trial == 5)
        {
            float meanRT = GetMean(reactiontimeList);
            float stdDevRT = GetStandardDeviation(reactiontimeList);
            AvgText.text = "Avg: " + meanRT.ToString("F2");
            StdText.text = "Std: " + stdDevRT.ToString("F2");
            DataProcessedObj.SetActive(true);
            DP_Rend.enabled = true;

            if (!isWriting)
            {
                //string filePath = Path.Combine(Application.persistentDataPath, "Participant.csv");
                SaveToCSV(path, trialList, reactiontimeList); //SaveToCSV(filePath, trialList, reactiontimeList);
                StartCoroutine(WaitForFileAndUpload(path)); //StartCoroutine(WaitForFileAndUpload(filePath));
                myAS.PlayOneShot(EndSound, 1f);
                isWriting = true;
            }
        }

        if (!IndexScript.IndexInArea && !ThumbScript.ThumbInArea)
        {
            FingersLeftAreas = true;
        }
        else
        {
            FingersLeftAreas = false;
        }
    }
    void SaveToCSV(string fullPath, List<int> Trial, List<float> RTs)
    {
        using (StreamWriter writer = new StreamWriter(fullPath))
        {
            writer.WriteLine("Trial;RT");
            for (int i = 0; i < Trial.Count; i++)
            {
                writer.WriteLine($"{Trial[i]};{RTs[i]}");
            }
        }
    }
    float GetMean(List<float> values)
    {
        var validValues = values.FindAll(v => v > 0f);
        if (validValues.Count == 0) return 0f;
        float sum = 0f;
        foreach (var v in validValues)
        {
            sum += v;
        }
        return sum / validValues.Count;
    }

    float GetStandardDeviation(List<float> values)
    {
        var validValues = values.FindAll(v => v > 0f);
        if (validValues.Count == 0) return 0f;
        float mean = GetMean(validValues);
        float sumSquares = 0f;
        foreach (var v in validValues)
        {
            sumSquares += (v - mean) * (v - mean);
        }
        return Mathf.Sqrt(sumSquares / validValues.Count);
    }
    public IEnumerator UploadToDropbox(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);

        UnityWebRequest www = new UnityWebRequest(dropboxUploadUrl, "POST");
        www.uploadHandler = new UploadHandlerRaw(fileData);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);
        www.SetRequestHeader("Content-Type", "application/octet-stream");
        www.SetRequestHeader("Dropbox-API-Arg", "{\"path\":\"" + targetPath + "\",\"mode\":\"overwrite\",\"autorename\":true,\"mute\":false}");
        www.disposeUploadHandlerOnDispose = true;

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            Debug.LogError("Upload Error: " + www.error);
        else
            Debug.Log("Upload Success: " + www.downloadHandler.text);
    }
    private IEnumerator WaitForFileAndUpload(string filePath)
    {
        while (!File.Exists(filePath))
        {
            yield return null;
        }

        yield return UploadToDropbox(filePath);
    }
}
