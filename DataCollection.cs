using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEditor.Rendering;
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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //Dropbox
    private string dropboxUploadUrl = "https://content.dropboxapi.com/2/files/upload";
    private string accessToken = "sl.u.AF9PSPGPw0ZcELGdcyNQMHspF2w9C5w5U_IChdUaNVMeEonPI9AmN8dHlAZmLuqVefrITnVvufSyzo9fQen-ODGatNn_4x-3lQrc3TeqTksCEre-Ob_5a0V1C2lWYlbQ4vhoF4AEauSzn7b4kyMbtKRPyB8hz-Pb-L65XrTHrLtQ0XGq9h01ilsqtytgRYn6FOImBD77FjWHYl59gSax34LvjITzQZ_TGJvwNKc8gYnDcH9ao_y03R72tyT5dvcPWgJESbCEvPhnMuKGwfawE0tlr-SBYvpQ2jlB1eRamE_yzFRpIwhm7NR34uc6PBb98N_r3Ef-aEDjVDrA0vMTm7FcMUmZVlrEsCdhUqutEpT8fygn7zu7VI7t2fXWathDXqZWIa5aXHB5q2bHfVb4H2WFiBDXoBmkkfjlnJZluMJlXN72gmXQmEKFERgOiiKFTR-lUsfv18Ynd8IWGGPAYHtWz-n0Csuu0GDrwQmJG1FI-dexnIFTYSb_RO87Mh57L5OJuMQ8ZxDJq6VdiWEl9dBTT4scexOoOJCXeI5gVwIbw7-5jIc20TJzqfsO9iXJO3_BPnajoBEbp7vAfDjI4l3JT6BLmzFsn8mVFAKn1IOo8mMrOuY0SaqKc2Bx1aD9Db3UCznNofHYA62W9cZl7Ey3adK2APDyVwLR6h41kyozrHmgfv3S8TcrEhG42gMF4UaBa8l4r5Adg_zHFNrHi_wUbQJHJpXajwqrze30bpyM9jYX5QTwbiQ-OjwZrUvXVtR4TmeDFNvaz4kzZXSBHMMYGWFxnzJFfXnDsYjPQgRo4txkZ24eOQK3wzIDaf_onWGXqS6Ozejp4lvvcilGx9GEgbbSVuWdWmpyib8gEX3sBuwOYvBKYqvN9v18I0xrlBIDYk9i7UElMdkC5jrFrrjz0T14aB6zyXTNbOUgkuU2-z9XszSXmCzuXvot1BBwM199sZPMZ_KAt0kF-9UucaWdEBxc3cTr7_Iv1vLTUA6zZ4VEPTIwJ-Tjv5nCwHNgMzDTg1sHKrsAbwTSAkQJLqcpsGVz2Elbz31vDha_yFcTlWafqB16MYoe-zjxH3EKW40snvR4Djh5N8kZvsEZ8Mpu-sv6HvCQNI52fPS0m66qT0kw1VR13POWcr4SXOBZNal6r6rs_mRQrbd_7ievRIiRHoBbWhKG0cQuxqbjWCCCoUpTnqbVerWOZHvkKrGGkvzAkLq8KfF_scUj1gKF51MIPgyFog8kbiUhz1sOvqH2TQ";
    private string targetPath = "/data.json";
    void Start()
    {
        isWriting = false;
        barHasFallen = false;
        FingersLeftAreas = false;
        BarFalledTimePoint = 0.0f;
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

        if (!isWriting && SM_Script.trial == 15)
        {
            float meanRT = GetMean(reactiontimeList);
            float stdDevRT = GetStandardDeviation(reactiontimeList);
            AvgText.text = "Avg: " + meanRT.ToString("F2");
            StdText.text = "Std: " + stdDevRT.ToString("F2");
            SaveToCSV("Participant.csv", trialList, reactiontimeList);
            StartCoroutine(UploadToDropbox(Application.persistentDataPath + "/meinedaten.json"));
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
    void SaveToCSV(string filename, List<int> Trial, List<float> RTs)
    {
        using (StreamWriter writer = new StreamWriter(filename))
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
}
