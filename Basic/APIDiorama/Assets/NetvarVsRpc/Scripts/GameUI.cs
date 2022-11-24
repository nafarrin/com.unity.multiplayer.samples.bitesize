using System.Collections;
using TMPro;
using UnityEngine;

namespace Unity.Netcode.Samples.APIDiorama
{
    public class GameUI : MonoBehaviour
    {
        [SerializeField] TMP_Text startupLabel;
        [SerializeField] TMP_Text controlsLabel;

        void OnEnable()
        {
            Refreshlabels(NetworkManager.Singleton && (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer));
            StartCoroutine(SubscribeToNetworkManagerEvents());
        }

        IEnumerator SubscribeToNetworkManagerEvents()
        {
            yield return new WaitUntil(() => NetworkManager.Singleton);
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
        }

        void OnDestroy()
        {
            if (NetworkManager.Singleton)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
                NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            }
        }

        void OnServerStarted()
        {
            OnNetworkedSessionStarted();
        }

        void OnClientConnectedCallback(ulong obj)
        {
            if (NetworkManager.Singleton && NetworkManager.Singleton.IsServer)
            {
                return; //we don't want to do actions twice when playing as a host
            }
            OnNetworkedSessionStarted();
        }

        void OnClientDisconnectCallback(ulong obj)
        {
            OnNetworkedSessionEnded();
        }

        void Refreshlabels(bool isConnected)
        {
            startupLabel.gameObject.SetActive(!isConnected);
            controlsLabel.gameObject.SetActive(isConnected);
        }

        void OnNetworkedSessionStarted()
        {
            Refreshlabels(true);
        }

        void OnNetworkedSessionEnded()
        {
            Refreshlabels(false);
        }
    }
}