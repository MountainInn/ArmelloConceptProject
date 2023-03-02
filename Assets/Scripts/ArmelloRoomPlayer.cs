using Mirror;
using UnityEngine;

public class ArmelloRoomPlayer : NetworkRoomPlayer
{
    [SyncVar] public string nickname;

    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
    {
        if (newReadyState == true)
        {
            nickname = PlayerPrefs.GetString("Nickname");
        }
    }
}
