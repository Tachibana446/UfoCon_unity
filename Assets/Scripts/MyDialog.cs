using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MyDialog : MonoBehaviour
{
    public string Title { get; private set; } = "タイトル";
    public string Description { get; private set; } = "メッセージ本文";

    public bool CancelButtonIsVisible { get; private set; } = true;

    public bool? Result { get; private set; } = null;

    /// <summary>
    /// ボタンが押された時に実行するイベント.引数はOKボタンが押されたかどうか
    /// </summary>
    public List<Action<bool?>> OnClosing = new List<Action<bool?>>();
    /// <summary>
    /// プレハブのリソースパス
    /// </summary>
    public const string PrefabPath = "Prefabs/DialogPanel_BackGroundPanel";

    /// <summary>
    /// Instantiateして返す
    /// </summary>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <param name="showCancelButton"></param>
    /// <returns></returns>
    public static MyDialog CreateDialog(Transform parent, string title, string message, bool showCancelButton = true)
    {
        var prefab = (GameObject)Resources.Load(PrefabPath);
        var gameObj = Instantiate(prefab, parent);
        var dialog = gameObj.GetComponent<MyDialog>();
        dialog.SetNames(title, message, showCancelButton);
        return dialog;
    }

    public void SetNames(string title, string descliption, bool showCancelButton = true)
    {
        Title = title; Description = descliption; CancelButtonIsVisible = showCancelButton;
    }

    private void Start()
    {
        var pane = gameObject.transform.Find("DialogPanel/TitlePanel")?.gameObject;

        var title = pane?.GetComponentInChildren<Text>();
        var description = gameObject.transform.Find("DialogPanel/MessagePanel")?.GetComponentInChildren<Text>();
        if (title != null) title.text = Title;
        if (description != null) description.text = Description;
        if (!CancelButtonIsVisible)
        {
            var cancelButton = gameObject.transform.Find("DialogPanel/ButtonsPanel/CancelButton");
            Destroy(cancelButton.gameObject);
        }
    }

    private void Update()
    {
        // ボタンがクリックされたらリスナを実行して閉じる
        if (Result != null)
        {
            OnClosing.ForEach(f => f(Result));
            Destroy(gameObject);
        }
    }

    public void OnOkButtonClick()
    {
        Result = true;
    }

    public void OnCancelButtonclick()
    {
        Result = false;
    }
}
