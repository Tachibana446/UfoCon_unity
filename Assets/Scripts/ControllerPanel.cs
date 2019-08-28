using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.UI;

public class ControllerPanel : MonoBehaviour
{

    private Text portnameText = null;
    private Text levelText = null;
    /// <summary>
    /// 動作記録用ボタン
    /// </summary>
    public UnityEngine.UI.Button RecButton = null;
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
    /// 音声を持っているパネル
    /// </summary>
    private MediaControllPanel mediaPanel = null;

    /// <summary>
    /// 現在回転している方向
    /// </summary>
    private Direction nowDirection = Direction.Right;
    /// <summary>
    /// 現在回転中かどうか
    /// </summary>
    private bool isMoving;
    /// <summary>
    /// ファイルダイアログが最後に開いたフォルダ
    /// </summary>
    private string lastDialogOpenedPath = "";

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

    /// <summary>
    /// ステータスバー的なテキスト
    /// </summary>
    static Text StatusText = null;

    /// <summary>
    /// 前フレーム時点での再生位置
    /// </summary>
    float prevTime = 0;

    static ControllerPanel()
    {
#if UNITY_EDITOR
        SaveFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/UfoCon";
#else
        SaveFolder = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');;
#endif
    }

    private void Awake()
    {
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
        // UI要素取得
        StatusText = GameObject.FindGameObjectWithTag("StatusText")?.GetComponent<Text>();
        mediaPanel = GameObject.FindGameObjectWithTag("MediaControllPanel")?.GetComponent<MediaControllPanel>();
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
    /// CSVを開くボタン
    /// </summary>
    public void OnOpenCSVButtonClick()
    {
        // 既にデータがあれば上書き確認
        if (recordList.Count > 0)
        {
            var dialog = MyDialog.CreateDialog(gameObject.transform.root, "読み込み確認",
                "新たにCSVをロードすると現在編集中のデータは破棄されますがよろしいですか？");
            dialog.OnClosing.Add(ok =>
            {
                if (ok == true)
                    ShowAndLoadCSVDialog();
                else
                    ShowStatusText("CSV読み込みをキャンセルしました。");
            });
        }
        else
        {
            ShowAndLoadCSVDialog();
        }

    }
    /// <summary>
    /// ファイルダイアログを開き、CSVを読み込んでデータを取得する
    /// </summary>
    private void ShowAndLoadCSVDialog()
    {
        var ofd = new OpenFileDialog();
        ofd.Filter = "csv files (*.csv)|*.csv|all files (*.*)|*.*";
        ofd.Title = "CSVファイルを選択";
        if (lastDialogOpenedPath == "")
            ofd.InitialDirectory = SaveFolder;
        else
            ofd.InitialDirectory = lastDialogOpenedPath;

        var resu = ofd.ShowDialog();
        if (resu == DialogResult.OK)
        {
            string filePath = ofd.FileName.Replace(@"\", "/");
            lastDialogOpenedPath = Path.GetDirectoryName(filePath);
            LoadRecordsCSV(filePath);
        }
    }


    /// <summary>
    /// CSVに書きこむボタン
    /// </summary>
    public void OnSaveCsvButton()
    {
        var inputField = GameObject.Find("CsvFileNameInputField").GetComponent<InputField>();
        string filename = inputField.text;
        // 空白の場合デフォルト名
        if (string.IsNullOrWhiteSpace(filename))
            filename = "record.csv";
        // 拡張子がついていない場合付ける
        if (!filename.EndsWith(".csv"))
            filename += ".csv";
        // ファイル名が補完された時のためにテキストボックスを更新
        inputField.text = filename;

        // 上書き確認
        if (File.Exists(GetSaveFilePath(filename)))
        {
            var dialog = MyDialog.CreateDialog(gameObject.transform.root, "上書き確認", "このファイルは既に存在しますが上書きしますか？");
            dialog.OnClosing.Add(isok =>
           {
               if (isok == true)
               {
                   SaveRecordCSV(filename);
               }
               else
               {
                   ShowStatusText("保存をキャンセルしました");
               }
           });
        }
        else
        {
            SaveRecordCSV(filename);
        }
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
        // 音声に合わせて実行
        MoveUfoByAudio();
    }

    /// <summary>
    /// 曲に合わせてデータ実行
    /// </summary>
    private void MoveUfoByAudio()
    {
        int nowTime = 0;
        if (mediaPanel?.audioSource?.isPlaying == true)
        {
            nowTime = (int)(TimeSpan.FromSeconds(mediaPanel.audioSource.time).TotalSeconds * 10);

            // 巻き戻しが行われていた場合何もしない
            if (prevTime < nowTime)
            {
                // 前フレームと現在の再生位置の間に時間指定されたデータがあれば実行
                var data = recordList.LastOrDefault(d => prevTime < d.Time && d.Time <= nowTime);
                if (data != null)
                    UfoUtil.Singleton.SendData(data.IsPositive, data.Level);
            }
        }
        prevTime = nowTime;
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
        ShowStatusText($"CSVを保存({filePath})");
    }

    /// <summary>
    /// CSVからRecordを読み込み
    /// </summary>
    /// <param name="filename"></param>
    void LoadRecordsCSV(string filename)
    {
        recordList.Clear();
        foreach (var line in File.ReadLines(filename))
        {
            var sp = line.Split(',');
            if (sp.Length < 3) continue;
            int t = 0, f = 0, l = 0;
            if (int.TryParse(sp[0], out t) && int.TryParse(sp[1], out f) && int.TryParse(sp[2], out l))
            {
                var data = new RecordData(t, f == 0, l);
                recordList.Add(data);
            }
        }
        if (recordList.Count > 0)
        {
            recordList.Sort((a, b) => a.Time - b.Time);
            // ステータスバー
            ShowStatusText("CSVを読み込みました");
            // テキストフィールド
            var inputField = GameObject.Find("CsvFileNameInputField").GetComponent<InputField>();
            inputField.text = filename;
        }
    }

    /// <summary>
    /// ステータスバー的なヤツに文字を表示
    /// </summary>
    /// <param name="text"></param>
    public static void ShowStatusText(string text)
    {
        StatusText.text = text;
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
