using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Xml;
using JetBrains.Annotations;
using UnityEditor.PackageManager;
using UnityEngine;


public class MonstersAPI : MonoBehaviour
{
    private static readonly Uri UriLink = new Uri("https://app.pixelencounter.com/api/basic/monsters/random");
    private const int GridSize = 12; //max width & height of a monster


    public async void GETMonster()
    {
        var httpClient = new HttpClient();
        var httpResponseMessage = await httpClient.GetAsync(UriLink);

        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            Debug.LogWarning("API Response failed");
            return;
        }

        var test = httpResponseMessage.Content;
        var text = await httpResponseMessage.Content.ReadAsStringAsync();
        Debug.Log(text);

        //Parsing to XML DOM
        var settings = new XmlReaderSettings
        {
            Async = true
        };

        var stream = await httpResponseMessage.Content.ReadAsStreamAsync();
        var reader = XmlReader.Create(stream, settings);
        Color[,] pixelGrid = new Color[GridSize,GridSize];

        //put all pixels in a list
        while (await reader.ReadAsync())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.Name == "rect")
                {
                    var x = 0;
                    var y = 0;
                    if(!int.TryParse(reader.GetAttribute("x"), out x)) continue;
                    if(!int.TryParse(reader.GetAttribute("y"), out y)) continue;
                    Debug.Log("Filledpos: " + x + "," + y);

                    var colorString = reader.GetAttribute("fill");

                    colorString = colorString.Remove(0, 1);
                    var colorInt = int.Parse(colorString, System.Globalization.NumberStyles.HexNumber);
                    pixelGrid[y/10, x/10] = new Color((colorInt >> 16) & 0xff, (colorInt >> 8) & 0xff, colorInt & 0xff);
                }
            }
        }
        GenerateRowHints(pixelGrid);
        GenerateColumnHints(pixelGrid);
        LogPixelGrid(pixelGrid);
    }

    private List<List<int>> GenerateColumnHints(Color[,] pixelGrid)
    {
        var columnsHints = new List<List<int>>();
        for (var x = 0; x < GridSize; x++)
        {
            var columnHints = new List<int>();
            var counter = 0;

            for (var y = 0; y < GridSize; y++)
            {
                if (pixelGrid[y, x].a != 0f)
                {
                    counter++;
                }
                else
                {
                    if(counter != 0) columnHints.Add(counter);
                    counter = 0;
                }
            }
            if(counter != 0) columnHints.Add(counter);
            columnsHints.Add(columnHints);
            
            // string debugMessage = "";
            // foreach (var hint in columnHints)
            // {
            //     debugMessage += hint.ToString() + ",";
            // }
            // Debug.Log(debugMessage);
        }

        return columnsHints;
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

            // string debugMessage = "";
            // foreach (var hint in rowHint)
            // {
            //     debugMessage += hint.ToString() + ",";
            // }
            //Debug.Log(debugMessage);
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

    void CheckForMultipleSolutions(List<List<int>> rowsHints, List<List<int>> columnsHints)
    {
        var height = rowsHints.Count;
        var width = columnsHints.Count;

        var grid = new bool[height,width];
        
        
    }
}
