using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterCardView : MonoBehaviour
{
    [SerializeField] public Button button;
    [SerializeField] Image portrait;
    [SerializeField] TextMeshProUGUI characterName;
    [SerializeField] TextMeshProUGUI characterStats;

    CharacterScriptableObject characterSO;

    public void SetScriptableObject(CharacterScriptableObject characterSO)
    {
        this.characterSO = characterSO;

        portrait.sprite = characterSO.characterSprite;
        characterName.text = characterSO.name;
        characterStats.text =
            characterSO.utilityStats.ToString()
            + "\n" +
            characterSO.combatStats.ToString();
    }

    public CharacterScriptableObject GetCharacterScriptableObject()
    {
        return characterSO;
    }

}
