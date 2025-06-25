using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SpaceX
{
    public class ShipDetailsPopup : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Transform shipListContainer;
        [SerializeField] private GameObject shipItemPrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject loadingIndicator;
        [SerializeField] private GameObject noShipsMessage;

        private List<GameObject> shipItems = new List<GameObject>();
        private Coroutine currentLoadCoroutine;

        private struct ShipItemComponents
        {
            public TextMeshProUGUI nameText;
            public TextMeshProUGUI typeText;
            public TextMeshProUGUI portText;
            public TextMeshProUGUI missionsText;
            public Button photoButton;
        }

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);

            if (popupPanel != null)
                popupPanel.SetActive(false);
        }

        public void Show(Launch launch)
        {
            if (currentLoadCoroutine != null)
            {
                StopCoroutine(currentLoadCoroutine);
                currentLoadCoroutine = null;
            }

            if (popupPanel != null)
                popupPanel.SetActive(true);

            if (titleText != null)
                titleText.text = $"{launch.name} - Ships";

            ClearShipItems();

            if (loadingIndicator != null)
                loadingIndicator.SetActive(true);
            if (noShipsMessage != null)
                noShipsMessage.SetActive(false);

            StartCoroutine(LoadShips(launch));
        }

        private IEnumerator LoadShips(Launch launch)
        {
            if (launch.ships == null || launch.ships.Count == 0)
            {
                if (loadingIndicator != null)
                    loadingIndicator.SetActive(false);
                if (noShipsMessage != null)
                    noShipsMessage.SetActive(true);
                yield break;
            }

            yield return SpaceXAPIManager.Instance.GetShips(launch.ships,
                ships =>
                {
                    if (loadingIndicator != null)
                        loadingIndicator.SetActive(false);

                    if (ships.Count == 0)
                    {
                        if (noShipsMessage != null)
                            noShipsMessage.SetActive(true);
                    }
                    else
                    {
                        foreach (Ship ship in ships)
                        {
                            CreateShipItem(ship);
                        }
                    }
                },
                error =>
                {
                    Debug.LogError($"Failed to load ships: {error}");
                    if (loadingIndicator != null)
                        loadingIndicator.SetActive(false);
                    if (noShipsMessage != null)
                        noShipsMessage.SetActive(true);
                }
            );
        }

        private void CreateShipItem(Ship ship)
        {
            if (shipItemPrefab == null || shipListContainer == null) return;

            GameObject item = Instantiate(shipItemPrefab, shipListContainer);
            shipItems.Add(item);

            ShipItemUI ui = item.GetComponent<ShipItemUI>();
            if (ui == null)
            {
                Debug.LogError("ShipItemUI component missing on ship item prefab!");
                return;
            }

            if (ui.nameText != null)
                ui.nameText.text = ship.name;
            if (ui.typeText != null)
                ui.typeText.text = $"Type: {ship.type}";
            if (ui.portText != null)
                ui.portText.text = $"Home Port: {ship.home_port}";
            if (ui.missionsText != null)
                ui.missionsText.text = $"Missions: {ship.GetMissionCount()}";

            if (ui.photoButton != null)
            {
                ui.photoButton.onClick.RemoveAllListeners();

                if (!string.IsNullOrEmpty(ship.image))
                {
                    string imageUrl = ship.image;
                    ui.photoButton.onClick.AddListener(() => OpenPhotoURL(imageUrl));
                    ui.photoButton.interactable = true;
                }
                else
                {
                    ui.photoButton.interactable = false;
                }
            }
        }

        private void OpenPhotoURL(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                Application.OpenURL(url);
            }
        }

        private void ClearShipItems()
        {
            foreach (GameObject item in shipItems)
            {
                if (item != null)
                {
                    Button btn = item.GetComponent<Button>();
                    if (btn != null)
                        btn.onClick.RemoveAllListeners();

                    Destroy(item);
                }
            }
            shipItems.Clear();
        }

        public void Close()
        {
            if (currentLoadCoroutine != null)
            {
                StopCoroutine(currentLoadCoroutine);
                currentLoadCoroutine = null;
            }

            ClearShipItems();

            if (popupPanel != null)
                popupPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (currentLoadCoroutine != null)
            {
                StopCoroutine(currentLoadCoroutine);
            }

            ClearShipItems();
        }
    }
}