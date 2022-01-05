using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using UnityEditor.PackageManager;
using UnityEngine;

public class MonstersAPI : MonoBehaviour
{
    private Uri _uri = new Uri("https://app.pixelencounter.com/api/basic/monsters/random");
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async void getMonster()
    {
        var httpClient = new HttpClient();
        var httpResponseMessage = await httpClient.GetAsync(_uri);

        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            Debug.LogWarning("API Response failed");
            return;
        }
        
        var text = await httpResponseMessage.Content.ReadAsStringAsync();
        Debug.Log(text);
    }
}
