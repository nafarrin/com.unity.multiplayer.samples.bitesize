using Unity.Netcode;
using UnityEngine;

namespace Unity.Template.Multiplayer.NGO.Runtime.ConnectionManagement
{
    class ServerListeningState : OnlineState
    {
        // used in ApprovalCheck. This is intended as a bit of light protection against DOS attacks that rely on sending silly big buffers of garbage.
        const int k_MaxConnectPayload = 1024;
        
        public override void Enter()
        {
            // todo setup gsh to receive matchmaker tickets
        }

        public override void Exit() { }
        
        public override void OnClientConnected(ulong clientId)
        {
            Debug.Log($"Client {clientId} connected to the server.");
        }

        public override void OnClientDisconnect(ulong clientId)
        {
            Debug.Log($"Client {clientId} disconnected from the server.");
        }

        public override void OnUserRequestedShutdown()
        {
            var reason = JsonUtility.ToJson(ConnectStatus.ServerEndedSession);
            for (var i = 0; i < ConnectionManager.NetworkManager.ConnectedClientsIds.Count; i++)
            {
                var id = ConnectionManager.NetworkManager.ConnectedClientsIds[i];

                ConnectionManager.NetworkManager.DisconnectClient(id, reason);
            }
            ConnectionManager.ChangeState(ConnectionManager.m_Offline);
        }

        public override void OnServerStopped()
        {
            ConnectionManager.ChangeState(ConnectionManager.m_Offline);
        }

        /// <summary>
        /// This logic plugs into the "ConnectionApprovalResponse" exposed by Netcode.NetworkManager. It is run every time a client connects to us.
        /// The complementary logic that runs when the client starts its connection can be found in ClientConnectingState.
        /// </summary>
        /// <remarks>
        /// Multiple things can be done here, some asynchronously. For example, it could authenticate your user against an auth service like UGS' auth service. It can
        /// also send custom messages to connecting users before they receive their connection result (this is useful to set status messages client side
        /// when connection is refused, for example).
        /// </remarks>
        /// <param name="request"> The initial request contains, among other things, binary data passed into StartClient. In our case, this is the client's GUID,
        /// which is a unique identifier for their install of the game that persists across app restarts.
        ///  <param name="response"> Our response to the approval process. In case of connection refusal with custom return message, we delay using the Pending field.
        public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            var connectionData = request.Payload;
            if (connectionData.Length > k_MaxConnectPayload)
            {
                // If connectionData too high, deny immediately to avoid wasting time on the server. This is intended as
                // a bit of light protection against DOS attacks that rely on sending silly big buffers of garbage.
                response.Approved = false;
                return;
            }

            var payload = System.Text.Encoding.UTF8.GetString(connectionData);
            var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload); // https://docs.unity3d.com/2020.2/Documentation/Manual/JSONSerialization.html
            var gameReturnStatus = GetConnectStatus(connectionPayload);

            if (gameReturnStatus == ConnectStatus.Success)
            {
                // connection approval will create a player object for you
                response.Approved = true;
                response.CreatePlayerObject = true;
                response.Position = Vector3.zero;
                response.Rotation = Quaternion.identity;
                return;
            }

            response.Approved = false;
            response.Reason = JsonUtility.ToJson(gameReturnStatus);
        }
        
        ConnectStatus GetConnectStatus(ConnectionPayload connectionPayload)
        {
            if (ConnectionManager.NetworkManager.ConnectedClientsIds.Count >= 10/*ConnectionManager.MaxConnectedPlayers*/)
            {
                return ConnectStatus.ServerFull;
            }

            if (connectionPayload.applicationVersion != Application.version)
            {
                return ConnectStatus.IncompatibleVersions;
            }

            return ConnectStatus.Success;
            //todo add support to deny connection if map or game version is different
        }
    }
}
