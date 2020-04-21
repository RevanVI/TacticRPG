using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnIcon : MonoBehaviour
{
    public Image CharacterImage;
    public Text HPText;
    public int ChainedCharacterBattleId;

    public TurnIcon()
    {
        ChainedCharacterBattleId = -1;
    }

    public TurnIcon(int characterBattleId, CharacterProperties properties)
    {
        SetCharacter(characterBattleId, properties);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCharacter(int characterBattleId, CharacterProperties properties)
    {
        ChainedCharacterBattleId = characterBattleId;
        CharacterImage.sprite = properties.Icon;
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
}
