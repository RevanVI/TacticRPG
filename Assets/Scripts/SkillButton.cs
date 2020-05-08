using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    public Button ButtomComponent;
    public Image ImageComponent;

    public void SetSkill(Skill skill)
    {
        ImageComponent.sprite = skill.SkillImage;

    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
