using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net.Sockets;

namespace SpaceX
{
    public class LaunchListItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI missionNameText;
        [SerializeField] private TextMeshProUGUI payloadCountText;
        [SerializeField] private TextMeshProUGUI rocketNameText;
        [SerializeField] private TextMeshProUGUI countryText;
        [SerializeField] private TextMeshProUGUI dateText;
        [SerializeField] private Image statusIcon;
        [SerializeField] private Button itemButton;

        [Header("Status Icons")]
        [SerializeField] private Sprite pastLaunchIcon;
        [SerializeField] private Sprite futureLaunchIcon;
        [SerializeField] private Color pastLaunchColor = Color.white;
        [SerializeField] private Color futureLaunchColor = Color.white;

        private Launch launchData;
        private Action<Launch> onClickCallback;

        private void Awake()
        {
            if (itemButton != null)
            {
                itemButton.onClick.AddListener(OnItemClicked);
            }
        }

        public void SetData(Launch launch, Rocket rocket, Action<Launch> onClick)
        {

            if (launch == null)
            {
                Debug.LogError("Launch data is null");
                return;
            }
            if (onClick == null)
            {
                Debug.LogError("OnClick callback is null");
                return;
            }

            launchData = launch;
            onClickCallback = onClick;

            if (missionNameText != null)
                missionNameText.text = launch.name;

            if (payloadCountText != null)
                payloadCountText.text = $"Payloads: {launch.payloads?.Count ?? 0}";

            if (dateText != null)
            {
                DateTime launchDate = launch.GetLaunchDate();
                dateText.text = launchDate.ToString("MMM dd, yyyy");
            }

            if (statusIcon != null)
            {
                bool isUpcoming = launch.IsUpcoming();
                statusIcon.sprite = isUpcoming ? futureLaunchIcon : pastLaunchIcon;
                statusIcon.color = isUpcoming ? futureLaunchColor : pastLaunchColor;
            }

            if (rocket != null)
            {
                if (rocketNameText != null)
                    rocketNameText.text = rocket.name;
                if (countryText != null)
                    countryText.text = rocket.country;
            }
            else
            {
                if (rocketNameText != null)
                    rocketNameText.text = "Unknown Rocket";
                if (countryText != null)
                    countryText.text = "";
            }

        }

        private void OnItemClicked()
        {
            Debug.Log($"Launch clicked: {launchData?.name}");
            onClickCallback?.Invoke(launchData);
        }

        //when item is returned to pool
        public void ResetItem()
        {
            launchData = null;
            onClickCallback = null;
        }
    }
}