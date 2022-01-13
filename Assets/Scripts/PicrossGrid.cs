using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PicrossGrid : MonoBehaviour
{
    public const int GridSize = 12; //max width & height of a monster
    
    private Color[,] _grid = new Color[GridSize,GridSize];
    private readonly Field[,] _fieldGrid = new Field[GridSize,GridSize];
    private readonly TMP_Text[] _rowHintText = new TMP_Text[12];
    private readonly TMP_Text[] _columnHintText = new TMP_Text[12];
    private Color[,] _solutionGrid = null;

    [SerializeField] private GameObject rowContainer;
    [SerializeField] private GameObject rowsHintsContainer;
    [SerializeField] private GameObject columnsHintsContainer;
    [SerializeField] private GameObject PopUpContainer;
    [SerializeField] private GameObject victoryPopup;

    private void Start()
    {
        if(!rowContainer) rowContainer = gameObject;
        if(!rowsHintsContainer) rowsHintsContainer = GameObject.Find("Rows Hints");
        if(!columnsHintsContainer) columnsHintsContainer = GameObject.Find("Columns Hints");
        if(!PopUpContainer) PopUpContainer = GameObject.Find("PopUp Container");

        
        
        //Fill _fieldGrid, _rowHintText, and _columnHintText
        for (var y = 0; y < GridSize; y++)
        {
            var row = rowContainer.transform.GetChild(y);
            for (var x = 0; x < GridSize; x++)
            {
                _fieldGrid[y,x] = row.GetChild(x).GetComponent<Field>();
            }
        }
        
        for (var i = 0; i < GridSize; i++)
        {
            _rowHintText[i] = rowsHintsContainer.transform.GetChild(i).GetComponent<TMP_Text>();
            _columnHintText[i] = columnsHintsContainer.transform.GetChild(i).GetComponent<TMP_Text>();
        }

        Field.FieldFlipped += CheckIfSolved;
    }

    private void CheckIfSolved(object sender, EventArgs eventArgs)
    {
        for (var y = 0; y < GridSize; y++)
        {
            for (var x = 0; x < GridSize; x++)
            {
                if ((_solutionGrid[y, x].a != 0f) != (_fieldGrid[y, x].State == Field.FieldState.Filled))
                {
                    //Debug.Log(x + "," + y);
                    return;
                }
            }
        }
        HandleGridSolved();
    }

    private void HandleGridSolved()
    {
        for (var y = 0; y < GridSize; y++)
        {
            for (var x = 0; x < GridSize; x++)
            {
                _fieldGrid[y, x].State = Field.FieldState.Colored; 
            }
        }

        Instantiate(victoryPopup, PopUpContainer.transform);
    }

    public void NewGrid(Color[,] solutionGrid, List<List<int>> rowsHints, List<List<int>> columnsHints)
    {
        _solutionGrid = solutionGrid;
        for (var i = 0; i < rowsHints.Count; i++)
        {
            var tmpRow = "";
            foreach (var hint in rowsHints[i])
            {
                tmpRow += hint + " ";
            }

            if (rowsHints[i].Count == 0)
            {
                tmpRow += "0";
            }

            _rowHintText[i].text = tmpRow;

            var tmpColumn = "";
            foreach (var hint in columnsHints[i])
            {
                tmpColumn += hint + "\n";
            }
            
            if (columnsHints[i].Count == 0)
            {
                tmpColumn += "0";
            }

            _columnHintText[i].text = tmpColumn;
        }
        
        
        //Set State of all field to empty & set solution color
        for (var y = 0; y < GridSize; y++)
        {
            for (var x = 0; x < GridSize; x++)
            {
                _fieldGrid[y, x].State = Field.FieldState.Empty;
                _fieldGrid[y, x].solutionColor = _solutionGrid[y,x];
            }
        }
    }
}
