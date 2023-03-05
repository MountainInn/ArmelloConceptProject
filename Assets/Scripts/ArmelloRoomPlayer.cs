using System;
using Mirror;
using UnityEngine;

public class ArmelloRoomPlayer : NetworkRoomPlayer
{
    [SyncVar(hook = nameof(OnNicknameSync))] public string nickname;
    [SyncVar(hook = nameof(OnColorSync))] public Color playerColor;
    [SyncVar(hook = nameof(OnCharacterNameSync))] private string characterSOName;

    public CharacterScriptableObject characterSO;

    private PlayerCustomizationView customizationView;
    private CharacterSelectionView characterSelection;

    public override void OnStartLocalPlayer()
    {
        customizationView = FindObjectOfType<PlayerCustomizationView>();
        characterSelection = FindObjectOfType<CharacterSelectionView>();

        customizationView.onNameChanged += CmdSetNickname;
        customizationView.onColorChanged += CmdSetColor;
        characterSelection.onSelectedCharacterChanged += CmdSetSelectedCharacter;

        CmdSetNickname(PlayerPrefs.GetString("Nickname"));
        CmdSetColor(customizationView.playerColor);
        CmdSetSelectedCharacter(characterSelection.GetSelectedCharacter().name);
    }

    /// Nickname
    [Command(requiresAuthority = false)]
    public void CmdSetNickname(string nickname)
    {
        SetNickname(nickname);
    }

    private void OnNicknameSync(string oldv, string newv)
    {
        SetNickname(newv);
    }

    private void SetNickname(string nickname)
    {
        name = $"[Room Player] {nickname}";
        this.nickname = nickname;
    }

    /// Color
    [Command(requiresAuthority = false)]
    public void CmdSetColor(Color color)
    {
        SetColor(color);
    }
    private void OnColorSync(Color oldv, Color newv)
    {
        SetColor(newv);
    }
    private void SetColor(Color color)
    {
        this.playerColor = color;
    }

    /// Selected Character
    [Command(requiresAuthority = false)]
    public void CmdSetSelectedCharacter(string characterName)
    {
        SetSelectedCharacter(characterName);
    }
    private void OnCharacterNameSync(string oldv, string newv)
    {
        SetSelectedCharacter(newv);
    }
    private void SetSelectedCharacter(string characterName)
    {
        characterSO = Resources.Load<CharacterScriptableObject>($"Characters/{characterName}");
        characterSOName = characterName;
    }

}
