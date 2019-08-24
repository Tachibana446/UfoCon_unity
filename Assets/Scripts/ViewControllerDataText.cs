using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// コントローラー部分のデータを読み取って表示するテキスト
/// </summary>
public class ViewControllerDataText : MonoBehaviour
{

    ControllerPanel cont;
    Text text;

    // Use this for initialization
    void Start()
    {
        cont = GameObject.Find("ControllerPanel").GetComponent<ControllerPanel>();
        text = gameObject.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        var str = $"{cont.RecordList.Count}件のデータを記録中";
        if (text.text != str)
            text.text = str;
    }
}
