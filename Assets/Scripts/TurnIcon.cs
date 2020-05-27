using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class TurnIcon : MonoBehaviour
{
    public Image CharacterImage;
    public Text HPText;
    public int ChainedCharacterBattleId;
    public UnityIntEvent OnTurnIconClick;

    private void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(ResendButtonClick);

        
    }


    public TurnIcon()
    {
        ChainedCharacterBattleId = -1;
        OnTurnIconClick = new UnityIntEvent();
    }

    public TurnIcon(int characterBattleId, CharacterProperties properties)
    {
        SetCharacter(characterBattleId, properties);
    }

    public void SetCharacter(int characterBattleId, CharacterProperties properties)
    {
        ChainedCharacterBattleId = characterBattleId;
        CharacterImage.sprite = properties.Icon;
        CharacterImage.color = properties.IconColor;
        HPText.text = $"HP: {properties.CurrentHealth} / {properties.Health}";
    } 

    public void UpdateIcon(CharacterProperties properties)
    {
        SetHP(properties.CurrentHealth, properties.Health);
    }

    public void SetHP(int currentHP, int maxHP)
    {
        HPText.text = $"HP: {currentHP} / {maxHP}";
    }

    private void ResendButtonClick()
    {
        OnTurnIconClick.Invoke(ChainedCharacterBattleId);
    }
}
