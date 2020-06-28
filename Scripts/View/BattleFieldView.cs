using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WeFrame;
using WeFrame.Common;

public class BattleFieldView : BaseView
{
    private DataCenter mDataCenter = new DataCenter();

    /// <summary>玩家单位</summary>
    private PlayerUnit mPlayerUnit;
    /// <summary>敌军单位集合</summary>
    private List<AIUnit> mEnemyList = new List<AIUnit>();

    /// <summary>敌军最大数量</summary>
    private int mEnemyCount_Max = 6;

    private List<GameObject> mMarkList = new List<GameObject>();
    private List<Slider> mSliderList = new List<Slider>();

    private GameObject mParticlePrefab_Hit;
    private GameObject mParticlePrefab_Destroy;

    private bool mIsLock = false;

    private int targetIndex = 0;
    public int TargetIndex
    {
        get
        {
            if (targetIndex >= mEnemyList.Count)
                targetIndex = 0;

            mPlayerUnit.mTarget = mEnemyList[targetIndex];

            return targetIndex;
        }
        set
        {
            if (value >= mEnemyList.Count)
                targetIndex = 0;
            else
                targetIndex = value;

            mPlayerUnit.mTarget = mEnemyList[targetIndex];
        }
    }

    protected override void Init()
    {
        base.Init();

        LoginSUIClick("菜单-Button", SwitchPanel);
        LoginSUIClick("返回首页-Button", ReturnHome);
        LoginSUIClick("重新开始-Button", Restart);
        LoginSUIClick("退出-Button", ReturnHome);

        ChangeVolume(GameSetting.Self.mVolume);
        mSUISlidDict["音量-Slider"].value = GameSetting.Self.mVolume;
        mSUISlidDict["音量-Slider"].onValueChanged.AddListener(ChangeVolume);

        mPlayerUnit = mWMDGoDict["WZ10"].GetComponent<PlayerUnit>();

        InitEnemy();

        mParticlePrefab_Hit = Resources.Load<GameObject>("Prefabs/FireEffects");
        mParticlePrefab_Destroy = Resources.Load<GameObject>("MilitaryBase/Prefab/Military1/Particles/Smoke2");

        StartCoroutine(GetEnemyPosition());

        Material material = Resources.Load<Material>("Materials/body_mat_" + GameSetting.Self.mMaterialIndex.ToString());
        mWMDMRDict["机身"].material = material;
        mWMDMRDict["侦测仪"].material = material;
        mWMDMRDict["机炮"].material = material;
        mWMDMRDict["旋翼"].material = material;
        mWMDMRDict["尾桨"].material = material;
    }

    protected override void Refresh()
    {
        base.Refresh();

        SwitchPanel();

        if (mPlayerUnit != null)
        {
            LockEnemy();
            RefreshHDR();
        }
    }

    /// <summary>
    /// 实例化敌军
    /// </summary>
    private void InitEnemy()
    {
        GameObject markPrefab = Resources.Load<GameObject>("Prefabs/EnemyMark");

        float health = GameSetting.Self.mDifficulty == Difficulty.Easy ? 100 : 200;

        switch (GameSetting.Self.mMode)
        {
            case Mode.Training:
                GameObject targetPrefab = Resources.Load<GameObject>("Prefabs/靶");
                for (int i = 0; i < mEnemyCount_Max; i++)
                {
                    Transform tf = mWMDPointDict["Point-" + i.ToString() + "-MarkSpawner"].transform;
                    GameObject go = Instantiate(targetPrefab, tf.position, tf.rotation, mWMDGoDict["EnemyUnit"].transform);
                    go.name = (i + 1).ToString() + "号靶";

                    AIUnit unit = go.GetComponent<AIUnit>();
                    mEnemyList.Add(unit);
                    unit.mHealth_Max = health;

                    GameObject mark = Instantiate(markPrefab, mSUIGoDict["Mark-Root"].transform);
                    mMarkList.Add(mark);
                    mSliderList.Add(mark.GetComponentInChildren<Slider>());
                }
                break;
            case Mode.ActualCombat:
                GameObject tankPrefab = Resources.Load<GameObject>("Prefabs/艾布拉姆斯");
                for (int i = 0; i < mEnemyCount_Max; i++)
                {
                    Transform tf = mWMDPointDict["Point-" + i.ToString() + "-TankSpawner"].transform;
                    GameObject go = Instantiate(tankPrefab, tf.position, tf.rotation, mWMDGoDict["EnemyUnit"].transform);
                    go.name = (i + 1).ToString() + "号坦克";

                    AIUnit unit = go.GetComponent<AIUnit>();
                    mEnemyList.Add(unit);
                    unit.mHealth_Max = health;
                    unit.mIsAllowedAttack = true;
                    unit.mTarget = mPlayerUnit.transform;

                    GameObject mark = Instantiate(markPrefab, mSUIGoDict["Mark-Root"].transform);
                    mMarkList.Add(mark);
                    mSliderList.Add(mark.GetComponentInChildren<Slider>());
                }
                break;
        }
    }

