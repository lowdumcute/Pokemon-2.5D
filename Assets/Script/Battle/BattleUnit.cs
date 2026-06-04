
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class BattleUnit : MonoBehaviour
{
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;
    public bool IsPlayerUnit {get { return isPlayerUnit; } }
    public BattleHud Hud { get { return hud; } }
    public Pokemon Pokemon { get; set; }
    Image image;
    Vector3 originalPos;
    Color originalColor;
    
    private void Awake()
    {
        image = GetComponent<Image>();
        originalPos = image.transform.localPosition;
        originalColor = image.color;
    }
    public void Setup(Pokemon pokemon)
    {
        Pokemon = pokemon;
        if (isPlayerUnit)
        {
            image.sprite = Pokemon.Base.BackSprite;
        }
        else
        {
            image.transform.localScale =  new Vector3(1f, 1f, 1f); // Lật hình ảnh cho đối thủ
            image.sprite = Pokemon.Base.FrontSprite;
            
        }
        hud.SetData(pokemon);
        image.color = originalColor;
        PlayEnterAnimation();
    }
    public void PlayEnterAnimation()
    {
        if (isPlayerUnit)
        {
            image.transform.localPosition = new Vector3 (-1500f, originalPos.y);
        }
        else
        {
            image.transform.localPosition = new Vector3 (1500f, originalPos.y);
        }
        image.transform.DOLocalMoveX(originalPos.x, 1.5f).SetEase(Ease.OutBack);
    }
    public void PlayAttackAnimation()
    {
        var senquence = DOTween.Sequence();
        if (isPlayerUnit)
        {
            senquence.Append(image.transform.DOLocalMoveX(originalPos.x + 50f, 0.25f));
        }
        else
        {
            senquence.Append(image.transform.DOLocalMoveX(originalPos.x - 50f, 0.25f));
        }
        senquence.Append(image.transform.DOLocalMoveX(originalPos.x, 0.25f));
    }
    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.gray, 0.1f));
        sequence.Append(image.DOColor(originalColor, 0.1f));
    }
    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveY(originalPos.y - 150f, 0.5f));
        sequence.Join(image.DOFade(0f, 0.5f));
    }
}
