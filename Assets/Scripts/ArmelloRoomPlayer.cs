using System;
using Mirror;
using UnityEngine;

public class ArmelloRoomPlayer : NetworkRoomPlayer
{
    [SyncVar] public string nickname;
    [SyncVar] public Color playerColor;

    [SyncVar(hook = nameof(LoadCharacterSO))] private string characterSOName;

    private void LoadCharacterSO(string oldv, string newv)
    {
        characterSO = Resources.Load<CharacterScriptableObject>($"Characters/{newv}");
    }

    public CharacterScriptableObject characterSO;

    private PlayerCustomizationView customizationView;
    private CharacterSelectionView characterSelection;

    public override void OnStartLocalPlayer()
    {
        customizationView = FindObjectOfType<PlayerCustomizationView>();
        characterSelection = FindObjectOfType<CharacterSelectionView>();

        SaveData();
    }

    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
    {
        if (newReadyState == true)
        {
            SaveData();
        }
    }

    private void SaveData()
    {
        nickname = PlayerPrefs.GetString("Nickname");
        name = $"[Room Player] {nickname}";

        playerColor = customizationView.playerColor;
        characterSOName = characterSelection.GetSelectedCharacter().name;
    }
}
