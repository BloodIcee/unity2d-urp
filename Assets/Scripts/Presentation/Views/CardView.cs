using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image cardImage;
    [SerializeField] private Image backImage;
    [SerializeField] private Image baseImage;
    [SerializeField] private Image bodyImage;
    
    private CardModel model;
    private CardController controller;
    private bool isInitialized;
    private bool interactable = true;

    public CardModel Model => model;
    public Image BaseImage => baseImage;
    public Image BackImage => backImage;
    public bool IsRevealed => model != null && 
        (model.CurrentState == CardState.Revealed || model.CurrentState == CardState.Matched);

    private void Awake()
    {
        if (cardImage != null)
            cardImage.raycastTarget = true;
        
        DisableRaycastOnChildren();
    }

    private void DisableRaycastOnChildren()
    {
        if (backImage != null)
            backImage.raycastTarget = false;
        if (baseImage != null)
            baseImage.raycastTarget = false;
        if (bodyImage != null)
            bodyImage.raycastTarget = false;
    }

    public void Initialize(CardModel cardModel, CardController cardController, Sprite baseSprite, Sprite backSprite)
    {
        model = cardModel;
        controller = cardController;
        isInitialized = true;

        if (cardImage != null)
        {
            cardImage.sprite = null;
            cardImage.color = new Color(1, 1, 1, 0);
        }

        if (baseImage != null)
            baseImage.sprite = baseSprite;
        
        if (backImage != null)
            backImage.sprite = backSprite;

        UpdateVisuals();
        SetInteractable(true);
    }

    public void UpdateVisuals()
    {
        if (!isInitialized || model == null)
            return;

        bool isRevealed = model.CurrentState == CardState.Revealed || 
                         model.CurrentState == CardState.Matched;

        if (baseImage != null)
            baseImage.gameObject.SetActive(isRevealed);

        if (bodyImage != null && model.FrontSprite != null)
            bodyImage.sprite = model.FrontSprite;

        if (backImage != null)
            backImage.gameObject.SetActive(!isRevealed);
    }

    public void SetInteractable(bool value)
    {
        interactable = value;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isInitialized && controller != null && interactable)
            controller.OnCardClicked(this);
    }

    public void ResetCard()
    {
        isInitialized = false;
        model = null;
        controller = null;
        
        if (baseImage != null)
            baseImage.gameObject.SetActive(false);
        if (backImage != null)
            backImage.gameObject.SetActive(true);
        
        SetInteractable(false);
    }
}

