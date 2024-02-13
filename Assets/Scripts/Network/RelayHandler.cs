using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Assets.Scripts.Network
{
    public class RelayHandler : MonoBehaviour
    {
        static RelayHandler s_Instance;
        public static RelayHandler Instance => s_Instance;

        // Use this for initialization
        void Start()
        {
            if (s_Instance == null)
                s_Instance = this;

            DontDestroyOnLoad(gameObject);
        }

        public async Task<string> CreateRelay()
        {
            Debug.Log("RelayHandler.CreateRelay()");

            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                Debug.Log("Created relay with code " + joinCode);

                RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

                NetworkManager.Singleton.StartHost();

                return joinCode;

            } catch (RelayServiceException e)
            {
                Debug.LogError(e.Message);
            }

            return "";
        }

        public async Task JoinRelay(string joinCode)
        {
            try
            {
                Debug.Log("Joining relay with code " + joinCode);   
                
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

                NetworkManager.Singleton.StartClient();

                Debug.Log("Client started with local id : " + NetworkManager.Singleton.LocalClientId);

            } catch (RelayServiceException e)
            {
                Debug.LogError(e.Message);
            }
        }   
    }
}