using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.UI;

public class YoloSentisTester : MonoBehaviour
{
    [Header("Model")]
    public ModelAsset modelAsset;
    public RenderTexture cameraTexture;
    public TextAsset classesFile;

    [Header("UI")]
    public TMP_Text resultText;
    public RawImage previewImage;
    public RectTransform boxesRoot;

    [Header("Box UI")]
    public Color boxColor = Color.green;
    public float boxLineWidth = 3f;
    public int fontSize = 14;

    [Header("Yolo Settings")]
    public int inputSize = 640;
    public float confidenceThreshold = 0.5f;
    public float iouThreshold = 0.45f;
    public int maxDetections = 20;
    public bool runEveryFrame = false;
    public float interval = 0.25f;

    Worker worker;
    string[] classNames;
    Tensor<float> inputTensor;
    float timer;

    readonly List<GameObject> boxObjects = new();

    struct Detection
    {
        public int classId;
        public float confidence;
        public Rect box;
        public string label;
    }

    void Start()
    {
        if (modelAsset == null || cameraTexture == null || classesFile == null || resultText == null)
        {
            Debug.LogError("Bitte modelAsset, cameraTexture, classesFile und resultText im Inspector setzen.");
            enabled = false;
            return;
        }

        if (previewImage != null)
            previewImage.texture = cameraTexture;

        classNames = classesFile.text
            .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToArray();

        Model model = ModelLoader.Load(modelAsset);
        worker = new Worker(model, BackendType.GPUCompute);

        inputTensor = new Tensor<float>(new TensorShape(1, 3, inputSize, inputSize));

        resultText.text = "YOLO bereit.";
    }

    void Update()
    {
        if (!runEveryFrame)
        {
            if (Input.GetKeyDown(KeyCode.Space))
                RunDetection(cameraTexture);

            return;
        }

        timer += Time.deltaTime;

        if (timer >= interval)
        {
            timer = 0f;
            RunDetection(cameraTexture);
        }
    }

    public void RunDetection(Texture sourceTexture)
    {
        if (sourceTexture == null) return;

        var transform = new TextureTransform()
            .SetDimensions(inputSize, inputSize, 3)
            .SetTensorLayout(TensorLayout.NCHW);

        TextureConverter.ToTensor(sourceTexture, inputTensor, transform);

        worker.Schedule(inputTensor);

        using Tensor<float> output = (worker.PeekOutput() as Tensor<float>).ReadbackAndClone();

        List<Detection> detections = ParseYoloOutput(output);

        detections = NonMaxSuppression(detections, iouThreshold)
            .OrderByDescending(d => d.confidence)
            .Take(maxDetections)
            .ToList();

        ShowResult(detections);
        DrawBoxes(detections);
    }

    List<Detection> ParseYoloOutput(Tensor<float> output)
    {
        List<Detection> detections = new();

        TensorShape shape = output.shape;

        int dim1 = shape[1];
        int dim2 = shape[2];

        bool channelsFirst = dim1 < dim2;
        int attributes = channelsFirst ? dim1 : dim2;
        int boxes = channelsFirst ? dim2 : dim1;

        int classCount = attributes - 4;

        if (classCount <= 0)
        {
            resultText.text = $"Unerwarteter Output: {shape}";
            return detections;
        }

        for (int i = 0; i < boxes; i++)
        {
            float cx = GetOutput(output, channelsFirst, 0, i);
            float cy = GetOutput(output, channelsFirst, 1, i);
            float w = GetOutput(output, channelsFirst, 2, i);
            float h = GetOutput(output, channelsFirst, 3, i);

            int bestClass = -1;
            float bestScore = 0f;

            for (int c = 0; c < classCount; c++)
            {
                float score = GetOutput(output, channelsFirst, 4 + c, i);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestClass = c;
                }
            }

            if (bestScore < confidenceThreshold)
                continue;

            string label = bestClass >= 0 && bestClass < classNames.Length
                ? classNames[bestClass]
                : $"Class {bestClass}";

            Rect box = new Rect(
                cx - w / 2f,
                cy - h / 2f,
                w,
                h
            );

            detections.Add(new Detection
            {
                classId = bestClass,
                confidence = bestScore,
                box = box,
                label = label
            });
        }

