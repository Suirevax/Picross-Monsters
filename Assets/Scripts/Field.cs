using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Field : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler
{
    private Image button;
    private bool filled;

    public static event EventHandler FieldFlipped; 
    
    public bool Filled
    {
        get => filled;
        set
        {
            button.color = value ? Color.black : Color.white;
            filled = value;
            FieldFlipped?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Start()
    {
        button = GetComponent<Image>();
        filled = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Filled = !Filled;
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Input.GetMouseButton(0))
        {
            Filled = true;
        }else if (Input.GetMouseButton(1))
        {
            Filled = false;
        }else if (Input.GetMouseButton(2))
        {
            
        }
    }
}
