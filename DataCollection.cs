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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //Dropbox
    private string dropboxUploadUrl = "https://content.dropboxapi.com/2/files/upload";
    private string accessToken = "sl.u.AF_5LdOuezsO_uci-VSqM5nAhA_5fP3lo8SOHJk0qqZ8uOnB_NxgejufacDw3UWAayXbYWxPCoALsf2F9gWktDaiZOgB2ADZ0U-jbp0BguALECdzsdNEfAc6QdjawfausIdLe_2yRvbnRDaZx_SZlZITPdnl-UCd95IlVw8MXkFRSaxgzcfCTaA_QSo_aLQ7l1t20ia-v_JyCh-Yb4xTgiJISvMAIcBcEZtJIrQyvRCVrOJxEm5zUB2KBEM5bO-Z6anXJHHjXgzwPwy7O57kO47LrABgJ5AxqYn5cRemQA0S0LvRX8u-1Q5Cd30t6ozpbH2XCJ_9ePeYSElA0GGAbT0llD9KincUw_Hsqsg_aE5BQMqQTFRnFKmIDo5uhFUZTny6HcTinGbBdqZXOWuo1qBF2P8985aYU-3Q-NbVPe5lFVw4-P-2S5RECfSGa-j6EJtL9hft-c6vVRymgWZPAfLdH2waHlb-HhJzC_VF7oXlEzT7wpHGoB7gKgLc6exoJfpVMBDpEtoQRzCFdi5NbM8d39a3EpYzhVQvV5QT9lFgMaFGpqfz8k0QCyof7Ef5WUYUXclUqbt-JFA6QP0SB3aGtr6qcxuQIchkPcOQ3pCe47Z63w_SOS4C2XXQ0qVBiWPfvaxVaqfcqt_HOojozQTf3-pLSeC0U8FTIDsyhobFYwmr3nop7TU2VLPnTGgKgOAu-ZeKUrJ0hLhDWZXZ13za1TVdo0LZ4QuoXJ-5XCspd0wm1Mo7j4ykL67HECWcgIBD1UUvjaBcvzsgvuqOk3stH3Pruo77yP-22BumZX3yjMb-m3a8O8kD5AbMMx80GB1lQ01ENWhgm54iaJlJi7RGhpfC8sD1VN5JcWq5h6b357uyNw46BOOi8iZNL8GpNsp-z-NAmNWs2i_KalWpmzH2rihuBDk52jqyG8DWTslKcVbogOXtpzJAIidMkPiK_ZcvsDDQw6m2_VTGnyZvZZs_-YkElZrar9GL2S7HH4ZD4blrXCOmZwcWU9-cuMJrkZWyFMdOCBi4jRVQpm4_SVVQSYfOyJUeH5XjbV_ieZLP8-mvICb3eTrrT8aIx37wIafNt8u4BLFCb2zeOYBS6HGQmUdWL0ELeCua-Nj0_eJgN6i6XsZW9-m1Md7TfPEtmiXXCs9yU_vNIBCn3aEmFQNHr_Obqao72smtg3hxfpI9eLtFcuKF6b-HaXfI7pTrRmD0Tgk-QERxcZ9nGaHPdwu_QO8qM8LAk4aPZwzKgh_unXGEYhbTqvwN7rOkzqj4XxY";
    private string targetPath;
    void Start()
    {
        isWriting = false;
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
        BarHasFallenControl.text = "Bar falls: " + barHasFallen.ToString(); // Controlling bar 

        if (indexFingerScript != null && thumbFingerScript != null)
        {
            if (indexFingerScript.barTouched && thumbFingerScript.barTouched && barHasFallen) // Test
            {
                barCaught = true;
                myAS.PlayOneShot(myAC, 1f);
                reactionTime = Time.time - TimeFirst;
                reactiontimeList.Add(reactionTime);
                ReactionTimeText.text = "Reaction Time: " + reactionTime.ToString("F2") + " s";
                if (FingersLeftAreas)
                {
                    SensoryRT = Time.time - BarFalledTimePoint;
                }
                SensoryRTText.text = "Sensory RT: " + SensoryRT.ToString("F4");
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
