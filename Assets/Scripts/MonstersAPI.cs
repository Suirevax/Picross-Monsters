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
    private const int Size = 12;


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
        XmlReaderSettings settings = new XmlReaderSettings();
        settings.Async = true;
        
        var reader = XmlReader.Create(httpResponseMessage.Content.ReadAsStreamAsync().Result, settings);
        
        while (await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    Debug.Log("Start Element " + reader.Name);
                    Debug.Log("X = " + reader.GetAttribute("x") + "; Y = " +  reader.GetAttribute("y"));
                    break;
                default:
                    Debug.Log("Other node " + reader.NodeType + " with value " + reader.Value);
                    break;
            }
        }
    }
}
