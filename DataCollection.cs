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
    private string accessToken = "sl.u.AF_LfOOjob2-ntHISk9VB4hcTMyZ1svEM7vrDXr3RMKt6aIaRwNi8WaWWx-3ZGXgv61Fklpp904VityOXXNDzxnJWiu2q2GFWyTOVznvZ00G7kP-G2w2YoDUSEqw8ypBt08eLEbhzUYRX8SEo-NEAxKIQnxboPMbhQ_Q4YZ8noqEw1brBtv-e8gz2LG241C6W3c-8ylm8WMOv5GXTML0OnY0q2lRxHa1eyCg6LHJegzHBgZBraIuRi0fhPHdw_SipujjvEdKX3-atF5emu5-ca56kNX1Z_EN_tVMYnVBYAWxtrpdxgR_vGAAfOD_taemSrujDc6B4oq3mc3sZQqBALHO0ayIlNjIG6R7dg6rJiqF9Crf10v-K4wdyXhVn_1WEnKo0KUIynfOT3sAynrxKYx0X7keImfjjA8fYj-Tal_cYQXd4GlfH9GY4bQOAUPsJjMT-1W4DfM2vkkOgudFloaYzrauUx-bIJNXTK9_0StJrILM7yIsIPeNGIcQokudm3NaMkP5TcwcaR8mNGApc2iIwpJ7D9ypuJMgmcu43_-xT44RHLLpEGFq6YB4k5bp8OSIFX_lFgmmKwvoe4dFp8L_9ez0z_fjDxSeQaxh-R21dgmr_EkCbY4Z_fc9Sd_q514xvu1NE7bOqS9Z4jSOjJJ61EC--nA3Vs8emhu4FgggzCIqZhcS6e4fvykxQnYtulGJeki4hMrbyL8ZoR3Q-HAY8U7-_OnxBwzw3twZzKTlpm7aC9JJ7pu8lbIeXaipOyU-YzDql7WCdq0EB8pMPyf6s9tTeN2P6RzNgEgUppd8wvBeLo8dS7wUr2KGcpbHJgwt0ax07VEQj3j4fHlodEzfqCnz6G2uC1QwghIMi2fbUQxHlRfF3l8nfchF_SCfDv7fYzmA7hUlKlq2zj2v4mSPJwFc1vqKKKXnZ_i5e7HnCMTnqks4tKr0FQNBx92A1_VhKBxx713wSmC9H3vaIIUDBw39wCZ8n0jed1cwwZttRHum0iriPt2QOpUFj_M1NBhLD3ql84E6Kww7edpsTf1wZhqBLMKDrk4IOOl6Ak5sGw5_KvfCyxxlJP-fAVBTY_D1YP0IQMFXV48tgHat8VLKvBWjnZyByPieduv9zvr4uFaHryvxmQgGyRFFIsyoOcd573CkjGgWczxgiFCwsg-j_oGfGbhfKhIk9skVL2QRJ8J6mXBn6UE-mhycES08GNvL8gPAn0YC6H7vgvLm8veaV8Zg1fg7zzrJ5Kb_IGNZgvjuMDZ97-XH6WGvZfHuCMM";
    private string targetPath = "/Participant_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
    void Start()
    {
        isWriting = false;
        barHasFallen = false;
        FingersLeftAreas = false;
        BarFalledTimePoint = 0.0f;
        DP_Rend = DataProcessedObj.GetComponent<MeshRenderer>();
        DP_Rend.enabled = false;
        path = Application.persistentDataPath + "/RT.csv";

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

        if (SM_Script.trial == 15)
        {
            float meanRT = GetMean(reactiontimeList);
            float stdDevRT = GetStandardDeviation(reactiontimeList);
            AvgText.text = "Avg: " + meanRT.ToString("F2");
            StdText.text = "Std: " + stdDevRT.ToString("F2");
            DataProcessedObj.SetActive(true);
            DP_Rend.enabled = true;

            if (!isWriting)
            {
                string filePath = Path.Combine(Application.persistentDataPath, "Participant.csv");
                SaveToCSV(filePath, trialList, reactiontimeList);
                StartCoroutine(WaitForFileAndUpload(filePath));
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
