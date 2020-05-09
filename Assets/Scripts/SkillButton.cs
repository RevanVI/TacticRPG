using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SkillButton : MonoBehaviour
{
    public int Id;
    private Button _buttomComponent;
    private Image _imageComponent;
    public Text StatusText;
    public UnityIntEvent OnClick;
    public bool IsActive;

    void Awake()
    {
        _buttomComponent = GetComponent<Button>();
        _imageComponent = GetComponent<Image>();
        _buttomComponent.onClick.AddListener(ResendButtonClick);
        OnClick = new UnityIntEvent();
    }

    public void SetSkill(Skill skill)
    {
        _imageComponent.sprite = skill.SkillImage;

        if (skill.Cooldown != -1)
        {
            if (skill.CurrentCooldown > 0)
            {
                StatusText.text = skill.CurrentCooldown.ToString();
                Deactivate();
            }
            else
            {
                StatusText.text = "";
                Activate();
            }
        }
        else
        {
            StatusText.text = $"{skill.CurrentCount} / {skill.Count}";
            if (skill.CurrentCount > 0)
                Activate();
            else
                Deactivate();
        }
    }

    public void ResendButtonClick()
    {
        OnClick.Invoke(Id);
    }

    public void Activate()
    {
        _buttomComponent.interactable = true;
        IsActive = true;
    }

    public void Deactivate()
    {
        _buttomComponent.interactable = false;
        IsActive = false;
    }
}
