using UnityEngine;
using UnityEngine.UI;
using Steamworks;

public class FriendUIElement : MonoBehaviour
{
    public Text friendNameText; // Arrastra aquí el componente Text de tu prefab
    
    private CSteamID friendSteamID;
    private SteamLobby lobbyManager;

    // Configura este elemento cuando se crea en la lista
    public void Setup(CSteamID steamID, string friendName, SteamLobby manager)
    {
        friendSteamID = steamID;
        friendNameText.text = friendName;
        lobbyManager = manager;
    }

    // Llama a esta función desde el evento OnClick() del Botón de este prefab
    public void InviteThisFriend()
    {
        lobbyManager.InviteSpecificFriend(friendSteamID);
        Debug.Log("Invitación enviada a: " + friendNameText.text);
    }
}