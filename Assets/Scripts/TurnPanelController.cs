using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class TurnPanelController : MonoBehaviour
{
    public GameObject TurnIconPrefab;
    public GameObject TurnIconsHandler;

    public List<TurnIcon> TurnIcons;
    public TurnIcon CurrentTurnIcon;
    private Transform _lastTurnIcon;

    public Text RoundText;
    public Text TurnText;
    // Start is called before the first frame update
    void Start()
    {
        TurnIcons = new List<TurnIcon>();
        GameController.Instance.OnTurnStart.AddListener(OnTurnStart);


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddIcon(int characterBattleId, CharacterProperties properties)
    {
        GameObject gameObject = Instantiate(TurnIconPrefab, TurnIconsHandler.transform);
        TurnIcon turnIcon = gameObject.GetComponent<TurnIcon>();
        turnIcon.SetCharacter(characterBattleId, properties);
        TurnIcons.Add(turnIcon);
    }

    public void OnTurnStart()
    {
        //make visible last turn icon
        if (_lastTurnIcon != null)
        {
            _lastTurnIcon.gameObject.SetActive(true);
        }

        //get first icon in queue
        _lastTurnIcon = TurnIconsHandler.transform.GetChild(0);
        //make invisible
        _lastTurnIcon.gameObject.SetActive(false);
        //transfer to last position in queue
        _lastTurnIcon.SetAsLastSibling();

        //set character in current turn icon
        CharacterProperties characterProperties;
        int characterBattleId;
        GameController.Instance.GetCurrentCharacterInfo(out characterBattleId, out characterProperties);
        CurrentTurnIcon.SetCharacter(characterBattleId, characterProperties);

        RoundText.text = $"Round: {GameController.Instance.RoundCount}";
        TurnText.text = $"Turn: {GameController.Instance.TurnCount}";
    }

    public void UpdateTurnIcon(int characterBattleId, CharacterProperties properties)
    {
        //if this character take step now
        TurnIcon turnIcon = null;
        if (characterBattleId == CurrentTurnIcon.ChainedCharacterBattleId)
        {
            CurrentTurnIcon.UpdateIcon(properties);
        }

        foreach (var icon in TurnIcons)
            if (characterBattleId == icon.ChainedCharacterBattleId)
            {
                turnIcon = icon;
                break;
            }

        //update icon if it was found
        if (turnIcon != null)
            turnIcon.UpdateIcon(properties);

    }

}

