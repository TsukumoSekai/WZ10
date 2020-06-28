using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using WeFrame;
using WeFrame.Common;
using DG.Tweening;
using UnityEngine.SceneManagement;

public enum SwitchPanelType
{
    None,
    Left,
    Right,
    Both
}

public class HomeView : BaseView
{
    /// <summary>机体材质</summary>
    private Material[] mPlaneMat = new Material[3];
    /// <summary>菜单动画队列</summary>
    private Sequence mSequence;

    protected override void Init()
    {
        base.Init();

        foreach (ScreenOutput output in mSUISoutDict.Values)
        {
            output.mOnScreenClick += BtnInteract;
        }

        for (int i = 0; i < 3; i++)
        {
            mPlaneMat[i] = Resources.Load<Material>("Materials/body_mat_" + i.ToString());
        }
    }

    protected override void AfterInit()
    {
        base.AfterInit();

        ChangeDifficulty(GameSetting.Self.mDifficulty);
        ChangeMaterial(GameSetting.Self.mMaterialIndex);
        SwitchPanel(SwitchPanelType.None, SwitchPanelType.Right);
    }

    private void BtnInteract(PointerEventData data)
    {
        if (mSequence.IsPlaying())
            return;

        TagData tagData = CConvert.StringToTag(data.pointerPress.name);

        switch (tagData.mIndexList[0])
        {
            case "资料介绍":
                SwitchPanel(SwitchPanelType.Right, SwitchPanelType.Both);
                mSequence.InsertCallback(1f, () =>
                {
                    SetGoActive(mSUIGoDict["RightPage-0"], false);
                    SetGoActive(mSUIGoDict["RightPage-1"], true);
                    SetLeftPanel(InfoDataCenter.InfoData.InfoList[0].Title, InfoDataCenter.InfoData.InfoList[0].Content);
                });
                break;
            case "训练教程":
                SwitchPanel(SwitchPanelType.Right, SwitchPanelType.Both);
                mSequence.InsertCallback(1f, () =>
                {
                    SetGoActive(mSUIGoDict["RightPage-0"], false);
                    SetGoActive(mSUIGoDict["RightPage-2"], true);
                    SetLeftPanel("操作方式说明", "W：前进\nS：后退\nA：左平移\nD：右平移\nQ：左旋转\nE：右旋转\n空格：上升\nCtrl：下降\nC：锁定目标\nTab：切换目标\n\n鼠标左键：机炮射击\n鼠标右键：火箭射击\n鼠标中键：导弹射击");
                });
                GameSetting.Self.mMode = Mode.Training;
                break;
            case "仿真实战":
                SwitchPanel(SwitchPanelType.Right, SwitchPanelType.Both);
                mSequence.InsertCallback(1f, () =>
                {
                    SetGoActive(mSUIGoDict["RightPage-0"], false);
                    SetGoActive(mSUIGoDict["RightPage-2"], true);
                    SetLeftPanel("任务简报", "任务性质：歼灭战\n任务地点：敌军小型补给点\n目标构成：装甲车辆\n任务目标：歼灭所有敌人");
                });
                GameSetting.Self.mMode = Mode.ActualCombat;
                break;
            case "背景资料":
                SwitchPanel(SwitchPanelType.Left, SwitchPanelType.Left);
                mSequence.InsertCallback(1f, () =>
                {
                    SetLeftPanel(InfoDataCenter.InfoData.InfoList[0].Title, InfoDataCenter.InfoData.InfoList[0].Content);
                });
                break;
            case "机体参数":
                SwitchPanel(SwitchPanelType.Left, SwitchPanelType.Left);
                mSequence.InsertCallback(1f, () =>
                {
                    SetLeftPanel(InfoDataCenter.InfoData.InfoList[1].Title, InfoDataCenter.InfoData.InfoList[1].Content);
                });
                break;
            case "武器资料":
                SwitchPanel(SwitchPanelType.Left, SwitchPanelType.Left);
                mSequence.InsertCallback(1f, () =>
                {
                    SetLeftPanel(InfoDataCenter.InfoData.InfoList[2].Title, InfoDataCenter.InfoData.InfoList[2].Content);
                });
                break;
            case "开始任务":
                SetSUIActive("LoadingPicture-Image", true);
                SceneManager.LoadSceneAsync("BattleField");
                break;
            case "难度":
                ChangeDifficulty();
                break;
            case "更换涂装":
                ChangeMaterial();
                break;
            case "返回":
                SwitchPanel(SwitchPanelType.Both, SwitchPanelType.Right);
                mSequence.InsertCallback(1f, () =>
                {
                    SetGoActive(mSUIGoDict["RightPage-0"], true);
                    SetGoActive(mSUIGoDict["RightPage-1"], false);
                    SetGoActive(mSUIGoDict["RightPage-2"], false);
                });
                break;
            case "退出":
                Application.Quit();
                break;
        }
    }

