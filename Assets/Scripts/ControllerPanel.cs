using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllerPanel : MonoBehaviour
{

    private Text portnameText = null;
    private Text levelText = null;

    /// <summary>
    /// 現在回転している方向
    /// </summary>
    private Direction nowDirection = Direction.Right;
    /// <summary>
    /// 現在回転中かどうか
    /// </summary>
    private bool isMoving;

    /// <summary>
    /// 現在のUFOのパワー
    /// </summary>
    private int nowLevel
    {
        get { return _nowLevel; }
        set
        {
            if (value != _nowLevel)
            {
                _nowLevel = value;
                if (_nowLevel > 100) _nowLevel = 100;
                else if (_nowLevel < 0) _nowLevel = 0;
                if (levelText != null)
                    levelText.text = _nowLevel.ToString();
                if (isMoving)
                    UfoUtil.Singleton.SendData(nowDirection == Direction.Right, nowLevel);
            }
        }
    }
    private int _nowLevel = 0;

    // Use this for initialization
    void Start()
    {
        var obj = GameObject.Find("CurrentPortText");
        portnameText = obj.GetComponent<Text>();
        levelText = GameObject.Find("NowLevelText")?.GetComponent<Text>();

        UfoUtil.Singleton.FindPort();
        portnameText.text = UfoUtil.Singleton.PortName;
    }

    /// <summary>
    /// ポートを探索
    /// </summary>
    public void OnFindPortButton_Click()
    {
        UfoUtil.Singleton.FindPort();
        portnameText.text = UfoUtil.Singleton.PortName;
    }

    /// <summary>
    /// 停止ボタン
    /// </summary>
    public void OnPauseButtonClick()
    {
        UfoUtil.Singleton.SendStop();
        isMoving = false;
    }

    /// <summary>
    /// 正回転
    /// </summary>
    public void OnRightButtonClick()
    {
        UfoUtil.Singleton.SendData(true, nowLevel);
        isMoving = true;
    }

    /// <summary>
    /// 逆回転
    /// </summary>
    public void OnLeftButtonClick()
    {
        UfoUtil.Singleton.SendData(false, nowLevel);
        isMoving = true;
    }

    /// <summary>
    /// 強さUP
    /// </summary>
    public void OnPowerUpButtonClick()
    {
        nowLevel += 10;
    }
    /// <summary>
    /// 強さDOWN
    /// </summary>
    public void OnPowerDownButtonClick()
    {
        nowLevel -= 10;
    }

    // Update is called once per frame
    void Update()
    {


    }

    enum Direction
    {
        Right, Left
    }
}
