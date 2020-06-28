using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeFrame;

/// <summary>
/// 模式
/// </summary>
public enum Mode
{
    /// <summary>训练</summary>
    Training,
    /// <summary>实战</summary>
    ActualCombat,
}

/// <summary>
/// 难度
/// </summary>
public enum Difficulty
{
    Easy,
    Hard
}

public class GameSetting : SystemSingleton<GameSetting>
{
    /// <summary>模式</summary>
    public Mode mMode = Mode.Training;

    /// <summary>难度</summary>
    public Difficulty mDifficulty = Difficulty.Easy;

    /// <summary>机身材质索引</summary>
    public int mMaterialIndex = 0;

    /// <summary>音量</summary>
    public float mVolume = 0.1f;
}
