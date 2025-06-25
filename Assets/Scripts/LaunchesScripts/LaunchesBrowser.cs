using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SpaceX
{
    public class LaunchesBrowser : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private ScrollRect scrollView;
        [SerializeField] private Transform contentContainer;
        [SerializeField] private LaunchListItem listItemPrefab;
        [SerializeField] private ShipDetailsPopup shipDetailsPopup;
        [SerializeField] private Button backButton;
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private TextMeshProUGUI errorText;

        [Header("Filtering")]
        [SerializeField] private TMP_Dropdown filterDropdown;
        [SerializeField] private TMP_InputField searchInput;

        [Header("Object Pool Settings")]
        [SerializeField] private int poolInitialSize = 20;

        private ObjectPool<LaunchListItem> itemPool;
        private List<Launch> allLaunches = new List<Launch>();
        private List<Launch> filteredLaunches = new List<Launch>();

        private Dictionary<string, Rocket> rocketCache = new Dictionary<string, Rocket>();

        private void Start()
        {
            //init pool object
            itemPool = new ObjectPool<LaunchListItem>(listItemPrefab, contentContainer, poolInitialSize);
            SetupUI();
            
            StartCoroutine(LoadLaunches());
        }

        private void SetupUI()
        {
            if (backButton != null)
                backButton.onClick.AddListener(BackToMainMenu);

            if (filterDropdown != null)
            {
                filterDropdown.onValueChanged.AddListener(_ => ApplyFilters());
                SetupFilterOptions();
            }

            if (searchInput != null)
                searchInput.onValueChanged.AddListener(_ => ApplyFilters());
        }

        private void SetupFilterOptions()
        {
            if (filterDropdown == null) return;

            filterDropdown.ClearOptions();
            filterDropdown.AddOptions(new List<string>
            {
                "All Launches",
                "Past Launches",
                "Upcoming Launches"
            });
        }

        private IEnumerator LoadLaunches()
        {
            ShowLoading(true);
            ShowError("");

            yield return SpaceXAPIManager.Instance.GetLaunches(
                launches =>
                {
                    allLaunches = launches.OrderByDescending(l => l.GetLaunchDate()).ToList();
                    StartCoroutine(PreloadRockets());
                },
                error =>
                {
                    ShowLoading(false);
                    ShowError($"Failed to load launches: {error}");
                }
            );
        }

        private IEnumerator PreloadRockets()
        {
            var uniqueRocketIds = allLaunches
                .Where(l => !string.IsNullOrEmpty(l.rocket))
                .Select(l => l.rocket)
                .Distinct()
                .ToList();

            Debug.Log($"Preloading {uniqueRocketIds.Count} unique rockets");

            //fetch all rockets
            int loaded = 0;
            foreach (var rocketId in uniqueRocketIds)
            {
                yield return SpaceXAPIManager.Instance.GetRocket(rocketId,
                    rocket =>
                    {
                        rocketCache[rocketId] = rocket;
                        loaded++;
                    },
                    error => Debug.LogWarning($"Failed to preload rocket {rocketId}")
                );
            }

            Debug.Log($"Preloaded {loaded} rockets");
            ShowLoading(false);
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            filteredLaunches.Clear();

            //start with all
            filteredLaunches.AddRange(allLaunches);

            //apply dropdown filter
            if (filterDropdown != null)
            {
                switch (filterDropdown.value)
                {
                    case 1: // Past
                        filteredLaunches = filteredLaunches.Where(l => !l.IsUpcoming()).ToList();
                        break;
                    case 2: // Upcoming
                        filteredLaunches = filteredLaunches.Where(l => l.IsUpcoming()).ToList();
                        break;
                }
            }

            //apply search filter
            if (searchInput != null && !string.IsNullOrEmpty(searchInput.text))
            {
                string search = searchInput.text.ToLower();
                filteredLaunches = filteredLaunches.Where(l =>
                    l.name.ToLower().Contains(search)
                ).ToList();
            }

            RefreshList();
        }

        private void RefreshList()
        {
            //all items to pool
            itemPool.ReturnAll();

            foreach (Launch launch in filteredLaunches)
            {
                LaunchListItem item = itemPool.Get();
                Rocket rocket = null;
                if (!string.IsNullOrEmpty(launch.rocket) && rocketCache.ContainsKey(launch.rocket))
                {
                    rocket = rocketCache[launch.rocket];
                }
                item.SetData(launch, rocket, OnLaunchClicked);
            }

            //Canvas.ForceUpdateCanvases(); I needed it earlier for debug
        }

        private void OnLaunchClicked(Launch launch)
        {
            if (shipDetailsPopup != null)
                shipDetailsPopup.Show(launch);
        }

        private void ShowLoading(bool show)
        {
            if (loadingPanel != null)
                loadingPanel.SetActive(show);
        }

        private void ShowError(string message)
        {
            if (errorText != null)
            {
                errorText.text = message;
                errorText.gameObject.SetActive(!string.IsNullOrEmpty(message));
            }
        }

        private void BackToMainMenu()
        {
            MainMenuController.ReturnToMainMenu();
        }

        private void OnDestroy()
        {
            allLaunches.Clear();
            filteredLaunches.Clear();
            rocketCache.Clear();

            if (itemPool != null)
                itemPool.ReturnAll();
        }

    }
}