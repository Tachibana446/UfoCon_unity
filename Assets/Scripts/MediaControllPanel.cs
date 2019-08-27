using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.UI;

public class MediaControllPanel : MonoBehaviour
{

    OpenFileDialog dialog;
    string filePath;
    InputField FilePathInput = null;
    AudioSource audioSource = null;

    // Use this for initialization
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        FilePathInput = gameObject.GetComponentInChildren<InputField>();
    }

    // Update is called once per frame
    void Update()
    {

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
            filePath = dialog.FileName;
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
            ControllerPanel.ShowStatusText($"{filePath.Replace("\n", "")}をオープン");

        }
    }
}
