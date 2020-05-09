using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class PointerHandler : MonoBehaviour
{
    public enum PointerStatus
    {
        Normal = 0,
        RightAttack = 1,
        BottomAttack = 2,
        LeftAttack = 3,
        TopAttack = 4,
        RangeAttack = 5,
    }
    public List<Sprite> SpritesList = new List<Sprite>(5);
    public List<Color> ColorsList = new List<Color>(5);
    private PointerStatus _currentStatus;
    private SpriteRenderer _spriteRenderer;
    private float _tileSpace = 0.25f;

    // Start is called before the first frame update
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSprite(PointerStatus newStatus)
    {
        //_spriteRenderer.sprite = SpritesList[(int)newStatus];
        _spriteRenderer.color = ColorsList[(int)newStatus];
        _currentStatus = newStatus;
    }

    public PointerStatus GetStatus()
    {
        return _currentStatus;
    }

    public void DefineSprite(Vector2 relativeMousePosition, List<PointerHandler.PointerStatus> directionsList, out int dirListIndex)
    {
        //check all directions from right to top clockwise
        if (relativeMousePosition.x > (1 - _tileSpace) && (dirListIndex = directionsList.IndexOf(PointerStatus.RightAttack)) != -1)
            SetSprite(PointerStatus.RightAttack);
        else if (relativeMousePosition.y < _tileSpace && (dirListIndex = directionsList.IndexOf(PointerStatus.BottomAttack)) != -1)
            SetSprite(PointerStatus.BottomAttack);
        else if (relativeMousePosition.x < _tileSpace && (dirListIndex = directionsList.IndexOf(PointerStatus.LeftAttack)) != -1)
            SetSprite(PointerStatus.LeftAttack);
        else if (relativeMousePosition.y > (1 - _tileSpace) && (dirListIndex = directionsList.IndexOf(PointerStatus.TopAttack)) != -1)
            SetSprite(PointerStatus.TopAttack);
        else
        {
            SetSprite(directionsList[0]);
            dirListIndex = 0;
        }
    }
}