    /// <summary>
    /// 刷新HDR
    /// </summary>
    private void RefreshHDR()
    {
        mSUITextDict["生命值-Text"].text = "损伤程度:" + (int)(mPlayerUnit.mHealth_Cur / mPlayerUnit.mHealth_Max * 100) + "%";
        mSUIImgDict["生命值-Image"].fillAmount = mPlayerUnit.mHealth_Cur / mPlayerUnit.mHealth_Max;

        mSUITextDict["速度仪-Text"].text = "速度:" + (int)mPlayerUnit.Speed * 6 + "km/h";
        mSUITextDict["高度仪-Text"].text = "高度:" + (int)mPlayerUnit.Height + "m";
        mSUITextDict["坐标-Text"].text = (int)mPlayerUnit.transform.position.x + "," + (int)mPlayerUnit.transform.position.z;

        mSUITextDict["测距-Text"].text = (int)mPlayerUnit.SightDistance + "m";
        mSUITextDict["弹药量-Text"].text = "机炮子弹:" + mPlayerUnit.mAmmunitionArr_Cur[0] + "发\n" +
                                          "火箭弹:" + mPlayerUnit.mAmmunitionArr_Cur[1] + "枚\n" +
                                          "跟踪弹:" + mPlayerUnit.mAmmunitionArr_Cur[2] + "枚\n";

        mSUIGoDict["准星-Image"].transform.position = Input.mousePosition;
        mSUIGoDict["速度仪-Pointer"].transform.eulerAngles = Vector3.forward * (mPlayerUnit.Speed * (-200 / mPlayerUnit.mVerticalSpeed_Limit) + 60f);
        mSUIGoDict["高度仪-Pointer"].transform.eulerAngles = Vector3.forward * (mPlayerUnit.Height * -0.77f + 60f);

        for (int i = 0; i < mEnemyList.Count; i++)
        {
            if (IsInView(mEnemyList[i].transform.position))
            {
                if (!mMarkList[i].activeSelf)
                {
                    SetGoActive(mMarkList[i], true);
                }
                mMarkList[i].transform.position = Camera.main.WorldToScreenPoint(mEnemyList[i].transform.position + Vector3.up * 3);
                mSliderList[i].value = mEnemyList[i].mHealth_Cur / mEnemyList[i].mHealth_Max;
            }
            else
            {
                SetGoActive(mMarkList[i], false);
            }
        }
    }

    /// <summary>
    /// 锁敌逻辑
    /// </summary>
    private void LockEnemy()
    {
        if (Input.GetButtonDown("Lock"))
        {
            mIsLock = !mIsLock;

            if (mIsLock)
                mPlayerUnit.mTarget = mEnemyList[TargetIndex];
        }

        if (mIsLock)
        {
            if (Input.GetButtonDown("SwitchTarget"))
                TargetIndex++;

            if (IsInView(mEnemyList[TargetIndex].transform.position))
            {
                if (!mSUIGoDict["锁定-Image"].activeSelf)
                    SetGoActive(mSUIGoDict["锁定-Image"], true);
                mSUIGoDict["锁定-Image"].transform.position = mMarkList[TargetIndex].transform.position;
            }
            else if (mSUIGoDict["锁定-Image"].activeSelf)
            {
                SetGoActive(mSUIGoDict["锁定-Image"], false);
            }
        }
        else
        {
            if (mSUIGoDict["锁定-Image"].activeSelf)
            {
                SetGoActive(mSUIGoDict["锁定-Image"], false);
                mPlayerUnit.mTarget = null;
            }
        }
    }

