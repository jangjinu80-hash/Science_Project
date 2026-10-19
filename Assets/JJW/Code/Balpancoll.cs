using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Balpancoll : MonoBehaviour
{
    [SerializeField] private Sprite onButtonSprite; 
    [SerializeField] private Sprite offButtonSprite;
    
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        OnElectricityButton();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        OffElectricityButton();
    }

    private void OnElectricityButton()
    {
        Debug.Log("버튼 작동함");
        _spriteRenderer.sprite = onButtonSprite;
       
    }

    private void OffElectricityButton()
    {
        Debug.Log("버튼 작동 해제");
        _spriteRenderer.sprite = offButtonSprite;
    }
}