    /// <summary>
    /// 设置左侧面板标题和内容
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="content">内容</param>
    private void SetLeftPanel(string title, string content)
    {
        SetSUIText("LeftPage-Title-Text", title);
        SetSUIText("LeftPage-Content-Text", content);
    }

    /// <summary>
    /// 切换面板动画
    /// </summary>
    /// <param name="switchType_out">退出动画</param>
    /// <param name="switchType_back">进入动画</param>
    private void SwitchPanel(SwitchPanelType switchType_out, SwitchPanelType switchType_back)
    {
        mSequence = DOTween.Sequence();

        switch (switchType_out)
        {
            case SwitchPanelType.Left:
                mSequence.Insert(0f, mSUIGoDict["LeftPanel"].transform.DOMove(mSPointDict["Point-0-LeftPanel"].transform.position, 1f));
                break;
            case SwitchPanelType.Right:
                mSequence.Insert(0f, mSUIGoDict["RightPanel"].transform.DOMove(mSPointDict["Point-0-RightPanel"].transform.position, 1f));
                break;
            case SwitchPanelType.Both:
                mSequence.Insert(0f, mSUIGoDict["RightPanel"].transform.DOMove(mSPointDict["Point-0-RightPanel"].transform.position, 1f));
                mSequence.Insert(0f, mSUIGoDict["LeftPanel"].transform.DOMove(mSPointDict["Point-0-LeftPanel"].transform.position, 1f));
                break;
        }

        float waitTime = switchType_out == SwitchPanelType.None ? 0 : 1.1f;

        switch (switchType_back)
        {
            case SwitchPanelType.Left:
                mSequence.Insert(waitTime, mSUIGoDict["LeftPanel"].transform.DOMove(mSPointDict["Point-1-LeftPanel"].transform.position, 1f));
                break;
            case SwitchPanelType.Right:
                mSequence.Insert(waitTime, mSUIGoDict["RightPanel"].transform.DOMove(mSPointDict["Point-1-RightPanel"].transform.position, 1f));
                break;
            case SwitchPanelType.Both:
                mSequence.Insert(waitTime, mSUIGoDict["RightPanel"].transform.DOMove(mSPointDict["Point-1-RightPanel"].transform.position, 1f));
                mSequence.Insert(waitTime, mSUIGoDict["LeftPanel"].transform.DOMove(mSPointDict["Point-1-LeftPanel"].transform.position, 1f));
                break;
        }
    }

    /// <summary>
    /// 切换难度
    /// </summary>
    /// <param name="difficulty"></param>
    private void ChangeDifficulty(Difficulty difficulty)
    {
        GameSetting.Self.mDifficulty = difficulty;

        switch (difficulty)
        {
            case Difficulty.Easy:
                mSUITextDict["难度-Text"].text = "难度：简单";
                break;
            case Difficulty.Hard:
                mSUITextDict["难度-Text"].text = "难度：困难";
                break;
        }
    }

    /// <summary>
    /// 切换难度
    /// </summary>
    private void ChangeDifficulty()
    {
        switch (GameSetting.Self.mDifficulty)
        {
            case Difficulty.Easy:
                GameSetting.Self.mDifficulty = Difficulty.Hard;
                mSUITextDict["难度-Text"].text = "难度：困难";
                break;
            case Difficulty.Hard:
                GameSetting.Self.mDifficulty = Difficulty.Easy;
                mSUITextDict["难度-Text"].text = "难度：简单";
                break;
        }
    }

    /// <summary>
    /// 更换机身材质
    /// </summary>
    private void ChangeMaterial()
    {
        GameSetting.Self.mMaterialIndex = GameSetting.Self.mMaterialIndex == 2 ? 0 : GameSetting.Self.mMaterialIndex + 1;

        ChangeMaterial(GameSetting.Self.mMaterialIndex);
    }

    /// <summary>
    /// 更换机身材质
    /// </summary>
    private void ChangeMaterial(int index)
    {
        mWMDMRDict["机身"].material = mPlaneMat[index];
        mWMDMRDict["侦测仪"].material = mPlaneMat[index];
        mWMDMRDict["机炮"].material = mPlaneMat[index];
        mWMDMRDict["旋翼"].material = mPlaneMat[index];
        mWMDMRDict["尾桨"].material = mPlaneMat[index];
    }
}
