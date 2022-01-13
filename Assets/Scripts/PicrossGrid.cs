using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PicrossGrid : MonoBehaviour
{
    private const int GridSize = 12;
    
    private Color[,] _grid = new Color[GridSize,GridSize];
    public Color[,] _solutionGrid = null;

    [SerializeField] private GameObject rowContainter;
    [SerializeField] private GameObject rowsHintsContainter;
    [SerializeField] private GameObject columnsHintsContainter;

    [SerializeField] private GameObject victoryPopup;
    
    private Field[,] _fieldGrid = new Field[GridSize,GridSize];
    private TMP_Text[] rowHintText = new TMP_Text[12];
    private TMP_Text[] columnHintText = new TMP_Text[12];

    
    
    private void Start()
    {
        for (var y = 0; y < GridSize; y++)
        {
            var row = rowContainter.transform.GetChild(y);
            for (var x = 0; x < GridSize; x++)
            {
                _fieldGrid[y,x] = row.GetChild(x).GetComponent<Field>();
            }
        }
        
        for (var i = 0; i < GridSize; i++)
        {
            rowHintText[i] = rowsHintsContainter.transform.GetChild(i).GetComponent<TMP_Text>();
            columnHintText[i] = columnsHintsContainter.transform.GetChild(i).GetComponent<TMP_Text>();
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

    void HandleGridSolved()
    {
        for (var y = 0; y < GridSize; y++)
        {
            for (var x = 0; x < GridSize; x++)
            {
                _fieldGrid[y, x].State = Field.FieldState.Colored; //TODO: figure out why this doesn't update in the game view but does update in the inspector
            }
        }

        Instantiate(victoryPopup, transform);
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

            rowHintText[i].text = tmpRow;

            var tmpColumn = "";
            foreach (var hint in columnsHints[i])
            {
                tmpColumn += hint + "\n";
            }
            
            if (columnsHints[i].Count == 0)
            {
                tmpColumn += "0";
            }

            columnHintText[i].text = tmpColumn;
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
