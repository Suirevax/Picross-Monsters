using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUp : MonoBehaviour
{
    private void Start()
    {
        Destroy(gameObject, 5f);
    }
}
