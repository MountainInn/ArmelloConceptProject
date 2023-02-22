using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterCardView : MonoBehaviour
{
    [SerializeField] public Button button;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] TextMeshProUGUI characterName;
    [SerializeField] TextMeshProUGUI characterStats;

    CharacterScriptableObject characterSO;

    public void SetScriptableObject(CharacterScriptableObject characterSO)
    {
        this.characterSO = characterSO;

        spriteRenderer.sprite = characterSO.characterSprite;
        characterName.text = characterSO.characterName;
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
