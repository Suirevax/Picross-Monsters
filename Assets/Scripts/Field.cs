using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Field : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler
{
    private Image _image;
    public enum FieldState{Disabled, Empty, Filled, Colored}
    private FieldState _state;

    public static event EventHandler FieldFlipped;

    public Color solutionColor;

    public FieldState State
    {
        get => _state;
        set
        {
            _state = value;
            switch (value)
            {
                case FieldState.Disabled:
                    _image.color = Color.grey;
                    break;
                case FieldState.Empty:
                    _image.color = Color.white;
                    FieldFlipped?.Invoke(this, EventArgs.Empty); //TODO: right now at the start of game the solution gets checked many many times
                    break;
                case FieldState.Filled:
                    _image.color = Color.black;
                    FieldFlipped?.Invoke(this, EventArgs.Empty);
                    break;
                case FieldState.Colored:
                    _image.color = new Color(solutionColor.r / 255, solutionColor.g / 255, solutionColor.b / 255, solutionColor.a); //TODO: changes the color in editor but doesn't change color in game view??
                    break;
            }
        }
    }

    private void Start()
    {
        _image = GetComponent<Image>();
        State = FieldState.Disabled;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        HandleUserInput();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        HandleUserInput();
    }

    private void HandleUserInput()
    {
        switch (State)
        {
            case FieldState.Empty:
                if (Input.GetMouseButton(0)) State = FieldState.Filled;
                break;
            case FieldState.Filled:
                if (Input.GetMouseButton(1)) State = FieldState.Empty;
                break;
        }
    }
}
