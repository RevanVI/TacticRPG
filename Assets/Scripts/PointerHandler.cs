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
}
