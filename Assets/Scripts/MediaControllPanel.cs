using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.UI;

public class MediaControllPanel : MonoBehaviour
{

    OpenFileDialog dialog;
    /// <summary>
    /// 音声ファイルのパス
    /// </summary>
    string filePath;
    /// <summary>
    /// ファイルパスを表示するInputField
    /// </summary>
    InputField FilePathInput = null;
    /// <summary>
    /// 音声
    /// </summary>
    public AudioSource audioSource { get; private set; } = null;
    /// <summary>
    /// 現在の再生時間を表示するテキスト
    /// </summary>
    Text positionText = null;
    /// <summary>
    /// 現在の再生位置を表すスライダー
    /// </summary>
    Slider positionSlider = null;
    /// <summary>
    /// スライダーのValueChangedイベントがユーザーの手で起こされたのかを知るためのフラグ
    /// </summary>
    bool isSliderChangedByCode = false;

    // Use this for initialization
    void Start()
    {
        positionText = gameObject.transform.Find("PositionText")?.GetComponent<Text>();
        audioSource = gameObject.AddComponent<AudioSource>();
        FilePathInput = gameObject.GetComponentInChildren<InputField>();
        positionSlider = gameObject.transform.Find("PositionSlider")?.GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        // 再生時間を表示
        if (audioSource?.isPlaying == true)
        {
            if (positionText != null)
            {
                var ts = System.TimeSpan.FromSeconds(audioSource.time);
                string s1 = ts.ToString(@"hh\:mm\:ss\.f");
                string s2 = ((int)(ts.TotalSeconds * 10)).ToString("D4");
                positionText.text = $"{s1}\n({s2})";
            }
            // スライダを更新
            if (positionSlider != null)
            {
                isSliderChangedByCode = true;
                positionSlider.value = audioSource.time;
            }
        }
    }


    /// <summary>
    /// ファイルを開くボタンをクリックした
    /// </summary>
    public void OnOpenFileButtonClick()
    {
        dialog = new OpenFileDialog();
        dialog.Filter = "wav files (*.wav)|*.wav|ogg files (*.ogg)|*.ogg|all files (*.*)|*.*";
        dialog.Title = "音声ファイルを選択";

        var res = dialog.ShowDialog();
        if (res == DialogResult.OK)
        {
            filePath = dialog.FileName;
            StartCoroutine(LoadAudioSource());
            FilePathInput.text = filePath;
        }
        else
        {
            ControllerPanel.ShowStatusText("ファイルオープンをキャンセル");
        }
    }

    /// <summary>
    /// 再生ボタンをクリックした
    /// </summary>
    public void OnPlayButtonClick()
    {
        if (audioSource?.isPlaying == false)
            audioSource.Play();
    }
    /// <summary>
    /// 一時停止ボタンをクリックした
    /// </summary>
    public void OnPauseButtonClick()
    {
        audioSource?.Pause();
    }
    /// <summary>
    /// 再生時間のスライダーが移動された時
    /// </summary>
    /// <param name="pos"></param>
    public void OnChangePositionSlider(float pos)
    {
        if (isSliderChangedByCode)
        {
            isSliderChangedByCode = false;
            return;
        }
        audioSource.time = pos;
    }

    /// <summary>
    /// ファイルパスからオーディオクリップをロード
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadAudioSource()
    {
        using (WWW www = new WWW("file://" + filePath))
        {
            while (!www.isDone)
                yield return null;

            audioSource.clip = www.GetAudioClip(false, true);
            if (audioSource.clip.loadState != AudioDataLoadState.Loaded)
            {
                ControllerPanel.ShowStatusText("音声ファイルの読み込みに失敗");
                yield break;
            }
            positionSlider.maxValue = audioSource.clip.length; // スライダーの最大値を指定
            ControllerPanel.ShowStatusText($"{filePath.Replace("\n", "")}をオープン");

        }
    }
}
