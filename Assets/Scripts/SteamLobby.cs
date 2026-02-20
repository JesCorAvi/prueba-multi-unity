using UnityEngine;
using Mirror;
using Steamworks;

public class SteamLobby : MonoBehaviour
{
    [Header("UI Personalizada de Amigos")]
    public GameObject friendUIPrefab;       // El prefab que contiene el FriendUIElement
    public Transform friendsListContainer;  // El objeto 'Content' de un ScrollView donde se agruparán los amigos

    // Referencias a los "eventos" de Steam
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;

    private const string HostAddressKey = "HostAddress";
    
    // ¡Aquí está la variable que faltaba! Guarda el ID de la sala actual.
    private CSteamID currentLobbyID; 

    private void Start()
    {
        // Si Steam no está abierto o inicializado, no hacemos nada
        if (!SteamManager.Initialized) { return; }

        // Inicializamos los callbacks para escuchar a Steam
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    /// <summary>
    /// Llama a esta función desde el botón de "Crear Partida" o "Host" de tu menú.
    /// </summary>
    public void HostLobby()
    {
        // Crea un lobby en Steam configurado para que solo los amigos puedan unirse
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, NetworkManager.singleton.maxConnections);
    }

    /// <summary>
    /// Llama a esta función desde tu botón de "Invitar Amigos" de la UI (Abre panel de Steam).
    /// </summary>
    public void OpenInviteOverlay()
    {
        // Comprobamos si estamos dentro de una sala válida
        if (currentLobbyID.IsValid())
        {
            SteamFriends.ActivateGameOverlayInviteDialog(currentLobbyID);
        }
        else
        {
            Debug.LogWarning("No estás en ninguna sala. Crea una partida primero.");
        }
    }

    /// <summary>
    /// Llama a esta función desde un botón "Ver Amigos" para rellenar tu UI personalizada
    /// </summary>
    public void PopulateFriendsListUI()
    {
        // Limpiamos la lista vieja
        foreach (Transform child in friendsListContainer)
        {
            Destroy(child.gameObject);
        }

        // Comprobamos cuántos amigos tienes en total
        int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);

        for (int i = 0; i < friendCount; i++)
        {
            CSteamID friendSteamID = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
            string friendName = SteamFriends.GetFriendPersonaName(friendSteamID);
            EPersonaState state = SteamFriends.GetFriendPersonaState(friendSteamID);

            // Solo mostramos a los amigos que NO estén desconectados
            if (state != EPersonaState.k_EPersonaStateOffline)
            {
                GameObject newFriendUI = Instantiate(friendUIPrefab, friendsListContainer);
                FriendUIElement uiScript = newFriendUI.GetComponent<FriendUIElement>();
                
                if (uiScript != null)
                {
                    uiScript.Setup(friendSteamID, friendName, this);
                }
            }
        }
    }

    /// <summary>
    /// Envía la invitación interna por código. La llama el script FriendUIElement.
    /// </summary>
    public void InviteSpecificFriend(CSteamID friendID)
    {
        if (currentLobbyID.IsValid())
        {
            SteamMatchmaking.InviteUserToLobby(currentLobbyID, friendID);
        }
        else
        {
            Debug.LogWarning("¡Debes crear una partida (Host) antes de poder invitar!");
        }
    }

    // --- Callbacks automáticos de Steam ---

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError("Fallo al crear el lobby en Steam.");
            return;
        }

        // Arrancamos el Host en Mirror
        NetworkManager.singleton.StartHost();

        // Guardamos el ID de la sala actual (Soluciona el error CS0103)
        currentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);

        // Guardamos el ID de Steam del Host en el Lobby para que los invitados sepan a quién conectarse
        SteamMatchmaking.SetLobbyData(currentLobbyID, HostAddressKey, SteamUser.GetSteamID().ToString());
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        // Un amigo nos invitó y le dimos a "Jugar". Le decimos a Steam que queremos entrar a esa sala.
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        // Si somos el Host, no hacemos nada (ya estamos conectados)
        if (NetworkServer.active) return;

        // Si somos el Cliente, leemos el ID de Steam del Host usando los datos del Lobby
        string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);

        // Le pasamos el ID a Mirror y nos conectamos
        NetworkManager.singleton.networkAddress = hostAddress;
        NetworkManager.singleton.StartClient();
    }
}