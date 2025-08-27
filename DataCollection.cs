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
    private string accessToken = "sl.u.AF9JO5Gbx_skZq4vx7Q8qNp7baNXghrwZ2cOKgTKiGcbcQORNgl5a07x8TIkU6ng3qSuCeMBx_KKIhIXCo9MmTjMlfK8800kwThb9zWn1UDoqXbbe35w4TUuUKPKhaLcT1wyGIvTP75Ll9iHOmLVEGCtdgXhq8t14JAq4FnOZswXuXB8Ao40snn_hV8RdfkDmSDZJOgVJjQbV1Z0rX_Vc4fz5OCZIEu7goUpHaztBVZH8JnmLCPpW1TszVLEmgfGKql-KwDNTtS7I4ABM2ofDaViLq98lAtS1EtCFHV2yYgKQStKUtnmpwrcr9Hn4UPkCfIGFKJuojNo-Yo3jHSCg32uleXrKE3AgnJsP3zKDSgR-vHJ3f4-fGkb8vJI25XfVmOLyrIm60qT8gwuQSqzfHcE6nD1ozNafURsRjs_Dvwl4IGudXhL9XgMem6fpEbTBfN8q2TjhzCC8izcNRpFt9nHHeqtXghG3mFlnocGSD8RLW9vQCkOnrYquAs0UNkOwIQcePFfgMP7FlNNaqZV61hl6lpKylFfYO-1sKjei3flBHDxQtW3ZKy2KxLiexhqN18z1LUOGmJRPmQMbX5OEHF1wlMKsKThgHIxe5V176qqfdr3krlzjD1pxTx-EZ_sGVvBuwiPqZaIxXMm_vmBNEwwgh5lPPerUAdxA19cejT8XW4y7WoP96Qv5HiJH6kue4g8Q0-JyGZWbpdwXZ2g1R1Aa-NkoW1R4OucLRIkVLnY7OdYm51_cSWbKRNUrscgL_9FVSa4iryON4psJT4id3aTk5AY8CnWI56wDn4g2T31r8Nn0jkdN-bGcYSQGZnQcAMymT09srXmQHBK7jlrfFxpwfOTowP-6LXg-8OOJP2F5jrK6K30smr_ooiTEEi-cM9sLBbId9yBUUt1uAGxLcfHyW-71voeb_pP3_pRT_Qu_Ax9L9vANX2xujrruqQnUQxRv6DcITCLoYz5OJRrweQvXxvO7RImc5FOd3wZzYV1PO5tpv8SmGED8e5xBe3d4HxjnMtV5vNGldSCSP6cPq8SAZNLvz9_2DpgbqWXDeaDDIurzGI66a23liQHuF-UoQwCeUw4G75jH-8uf1H2AnvKEKsHQCa3hFEaF06S0tJKL1v976llzdNLY8FLtCx17MHrgsaHRGCHuZnPpCyo5EhJNcDYkMZo2nFPcJbYpfc5wpKllBw2Al-0SLpak9cr6YuMyOoaHJ4-ts7Ampv9Ek49IzPCMFa86Wi5izzYp7I1mg";
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
        path = Path.Combine(Application.persistentDataPath, fileName); // path = Application.persistentDataPath + "/RT.csv";
        targetPath = "/" + fileName;
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
