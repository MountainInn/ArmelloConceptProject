using System;
using Mirror;
using UnityEngine;

public class ArmelloRoomPlayer : NetworkRoomPlayer
{
    [SyncVar(hook = nameof(OnNicknameSync))] public string nickname;
    private void OnNicknameSync(string oldv, string newv)
    {
        RenameGameObject(newv);
    }

    [SyncVar] public Color playerColor;
    [SyncVar(hook = nameof(LoadCharacterSO))] private string characterSOName;

    public CharacterScriptableObject characterSO;

    private PlayerCustomizationView customizationView;
    private CharacterSelectionView characterSelection;

    public override void OnStartLocalPlayer()
    {
        customizationView = FindObjectOfType<PlayerCustomizationView>();
        characterSelection = FindObjectOfType<CharacterSelectionView>();

        customizationView.onNameChanged += SetName;
        customizationView.onColorChanged += SetColor;

        characterSelection.onSelectedCharacterChanged += SetSelectedCharacter;

        LoadName();
        SetColor(customizationView.playerColor);
        SetSelectedCharacter(characterSelection.GetSelectedCharacter());
    }

    private void SetSelectedCharacter(CharacterScriptableObject selectedCharacterSO)
    {
        characterSOName = selectedCharacterSO.name;
    }

    private void LoadCharacterSO(string oldv, string newv)
    {
        characterSO = Resources.Load<CharacterScriptableObject>($"Characters/{newv}");
    }

    private void SetColor(Color color)
    {
        playerColor = color;
    }

    private void LoadName()
    {
        nickname = PlayerPrefs.GetString("Nickname");
    }
    private void SetName(string playerName)
    {
        nickname = playerName;
        RenameGameObject(nickname);
    }

    private void RenameGameObject(string nickname)
    {
        name = $"[Room Player] {nickname}";
    }
}
