﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;

public class UfoUtil
{
    public static UfoUtil Singleton = new UfoUtil();

    private UfoUtil()
    {

    }

    private SerialPort Port;
    public string PortName { get; private set; } = "なし";

    /// <summary>
    /// 機種識別用のデータ
    /// </summary>
    readonly byte[] CheckCmd = new byte[] { 0xF0, 0x01, 0x00 };

    /// <summary>
    /// 自動的にポートを探す.見つからなければnull
    /// </summary>
    public void FindPort()
    {
        Port?.Close();
        Port = null;

        foreach (var name in SerialPort.GetPortNames())
        {
            var p = new SerialPort(name, 19200, Parity.None, 8, StopBits.One);
            p.Open();
            p.DtrEnable = true;
            p.RtsEnable = true;
            p.ReadTimeout = 100;
            p.Write(CheckCmd, 0, CheckCmd.Length);
            Int32 result = p.ReadByte();
            // UFOは機種識別用のデータに対して0x02を返す。なおA10は0x01を返すらしい。
            if (result == 2)
            {
                PortName = name;
                Port = p;
                break;
            }
            else
            {
                p.Close();
            }
        }
        if (Port == null)
            PortName = "なし";
    }

    /// <summary>
    /// ポートの手動指定
    /// </summary>
    /// <param name="portName">COM5とか</param>
    public void SetPort(string portName)
    {
        this.PortName = portName;
        Port.Close();
        Port = new SerialPort(portName, 19200, Parity.None, 8, StopBits.One);
        Port.Open();

        Port.DtrEnable = true;
        Port.RtsEnable = true;
        Port.ReadTimeout = 100;
    }

    /// <summary>
    /// ポート一覧を返す
    /// </summary>
    /// <returns></returns>
    public static string[] GetPorts()
    {
        return SerialPort.GetPortNames();
    }

    /// <summary>
    /// データ送信
    /// </summary>
    /// <param name="direction">正方向ならtrue</param>
    /// <param name="value">0 ~ 100</param>
    public void SendData(bool direction, int value)
    {
        if (Port == null || !Port.IsOpen)
            return;

        if (value > 100)
            value = 100;
        else if (value < 0)
            value = 0;
        // 逆回転
        if (!direction)
            value = value | 0x80;

        byte[] data = new byte[] { 0x02, 0x01, (byte)value };
        Port.Write(data, 0, data.Length);
    }

    /// <summary>
    /// UFOの回転を止める
    /// </summary>
    public void SendStop()
    {
        SendData(true, 0);
    }
}
