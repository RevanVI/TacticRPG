using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPanel : MonoBehaviour
{
    public Text NameText;
    public Text ClassText;
    public Text LevelText;
    public Text HPText;
    public Text SpeedText;
    public Text MeleeDmgText;
    public Text RangeDmgText;
    public Text AmmoText;

    public Button ExitButton;

    private void Start()
    {
        ExitButton.onClick.AddListener(HidePanel);
    }

    public void ShowPanel(CharacterProperties characterProperties)
    {
        NameText.text = characterProperties.Name;
        ClassText.text = Character.GetStringClassName(characterProperties.Class);
        LevelText.text = $"Lvl. {characterProperties.Level}";
        HPText.text = $"{characterProperties.CurrentHealth} / {characterProperties.Health}";
        SpeedText.text = characterProperties.Speed.ToString();
        MeleeDmgText.text = characterProperties.MeleeDamage.ToString();
        RangeDmgText.text = characterProperties.RangedDamage.ToString();
        AmmoText.text = characterProperties.CurrentMissiles.ToString();

        gameObject.SetActive(true);
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
    }
}
