using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IntroTextWriter : MonoBehaviour, IPointerClickHandler
{
    #region Public Fields
    
    [Header("Intro Text Writer Settings")]
    
    [Tooltip("The continue button that will be used to get to the next scene")]
    public Button continueButton;
    
    [Tooltip("The scroll image's rect transform")]
    public RectTransform scrollRectTransform;
    
    [Tooltip("The delay before text writing will begin")]
    public float startDelay = 2f;
    
    [Tooltip("The delay between a character being written")]
    public float writeCharacterDelay = 0.1f;
    
    [Tooltip("The delay between a full sentence when a punctuation character is seen")]
    public float fullSentenceDelay = 0.5f;
    
    #endregion

    #region Private Fields
    
    // Text Writing
    private TMP_Text _introText;
    private string _messageToWrite;
    private int _textIndex;
    
    // States
    private bool _textWriteComplete;
    
    // Timers
    private float _startTimer;
    private float _writeCharacterTimer;
    
    // UI Manager
    private UIManager _uiManager;

    #endregion

    #region Unity Events
    
    private void Awake()
    {
        _uiManager = FindObjectOfType<UIManager>();
        _introText = GetComponent<TMP_Text>();
        continueButton.onClick.AddListener(ContinueBtnOnClick);
    }

    private void OnEnable()
    {
        continueButton.interactable = false;
        _textWriteComplete = false;
        _textIndex = 0;
        _writeCharacterTimer = writeCharacterDelay;
        _startTimer = startDelay;
        _messageToWrite = _introText.text;
        _introText.text = string.Empty;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!gameObject.activeInHierarchy || scrollRectTransform.localScale.x < 1f)
            return;

        if (_textWriteComplete && _introText.text.Length != _messageToWrite.Length)
            _introText.text = _messageToWrite;

        _startTimer -= Time.deltaTime;
        
        if (_startTimer > 0)
            return;
        
        WriteText();
    }
    
    #endregion

    #region Update Methods
    
    private void WriteText()
    {
        if (_textWriteComplete)
            return;
        
        if (_writeCharacterTimer <= 0)
        {
            string text = _messageToWrite[.._textIndex];
            text += $"<color=#00000000>{_messageToWrite[_textIndex..]}</color>";
            _introText.text = text;
            
            _writeCharacterTimer = IsPunctuationCharacter(_textIndex != 0 ? _textIndex - 1 : _textIndex)
                ? fullSentenceDelay
                : writeCharacterDelay;
            
            _textIndex++;

            if (_textIndex < _messageToWrite.Length)
                return;

            TriggerWriteComplete();
        }
        else
            _writeCharacterTimer -= Time.deltaTime;
    }
    
    #endregion

    #region Private Helper Methods
    
    private bool IsPunctuationCharacter(int textIndex)
    {
        return _messageToWrite[textIndex] == '.' || _messageToWrite[textIndex] == '?' ||
               _messageToWrite[textIndex] == '!';
    }

    private void TriggerWriteComplete()
    {
        _textWriteComplete = true;
        continueButton.interactable = true;
    }
    
    #endregion

    #region Click Event Methods
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (scrollRectTransform.localScale.x < 1)
            return;

        TriggerWriteComplete();
    }

    private void ContinueBtnOnClick()
    {
        if (!_uiManager)
            return;
        
        _uiManager.ShowTransitionBgUI(true);
    }
    
    #endregion
}