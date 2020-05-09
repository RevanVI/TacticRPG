using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillPanel : MonoBehaviour
{
    private int _skillCounter;
    public List<SkillButton> SkillButtons;

    public void SetSkills(Character character)
    {
        _skillCounter = 0;
        //disable all skill buttons
        foreach (var skillButton in SkillButtons)
        {
            skillButton.gameObject.SetActive(false);
        }
        //set new skills
        for (int i = 0; i < character.Skills.Count; ++i)
        {
            SkillButtons[i].SetSkill(character.Skills[i]);
            ++_skillCounter;
        }
        //activate necessary button
        for (int i = 0; i < _skillCounter; ++i)
            SkillButtons[i].gameObject.SetActive(true);
    }

    void Start()
    {
        //SkillButtons = new List<SkillButton>();
        foreach (var skillButton in SkillButtons)
            skillButton.OnClick.AddListener(ProcessSkillButtonClick);
    }

    public void ProcessSkillButtonClick(int buttonNo)
    {
        Debug.Log($"Skill no {buttonNo} was activated");

        SkillButtons[buttonNo].Deactivate();
        GameController.Instance.SkillUsed(buttonNo);
    }
}
