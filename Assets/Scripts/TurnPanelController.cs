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

    public CharacterPanel CharacterPanelRef;
    // Start is called before the first frame update
    void Start()
    {
        TurnIcons = new List<TurnIcon>();
        GameController.Instance.OnTurnStart.AddListener(OnTurnStart);

        CurrentTurnIcon.OnTurnIconClick.AddListener(ProcessTurnIconClick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddIcon(Character character, CharacterProperties properties)
    {
        GameObject gameObject = Instantiate(TurnIconPrefab, TurnIconsHandler.transform);
        TurnIcon turnIcon = gameObject.GetComponent<TurnIcon>();
        turnIcon.SetCharacter(character.BattleId, properties);
        TurnIcons.Add(turnIcon);
        turnIcon.OnTurnIconClick.AddListener(ProcessTurnIconClick);
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

    public void ProcessTurnIconClick(int characterBattleId)
    {
        //get character properties and set up panel
        CharacterProperties characterProperties = GameController.Instance.FindCharacter(characterBattleId).Properties;
        CharacterPanelRef.ShowPanel(characterProperties);
    }

    public void RemoveTurnIcon(int characterBattleId)
    {
        //character's turn
        if (CurrentTurnIcon.ChainedCharacterBattleId == characterBattleId && _lastTurnIcon != null)
        {
            Destroy(_lastTurnIcon.gameObject);
            _lastTurnIcon = null;
        }
        else
        {
            //find turn icon
            TurnIcon targetTurnIcon = null;
            int i;
            for (i = 0; i < TurnIcons.Count; ++i)
                if (TurnIcons[i].ChainedCharacterBattleId == characterBattleId)
                {
                    targetTurnIcon = TurnIcons[i];
                    break;
                }
            TurnIcons.RemoveAt(i);
            targetTurnIcon.gameObject.SetActive(false);
            Destroy(targetTurnIcon.gameObject);
        }
    }
}

