using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoadsterSimulation : MonoBehaviour
{
    // Constants
    private const float REQUIRED_MODE_SPEED = 24f; //1x speed for required mode
    private const float OPTIONAL_MODE_SPEED = 240f; //10x speed for optional mode

    [Header("3D Objects")]
    [SerializeField] private GameObject sunObject;
    [SerializeField] private GameObject roadsterObject;
    [SerializeField] private LineRenderer orbitTrailRenderer;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI orbitalDataText;
    [SerializeField] private Button backButton;
    [SerializeField] private Button modeToggleButton;
    [SerializeField] private TextMeshProUGUI modeButtonText;

    [Header("Simulation Settings")]
    /*[SerializeField]*/ private float simulationSpeed = 24f; //24 hours per second
    [SerializeField] private int trailLength = 20;
    [SerializeField] private float orbitScale = 0.00009f;

    [Header("Optional Mode Settings")]
    [SerializeField] private float optionalModeSpeedMultiplier = 1f;
    [SerializeField] private float maxInterpolationSeconds = 5f;

    [Header("Data")]
    [SerializeField] private TextAsset csvFile;

    private List<OrbitalData> orbitalDataList = new List<OrbitalData>();
    private List<OrbitalData> extendedDataList = new List<OrbitalData>(); //data after 08.10.2019
    private int currentDataIndex = 0;
    private List<Vector3> trailPositions = new List<Vector3>();

    // Mode control
    private bool useOptionalMode = false;
    private float interpolationProgress = 0f;
    private float currentInterpolationDuration = 1f;

    private void Start()
    {
        LoadOrbitalData();
        SetupBackToMenu();
        InitializeTrailRenderer();
        UpdateModeButtonText();
        StartCoroutine(SimulationLoop());
    }

    private void LoadOrbitalData()
    {
        if (csvFile == null)
        {
            Debug.LogError("CSV file not assigned!");
            return;
        }

        string[] lines = csvFile.text.Split('\n');
        Debug.Log($"CSV file loaded with {lines.Length} lines");

        for (int i = 1; i < lines.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(lines[i]))
            {
                try
                {
                    //clean the line from any potential BOM or extra characters
                    string cleanLine = lines[i].Trim().Replace("\r", "");

                    OrbitalData data = new OrbitalData(cleanLine);

                    //filter data
                    if (data.dateUTC >= new System.DateTime(2018, 2, 7) &&
                        data.dateUTC <= new System.DateTime(2019, 10, 8))
                    {
                        orbitalDataList.Add(data);
                    }

                    //filter data for optional mode
                    else if (data.dateUTC > new System.DateTime(2019, 10, 8))
                    {
                        extendedDataList.Add(data);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Failed to parse line {i}: {e.Message}");
                    Debug.LogWarning($"Line content: '{lines[i]}'");
                }
            }
        }

        Debug.Log($"Loaded {orbitalDataList.Count} orbital data points");
        Debug.Log($"Loaded {extendedDataList.Count} orbital data points (for optional mode)");

        if (orbitalDataList.Count == 0 && extendedDataList.Count == 0)
        {
            Debug.LogError("No valid orbital data points were loaded");
        }

        //analyze optional mode data gaps
        if (extendedDataList.Count > 1)
        {
            float totalHours = 0f;
            int gapsOver24Hours = 0;
            for (int i = 0; i < extendedDataList.Count - 1; i++)
            {
                var gap = (float)(extendedDataList[i + 1].dateUTC - extendedDataList[i].dateUTC).TotalHours;
                totalHours += gap;
                if (gap > 24f) gapsOver24Hours++;
            }
            Debug.Log($"Optional mode: Average gap = {totalHours / (extendedDataList.Count - 1):F1} hours, {gapsOver24Hours} gaps > 24 hours");
        }
    }

    private void SetupBackToMenu()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(() => MainMenuController.ReturnToMainMenu());
        }
    }

    public void ToggleMode()
    {
        useOptionalMode = !useOptionalMode;
        currentDataIndex = 0;
        interpolationProgress = 0f;
        trailPositions.Clear();

        if (orbitTrailRenderer != null)
        {
            orbitTrailRenderer.positionCount = 0;
        }

        if (useOptionalMode)
        {
            simulationSpeed = OPTIONAL_MODE_SPEED;
        }
        else
        {
            simulationSpeed = REQUIRED_MODE_SPEED;
        }

        UpdateModeButtonText();
        UpdateInterpolationDuration();

        Debug.Log($"Switched to {(useOptionalMode ? "Optional" : "Required")} mode");
    }

    private void UpdateModeButtonText()
    {
        if (modeButtonText != null)
        {
            modeButtonText.text = useOptionalMode ? "Try Required Mode" : "Try Optional Mode";
        }
    }

    private void UpdateInterpolationDuration()
    {
        if (!useOptionalMode || extendedDataList.Count < 2 || currentDataIndex >= extendedDataList.Count - 1)
        {
            currentInterpolationDuration = 1f;
            return;
        }

        //calculate time gap
        var current = extendedDataList[currentDataIndex];
        var next = extendedDataList[currentDataIndex + 1];
        float hoursBetween = (float)(next.dateUTC - current.dateUTC).TotalHours;

        if (hoursBetween <= 24f)
        {
            currentInterpolationDuration = hoursBetween / simulationSpeed;
        }
        else
        {
            //for larger gaps
            float normalDuration = hoursBetween / (simulationSpeed * optionalModeSpeedMultiplier); //I set optionalModeSpeedMultiplier to 1 by default, simulationSpeed is increased in optional mode
            currentInterpolationDuration = Mathf.Min(normalDuration, maxInterpolationSeconds);
        }


        Debug.Log($"Gap: {hoursBetween:F1} hours, Interpolation duration: {currentInterpolationDuration:F2} seconds");

    }

    private void InitializeTrailRenderer()
    {
        if (orbitTrailRenderer != null)
        {
            orbitTrailRenderer.positionCount = 0;
            orbitTrailRenderer.widthMultiplier = 0.05f;
            orbitTrailRenderer.material = new Material(Shader.Find("Sprites/Default"));
            orbitTrailRenderer.startColor = Color.cyan;
            orbitTrailRenderer.endColor = new Color(0, 1, 1, 0.1f);
        }
    }

    private IEnumerator SimulationLoop()
    {
        while (true)
        {
            var dataList = useOptionalMode ? extendedDataList : orbitalDataList;

            if (orbitalDataList.Count == 0)
            {
                yield return null;
                continue;
            }

            if (useOptionalMode)
            {
                UpdateRoadsterPositionOptional();
            }
            else
            {
                UpdateRoadsterPositionRequired();

                yield return new WaitForSeconds(1f / simulationSpeed);

                currentDataIndex++;
                if (currentDataIndex >= dataList.Count)
                {
                    currentDataIndex = 0;
                    trailPositions.Clear();
                }
            }

            //for optional mode, for interpolation
            if (useOptionalMode)
            {
                yield return null;
            }
        }
    }

    private void UpdateRoadsterPositionRequired()
    {
        if (currentDataIndex >= orbitalDataList.Count || roadsterObject == null)
            return;

        OrbitalData currentData = orbitalDataList[currentDataIndex];

        Vector3Double positionKm = CustomOrbitalCalculator.CalculateOrbitalPosition(
            currentData.semiMajorAxisAU,
            currentData.eccentricity,
            currentData.inclinationDegrees,
            currentData.longitudeOfAscendingNodeDegrees,
            currentData.argumentOfPeriapsisDegrees,
            currentData.trueAnomalyDegrees
        );

        Vector3 unityPosition = ConvertToUnityPosition(positionKm);

        ApplyPositionToRoadster(unityPosition, currentData);
    }

    private void UpdateRoadsterPositionOptional()
    {
        if (extendedDataList.Count < 2 || roadsterObject == null)
            return;

        if (currentDataIndex >= extendedDataList.Count - 1)
        {
            currentDataIndex = 0;
            interpolationProgress = 0f;
            UpdateInterpolationDuration();
            trailPositions.Clear();
            return;
        }

        OrbitalData currentData = extendedDataList[currentDataIndex];
        OrbitalData nextData = extendedDataList[currentDataIndex + 1];

        //calculate position with interpolation
        Vector3Double positionKm = CustomOrbitalCalculator.InterpolatePosition(
            currentData, nextData, interpolationProgress
        );

        Vector3 unityPosition = ConvertToUnityPosition(positionKm);
        ApplyPositionToRoadster(unityPosition, currentData);

        interpolationProgress += Time.deltaTime / currentInterpolationDuration;

        if (interpolationProgress >= 1f)
        {
            currentDataIndex++;
            interpolationProgress = 0f;
            UpdateInterpolationDuration();
            
            if (currentDataIndex >= extendedDataList.Count - 1)
            {
                currentDataIndex = 0;
                trailPositions.Clear();
            }
        }
    }


    private Vector3 ConvertToUnityPosition(Vector3Double positionKm)
    {
        return new Vector3(
            (float)(positionKm.x * orbitScale),
            (float)(positionKm.z * orbitScale),
            (float)(positionKm.y * orbitScale)
        );
    }

    private void ApplyPositionToRoadster(Vector3 unityPosition, OrbitalData data)
    {
        roadsterObject.transform.position = unityPosition;

        float distanceFromSun = unityPosition.magnitude;
        float scaleMultiplier = Mathf.Clamp(distanceFromSun / 10f, 0.5f, 3f);
        roadsterObject.transform.localScale = Vector3.one * 0.1f * scaleMultiplier;

        if (trailPositions.Count > 0)
        {
            Vector3 direction = unityPosition - trailPositions[trailPositions.Count - 1];
            if (direction.magnitude > 0.01f)
            {
                roadsterObject.transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        UpdateTrail(unityPosition);
        UpdateOrbitalDataDisplay(data, nextData: currentDataIndex < extendedDataList.Count - 1 ? extendedDataList[currentDataIndex + 1] : null);
    }

    private void UpdateTrail(Vector3 newPosition)
    {
        if (trailPositions.Count == 0 || Vector3.Distance(newPosition, trailPositions[trailPositions.Count - 1]) > 0.1f)
        {
            trailPositions.Add(newPosition);
        }

        if (trailPositions.Count > trailLength)
        {
            trailPositions.RemoveAt(0);
        }

        if (orbitTrailRenderer != null && trailPositions.Count > 1)
        {
            orbitTrailRenderer.positionCount = trailPositions.Count;
            orbitTrailRenderer.SetPositions(trailPositions.ToArray());
        }
    }

    private void UpdateOrbitalDataDisplay(OrbitalData data, OrbitalData nextData = null)
    {
        if (orbitalDataText != null)
        {
            string modeText = useOptionalMode ? "[OPTIONAL MODE]\n" : "[REQUIRED MODE]\n";
            string interpolationInfo = "";
            
            if (useOptionalMode && nextData != null)
            {
                System.TimeSpan timeDiff = nextData.dateUTC - data.dateUTC;
                interpolationInfo = $"\nNext: {nextData.GetLocalTime():yyyy-MM-dd}\n" +
                                  $"Gap: {timeDiff.TotalDays:F1} days\n" +
                                  $"Progress: {interpolationProgress:P0}";
            }
            
            orbitalDataText.text = modeText + data.GetFormattedDisplay() + interpolationInfo;
        }
    }

}