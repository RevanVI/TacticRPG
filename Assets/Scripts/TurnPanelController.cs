using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnPanelController : MonoBehaviour
{
    public GameObject TurnIconPrefab;
    public GameObject TurnIconsHandler;

    public List<TurnIcon> TurnIcons;
    public TurnIcon CurrentTurnIcon;
    // Start is called before the first frame update
    void Start()
    {
        TurnIcons = new List<TurnIcon>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddIcon(CharacterProperties properties)
    {
        GameObject gameObject = Instantiate(TurnIconPrefab, TurnIconsHandler.transform);
        TurnIcon turnIcon = gameObject.GetComponent<TurnIcon>();
        turnIcon.SetCharacter(properties);
        TurnIcons.Add(turnIcon);
    }

    public void OnTurnStart()
    {
        //transform.

        CharacterProperties characterProperties = GameController.Instance.GetCurrentCharacterProperties();
        CurrentTurnIcon.SetCharacter(characterProperties);
    }
}
