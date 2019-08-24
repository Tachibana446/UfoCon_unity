using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ControllerPanel : MonoBehaviour
{

    private Text portnameText = null;
    private Text levelText = null;
    /// <summary>
    /// 動作記録用ボタン
    /// </summary>
    public Button RecButton = null;
    private Text recButtonText = null;
    /// <summary>
    /// 現在録画中か
    /// </summary>
    private bool isRecording = false;
    /// <summary>
    /// 録画中の現在時刻
    /// </summary>
    private TimeSpan nowRecTime;
    /// <summary>
    /// 記録中のデータ
    /// </summary>
    private List<RecordData> recordList = new List<RecordData>();

    public List<RecordData> RecordList { get { return recordList; } }

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

    static string SaveFolder = "";

    static ControllerPanel()
    {
#if UNITY_EDITOR
        SaveFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/UfoCon";
#else
        SaveFolder = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');;
#endif
    }

    // Use this for initialization
    void Start()
    {
        var obj = GameObject.Find("CurrentPortText");
        portnameText = obj.GetComponent<Text>();
        levelText = GameObject.Find("NowLevelText")?.GetComponent<Text>();

        recButtonText = RecButton?.gameObject.GetComponentInChildren<Text>();

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
        nowDirection = Direction.Right;
        isMoving = true;
    }

    /// <summary>
    /// 逆回転
    /// </summary>
    public void OnLeftButtonClick()
    {
        UfoUtil.Singleton.SendData(false, nowLevel);
        nowDirection = Direction.Left;
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

    /// <summary>
    /// 録画開始・停止
    /// </summary>
    public void OnRecButtonClick()
    {
        isRecording = !isRecording;
        // 文字のトグル
        if (recButtonText != null)
        {
            if (isRecording)
                recButtonText.text = "■";
            else
                recButtonText.text = "●";
        }
        // 新たに記録を開始するとき経過時刻をリセット
        if (isRecording)
            nowRecTime = TimeSpan.FromMilliseconds(0);
    }

    /// <summary>
    /// CSVに書きこむボタン
    /// </summary>
    public void OnSaveCsvButton()
    {
        SaveRecordCSV("./sample_data.csv");
    }

    public void OnOpenCsvButton()
    {
        //  System.Diagnostics.Process.Start("code ./sample_data.csv");
    }

    // Update is called once per frame
    void Update()
    {
        // 記録
        if (isRecording)
        {
            nowRecTime += TimeSpan.FromSeconds(Time.deltaTime);
            // 変更があればデータとして追加
            int nowDeciSec = (int)(nowRecTime.TotalSeconds * 10);
            var current = new RecordData(nowDeciSec, nowDirection == Direction.Right, nowLevel);
            var prev = recordList.LastOrDefault();
            if (recordList.Count == 0 || !prev.LevelEqual(current))
                recordList.Add(current);
        }
    }

    /// <summary>
    /// ファイル名に保存用ディレクトリのパスを足して返す
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    static string GetSaveFilePath(string filename)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            filename = filename.Replace(c, '_');
        }
        return SaveFolder.TrimEnd('/') + "/" + filename;
    }

    /// <summary>
    /// 記録中のファイルをCSVに保存
    /// </summary>
    /// <param name="filename"></param>
    void SaveRecordCSV(string filename)
    {
        string filePath = GetSaveFilePath(filename);
        using (var sw = new StreamWriter(filePath))
        {
            foreach (var data in recordList)
            {
                sw.WriteLine(data.ToCSV());
            }
        }
    }

    /// <summary>
    /// 方向
    /// </summary>
    enum Direction
    {
        Right, Left
    }

    /// <summary>
    /// 記録用データ構造
    /// </summary>
    public class RecordData
    {
        /// <summary>
        /// デシ秒
        /// </summary>
        public int Time { get; set; }
        /// <summary>
        /// 強さ0 ~ 100
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// 正回転かどうか
        /// </summary>
        public bool IsPositive { get; set; }
        /// <summary>
        /// -100～+100で表した強さ
        /// </summary>
        public int SignedLevel { get { return (IsPositive ? 1 : -1) * Level; } }

        public RecordData(int time, bool isPositive, int level)
        {
            Time = time;
            IsPositive = isPositive;
            Level = level;
        }

        /// <summary>
        /// 時間以外（強さと方向）が一致しているかどうか
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool LevelEqual(RecordData b)
        {
            return SignedLevel == b.SignedLevel;
        }

        public override string ToString()
        {
            return $"{Time}:{(IsPositive ? "+" : "-")}{Level}";
        }

        public string ToCSV()
        {
            return Time.ToString() + "," + (IsPositive ? "0" : "1") + "," + Level.ToString();
        }
    }
}
