using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllerPanel : MonoBehaviour
{

    private Text portnameText = null;

    // Use this for initialization
    void Start()
    {
        var obj = GameObject.Find("CurrentPortText");
        portnameText = obj.GetComponent<Text>();

        UfoUtil.Singleton.FindPort();
        portnameText.text = UfoUtil.Singleton.PortName;
    }

    void OnFindPortButton_Click()
    {
        UfoUtil.Singleton.FindPort();
        portnameText.text = UfoUtil.Singleton.PortName;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