        return detections;
    }

    float GetOutput(Tensor<float> tensor, bool channelsFirst, int attribute, int box)
    {
        if (channelsFirst)
            return tensor[0, attribute, box];

        return tensor[0, box, attribute];
    }

    void ShowResult(List<Detection> detections)
    {
        if (detections.Count == 0)
        {
            resultText.text = "Kein Objekt erkannt.";
            Debug.Log("YOLO: Kein Objekt erkannt.");
            return;
        }

        resultText.text = string.Join("\n", detections.Select(d =>
            $"{d.label}: {(d.confidence * 100f):0.0}%"
        ));

        foreach (Detection d in detections)
        {
            Debug.Log($"YOLO erkannt: {d.label} | Confidence: {(d.confidence * 100f):0.0}% | Box: {d.box}");
        }
    }

    void DrawBoxes(List<Detection> detections)
    {
        ClearBoxes();

        if (boxesRoot == null)
            return;

        RectTransform root = boxesRoot;
        float uiWidth = root.rect.width;
        float uiHeight = root.rect.height;

        //Debug.Log($"BoxesRoot Größe: {uiWidth} x {uiHeight}");

        foreach (Detection det in detections)
        {
            Rect b = det.box;

            bool normalized = b.xMax <= 1.5f && b.yMax <= 1.5f;

            float x = normalized ? b.x * uiWidth : b.x / inputSize * uiWidth;
            float y = normalized ? b.y * uiHeight : b.y / inputSize * uiHeight;
            float w = normalized ? b.width * uiWidth : b.width / inputSize * uiWidth;
            float h = normalized ? b.height * uiHeight : b.height / inputSize * uiHeight;

            float anchoredX = x - uiWidth / 2f + w / 2f;
            float anchoredY = uiHeight / 2f - y - h / 2f;

            GameObject box = new GameObject($"Box_{det.label}", typeof(RectTransform));
            box.transform.SetParent(root, false);

            RectTransform boxRt = box.GetComponent<RectTransform>();
            boxRt.anchorMin = new Vector2(0.5f, 0.5f);
            boxRt.anchorMax = new Vector2(0.5f, 0.5f);
            boxRt.pivot = new Vector2(0.5f, 0.5f);
            boxRt.anchoredPosition = new Vector2(anchoredX, anchoredY);
            boxRt.sizeDelta = new Vector2(w, h);

            CreateLine(boxRt, "Top", new Vector2(0.5f, 1f), new Vector2(0, -boxLineWidth / 2f), new Vector2(w, boxLineWidth));
            CreateLine(boxRt, "Bottom", new Vector2(0.5f, 0f), new Vector2(0, boxLineWidth / 2f), new Vector2(w, boxLineWidth));
            CreateLine(boxRt, "Left", new Vector2(0f, 0.5f), new Vector2(boxLineWidth / 2f, 0), new Vector2(boxLineWidth, h));
            CreateLine(boxRt, "Right", new Vector2(1f, 0.5f), new Vector2(-boxLineWidth / 2f, 0), new Vector2(boxLineWidth, h));

            //CreateLabel(boxRt, $"{det.label} {(det.confidence * 100f):0.0}%");

            boxObjects.Add(box);
        }
    }

    void CreateLine(RectTransform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
    {
        GameObject line = new GameObject(name, typeof(RectTransform), typeof(Image));
        line.transform.SetParent(parent, false);

        RectTransform rt = line.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Image img = line.GetComponent<Image>();
        img.color = boxColor;
    }

    void CreateLabel(RectTransform parent, string text)
    {
        GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObj.transform.SetParent(parent, false);

        RectTransform rt = labelObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(0, 10);
        rt.sizeDelta = new Vector2(250, 25);

        TextMeshProUGUI tmp = labelObj.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = boxColor;
        tmp.raycastTarget = false;
    }

    void ClearBoxes()
    {
        foreach (GameObject obj in boxObjects)
        {
            if (obj != null)
                Destroy(obj);
        }

        boxObjects.Clear();
    }

    List<Detection> NonMaxSuppression(List<Detection> detections, float iou)
    {
        List<Detection> result = new();

        foreach (Detection det in detections.OrderByDescending(d => d.confidence))
        {
            bool overlaps = result.Any(r =>
                r.classId == det.classId && IoU(r.box, det.box) > iou
            );

            if (!overlaps)
                result.Add(det);
        }

        return result;
    }

    float IoU(Rect a, Rect b)
    {
        float x1 = Mathf.Max(a.xMin, b.xMin);
        float y1 = Mathf.Max(a.yMin, b.yMin);
        float x2 = Mathf.Min(a.xMax, b.xMax);
        float y2 = Mathf.Min(a.yMax, b.yMax);

        float intersection = Mathf.Max(0, x2 - x1) * Mathf.Max(0, y2 - y1);
        float union = a.width * a.height + b.width * b.height - intersection;

        return union <= 0 ? 0 : intersection / union;
    }

    void OnDestroy()
    {
        ClearBoxes();
        inputTensor?.Dispose();
        worker?.Dispose();
    }
}