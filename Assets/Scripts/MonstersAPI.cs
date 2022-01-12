using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Xml;
using JetBrains.Annotations;
using UnityEditor.PackageManager;
using UnityEngine;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

//https://github.com/Kanawanagasaki/NonogramSolver

public class MonstersAPI : MonoBehaviour
{
    [DllImport("C:\\Users\\rxpja\\source\\repos\\picross-solver-dll\\x64\\Debug\\picross-solver-dll.dll")]
    private static extern int Solutions_n(string rowBuffer, string columnBuffer);

    private static readonly Uri UriLink = new Uri("https://app.pixelencounter.com/api/basic/monsters/random");
    private const int GridSize = 12; //max width & height of a monster

    [SerializeField] private PicrossGrid _picrossGrid;

    public async void GETMonster()
    {
        
        var httpResponseMessage = await CallAPI();
        var pixelGrid = await ResponseToGrid(httpResponseMessage);

    

        

        

        //put all pixels in a grid
  

        LogPixelGrid(pixelGrid);

        Debug.Log("RowHints");
        var rowsHints = GenerateRowHints(pixelGrid);
        var rowsString = HintsToString(rowsHints);
        Debug.Log(rowsString);
        Debug.Log("ColumnHints");
        var columnHints = GenerateColumnHints(pixelGrid);
        var columnsString = HintsToString(columnHints);
        Debug.Log((columnsString));

        
        /*
        * Validation method: check that the input grid is valid and has a unique solution.
        *
        * Returns a pair composed of an integer code and an error message. The code is as follows:
        *
        *  -1  ERR     The input grid is invalid
        *   0  ZERO    No solution found
        *   1  OK      Valid grid with a unique solution
        *   2  MULT    The solution is not unique
        */
        
        var code = Solutions_n(rowsString, columnsString);
        if (code != 1)
        {
            Debug.LogWarning("validation code != 1");
            return;
        }

        _picrossGrid.NewGrid(pixelGrid, rowsHints, columnHints);
    }

    private string HintsToString(List<List<int>> hintsList)
    {
        var tmp = "";
        foreach (var hints in hintsList)
        {
            foreach (var hint in hints)
            {
                tmp += hint.ToString() + ",";
            }

            if (hints.Count == 0) tmp += "0";
            tmp += ";";
        }

        tmp = tmp.Replace(",;", ";");
        return tmp.Remove(tmp.Length -1);
    }

    private async Task<HttpResponseMessage> CallAPI()
    {
        var httpClient = new HttpClient();
        var httpResponseMessage = await httpClient.GetAsync(UriLink);

        if (httpResponseMessage.IsSuccessStatusCode) return httpResponseMessage;
        
        Debug.LogWarning("API Response failed");
        return null;
    }

    async Task<Color[,]> ResponseToGrid(HttpResponseMessage httpResponseMessage)
    {
        //Parsing to XML DOM
        var settings = new XmlReaderSettings
        {
            Async = true
        };

        var stream = await httpResponseMessage.Content.ReadAsStreamAsync();
        var reader = XmlReader.Create(stream, settings);
        var pixelGrid = new Color[GridSize, GridSize];
        
        while (await reader.ReadAsync())
        {
            if (reader.NodeType != XmlNodeType.Element) continue;
            if (reader.Name != "rect") continue;
            if (!int.TryParse(reader.GetAttribute("x"), out var x)) continue;
            if (!int.TryParse(reader.GetAttribute("y"), out var y)) continue;

            var colorString = reader.GetAttribute("fill").Substring(1);
            var colorInt = int.Parse(colorString, System.Globalization.NumberStyles.HexNumber);
            pixelGrid[y / 10, x / 10] =
                new Color((colorInt >> 16) & 0xff, (colorInt >> 8) & 0xff, colorInt & 0xff);
        }

        return pixelGrid;
    }
    

    private List<List<int>> GenerateRowHints(Color[,] pixelGrid)
    {
        var rowsHints = new List<List<int>>();
        for (var y = 0; y < GridSize; y++)
        {
            var rowHints = new List<int>();
            var counter = 0;
            for (var x = 0; x < GridSize; x++)
            {
                if (pixelGrid[y, x].a != 0f)
                {
                    counter++;
                }
                else
                {
                    if(counter != 0) rowHints.Add(counter);
                    counter = 0;
                }
            }
            if(counter != 0) rowHints.Add(counter);
            rowsHints.Add(rowHints);
        }
        return rowsHints;
    }
    
    private List<List<int>> GenerateColumnHints(Color[,] pixelGrid)
    {
        var rowsHints = new List<List<int>>();
        for (var x = 0; x < GridSize; x++)
        {
            var rowHints = new List<int>();
            var counter = 0;
            for (var y = 0; y < GridSize; y++)
            {
                if (pixelGrid[y, x].a != 0f)
                {
                    counter++;
                }
                else
                {
                    if(counter != 0) rowHints.Add(counter);
                    counter = 0;
                }
            }
            if(counter != 0) rowHints.Add(counter);
            rowsHints.Add(rowHints);
        }
        return rowsHints;
    }

    void LogPixelGrid(Color[,] pixelGrid)
    {
        var text = "[[";
        for (var y = 0; y < GridSize; y++)
        {
            if (y > 0)
            {
                text += "\n[";
            }
            for (var x = 0; x < GridSize; x++)
            {
                var value = pixelGrid[y, x].a != 0f;
                if (x < GridSize - 1)
                {
                    text += Convert.ToInt16(value) + ",";
                }
                else
                {
                    text += Convert.ToInt16(value);
                }
            }
            text += "]";
        }
        text += "]";
        Debug.Log(text);
    }
}