    /// <summary>
    /// 游戏结束结算页面
    /// </summary>
    private void GameOver()
    {
        float[] hitRate = new float[4];

        for (int i = 0; i < 4; i++)
        {
            hitRate[i] = mDataCenter.mAttackCount[i] == 0 ? 0 : (float)mDataCenter.mHitCount[i] / (float)mDataCenter.mAttackCount[i] * 100;
        }

        float destroyRate = (1 - (float)mEnemyList.Count / (float)mEnemyCount_Max) * 100;

        mSUITextDict["结算页面-Text"].text = "23mm自动航炮\n载弹量：" + mPlayerUnit.mAmmunitionArr_Max[0].ToString("D3") + "发     " +
                                            "消耗弹量" + (mPlayerUnit.mAmmunitionArr_Max[0] - mPlayerUnit.mAmmunitionArr_Cur[0]).ToString("D3") + "发     " +
                                            "剩余弹量：" + mPlayerUnit.mAmmunitionArr_Cur[0].ToString("D3") + "发     " +
                                            "命中次数：" + mDataCenter.mHitCount[0].ToString("D3") + "发     " +
                                            "命中率：" + hitRate[0].ToString("F2") + "%\n" +
                                            "57mm火箭巢\n载弹量：" + mPlayerUnit.mAmmunitionArr_Max[1].ToString("D3") + "枚     " +
                                            "消耗弹量" + (mPlayerUnit.mAmmunitionArr_Max[1] - mPlayerUnit.mAmmunitionArr_Cur[1]).ToString("D3") + "枚     " +
                                            "剩余弹量：" + mPlayerUnit.mAmmunitionArr_Cur[1].ToString("D3") + "枚     " +
                                            "命中次数：" + mDataCenter.mHitCount[1].ToString("D3") + "枚     " +
                                            "命中率：" + hitRate[1].ToString("F2") + "%\n" +
                                            "天燕-90导弹\n载弹量：" + mPlayerUnit.mAmmunitionArr_Max[2].ToString("D3") + "枚     " +
                                            "消耗弹量" + (mPlayerUnit.mAmmunitionArr_Max[2] - mPlayerUnit.mAmmunitionArr_Cur[2]).ToString("D3") + "枚     " +
                                            "剩余弹量：" + mPlayerUnit.mAmmunitionArr_Cur[2].ToString("D3") + "枚     " +
                                            "命中次数：" + mDataCenter.mHitCount[2].ToString("D3") + "枚     " +
                                            "命中率：" + hitRate[2].ToString("F2") + "%\n\n" +
                                            "敌军数量：" + mEnemyCount_Max.ToString("D3") + "辆     " +
                                            "歼灭数量：" + (mEnemyCount_Max - mEnemyList.Count).ToString("D3") + "辆     " +
                                            "歼灭率：" + destroyRate.ToString("F2") + "%\n" +
                                            "剩余生命值：" + mPlayerUnit.mHealth_Cur + "%     " +
                                            "被击中次数：" + mDataCenter.mHitCount[3] + "次     " +
                                            "被击率：" + hitRate[3].ToString("F2") + "%";

        CActive.SetGo(mSUIGoDict["结算页面-Image"], true);

        mWMDGoDict["Camera-Main"].transform.SetParent(this.transform);
        mWMDGoDict["Camera-Rader"].transform.SetParent(this.transform);
    }

    /// <summary>
    /// 设置子弹父物体
    /// </summary>
    private void SetBulletParent(GameObject go)
    {
        go.transform.SetParent(mWMDGoDict["Bullet-Root"].transform);
    }

    /// <summary>
    /// 当单位发动攻击时
    /// </summary>
    public void OnUnitAttack(BulletType bulletType)
    {
        mDataCenter.mAttackCount[(int)bulletType]++;
    }

    /// <summary>
    /// 当子弹击中任意物体时
    /// </summary>
    private void OnBulletHit(Vector3 point)
    {
        GameObject particle = Instantiate(mParticlePrefab_Hit, point, Quaternion.identity, mWMDGoDict["Particle-Root"].transform);
        Destroy(particle, 2);
    }

    /// <summary>
    /// 当单位被击中时
    /// </summary>
    private void OnUnitHited(Bullet bullet)
    {
        mDataCenter.mHitCount[(int)bullet.mBulletType]++;
        bullet.mTarget.ReceiveDamage(bullet.mDamage);
    }

    /// <summary>
    /// 当任意单位被摧毁时
    /// </summary>
    /// <param name="unit">被摧毁的单位</param>
    private void OnUnitDestroy(BaseUnit unit)
    {
        Type type = unit.GetType();
        GameObject particle = Instantiate(mParticlePrefab_Destroy, unit.transform.position, Quaternion.identity, mWMDGoDict["Particle-Root"].transform);

        if (type == typeof(PlayerUnit))
        {
            GameOver();
        }
        else if (type == typeof(AIUnit))
        {
            mEnemyList.Remove((AIUnit)unit);
            if (mEnemyList.Count <= 0)
            {
                GameOver();
            }
        }

        Destroy(mMarkList[0]);
        mMarkList.RemoveAt(0);
        mSliderList.RemoveAt(0);
    }

    private IEnumerator GetEnemyPosition()
    {
        while (mEnemyList.Count > 0 && mPlayerUnit != null)
        {
            mSUITextDict["信息栏-Text"].text = "";
            foreach (AIUnit enemy in mEnemyList)
            {
                mSUITextDict["信息栏-Text"].text += "[" + DateTime.Now.Hour.ToString("D2") + ":" +
                                                    DateTime.Now.Minute.ToString("D2") + ":" +
                                                    DateTime.Now.Second.ToString("D2") + "]" +
                                                    "  目标:" + enemy.name + "," +
                                                    GetEnemyDirection(mPlayerUnit.transform, enemy.transform) + "点方向，" +
                                                    "距离" + (int)(enemy.transform.position - mPlayerUnit.transform.position).magnitude + "米，" +
                                                    "坐标" + enemy.transform.position + "\n";
            }
            yield return new WaitForSeconds(10.0f);
        }
    }

    /// <summary>
    /// 获取目标方向
    /// </summary>
    /// <param name="player">玩家单位</param>
    /// <param name="target">目标</param>
    /// <returns></returns>
    private int GetEnemyDirection(Transform player, Transform target)
    {
        //Vector3 dir = new Vector3(target.position.x, 0, target.position.z) - new Vector3(player.position.x, 0, player.position.z);
        Vector3 dir = new Vector3(target.position.x - player.position.x, 0, target.position.z - player.position.z);
        float dot = Vector3.Dot(player.right, dir.normalized);
        float angle = Mathf.Acos(Vector3.Dot(player.forward.normalized, dir.normalized)) * Mathf.Rad2Deg;

        if (dot < 0)
            angle = -angle;

        for (int i = -5; i <= 6; i++)
        {
            if (angle >= (i * 30) - 15 && (angle < ((i + 1) * 30) - 15 || i == 6))
            {
                if (i < 1)
                    i += 12;
                return i;
            }
        }
        return 0;
    }

    /// <summary>
    /// 坐标是否在相机视野中
    /// </summary>
    private bool IsInView(Vector3 worldPos)
    {
        Vector2 viewPos = Camera.main.WorldToViewportPoint(worldPos);
        Vector3 dir = (worldPos - Camera.main.transform.position).normalized;
        float dot = Vector3.Dot(Camera.main.transform.forward, dir);

        if (dot > 0 && viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1)
            return true;
        else
            return false;
    }

    private void SwitchPanel()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            SetSUIActive("菜单-Panel", !mSUIGoDict["菜单-Panel"].activeSelf);
        }
    }

    private void ChangeVolume(float value)
    {
        GameSetting.Self.mVolume = value;
        AudioListener.volume = value;
    }

    private void Restart()
    {
        SceneManager.LoadScene("BattleField");
    }

    private void ReturnHome()
    {
        SceneManager.LoadScene("Home");
    }
}