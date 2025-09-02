using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace LYTest.ViewModel.Debug
{
    /// <summary>
    /// 谐波调试
    /// </summary>
    public class Debug_HarmonicViewModel : ViewModelBase
    {
        public Debug_HarmonicViewModel()
        {
            HarmonicTypeList.Clear();
            HarmonicTypeList.Add("常规谐波");
            HarmonicTypeList.Add("方顶波");
            HarmonicTypeList.Add("尖顶波");
            HarmonicTypeList.Add("次谐波");
            HarmonicTypeList.Add("奇谐波");
            HarmonicTypeList.Add("偶次谐波");
            HarmonicType = "常规谐波";
        }

        #region 属性

        public Dictionary<string, Dictionary<int, HarmonicData>> harmonicData = new Dictionary<string, Dictionary<int, HarmonicData>>();


        private int harmonicNumber = 5;

        /// <summary>
        /// 第几次谐波
        /// </summary>
        public int HarmonicNumber
        {
            get { return harmonicNumber; }
            set { SetPropertyValue(value, ref harmonicNumber, "HarmonicNumber"); }
        }


        private ObservableCollection<string> harmonicTypeList = new ObservableCollection<string>();
        /// <summary>
        ///谐波类型
        public ObservableCollection<string> HarmonicTypeList
        {
            get { return harmonicTypeList; }
            set { SetPropertyValue(value, ref harmonicTypeList, "ProtList"); }
        }

        private bool isEnableHarmonic = true;

        /// <summary>
        /// 开启或关闭谐波
        /// </summary>
        public bool IsEnableHarmonic
        {
            get { return isEnableHarmonic; }
            set { SetPropertyValue(value, ref isEnableHarmonic, "IsEnableHarmonic"); }
        }

        #region 是否选中
        private bool isReadStaData = false;
        /// <summary>
        /// 读取标准表谐波信息
        /// </summary>
        public bool IsReadStaData
        {
            get { return isReadStaData; }
            set
            {
                SetPropertyValue(value, ref isReadStaData, "IsReadStaData");
            }
        }


        private bool isUA = true;
        public bool IsUA
        {
            get { return isUA; }
            set
            {
                SetPropertyValue(value, ref isUA, "IsUA");
            }
        }
        private bool isUB = true;
        public bool IsUB
        {
            get { return isUB; }
            set
            {
                SetPropertyValue(value, ref isUB, "IsUB");
            }
        }
        private bool isUC = true;
        public bool IsUC
        {
            get { return isUC; }
            set
            {
                SetPropertyValue(value, ref isUC, "IsUC");
            }
        }
        private bool isIA = true;
        public bool IsIA
        {
            get { return isIA; }
            set
            {
                SetPropertyValue(value, ref isIA, "IsIA");
            }
        }
        private bool isIB = true;
        public bool IsIB
        {
            get { return isIB; }
            set
            {
                SetPropertyValue(value, ref isIB, "IsIB");
            }
        }

        private bool isIC = true;
        public bool IsIC
        {
            get { return isIC; }
            set
            {
                SetPropertyValue(value, ref isIC, "IsIC");
            }
        }


        #endregion



        private float harmonicContent;

        /// <summary>
        /// 谐波含量
        /// </summary>
        public float HarmonicContent
        {
            get { return harmonicContent; }
            set { SetPropertyValue(value, ref harmonicContent, "HarmonicContent"); }
        }
        private float harmonicPhase;

        /// <summary>
        /// 谐波相位
        /// </summary>
        public float HarmonicPhase
        {
            get { return harmonicPhase; }
            set { SetPropertyValue(value, ref harmonicPhase, "HarmonicPhase"); }
        }

        private string harmonicType;
        /// <summary>
        ///当前选中谐波类型
        public string HarmonicType
        {
            get { return harmonicType; }
            set
            {
                SetPropertyValue(value, ref harmonicType, "HarmonicType");
                switch (HarmonicType)
                {
                    case "常规谐波":
                        //IsEnableHarmonic_Visible = true;
                        HarmonicContent_Visible = true;
                        HarmonicPhase_Visible = true;
                        HarmonicNumbere_Visible = true;
                        break;
                    case "方顶波":
                    case "尖顶波":
                    case "次谐波":
                    case "奇谐波":
                    case "偶次谐波":
                        //IsEnableHarmonic_Visible = false;
                        HarmonicContent_Visible = false;
                        HarmonicPhase_Visible = false;
                        HarmonicNumbere_Visible = false;
                        break;
                    default:
                        break;
                }
            }
        }

        #region 显示和隐藏
        private bool isEnableHarmonic_Visible;

        /// <summary>
        /// 是否显示开启或关闭谐波
        /// </summary>
        public bool IsEnableHarmonic_Visible
        {
            get { return isEnableHarmonic_Visible; }
            set { SetPropertyValue(value, ref isEnableHarmonic_Visible, "IsEnableHarmonic_Visible"); }
        }
        private bool harmonicContent_Visible;

        /// <summary>
        /// 是否显示谐波含量
        /// </summary>
        public bool HarmonicContent_Visible
        {
            get { return harmonicContent_Visible; }
            set { SetPropertyValue(value, ref harmonicContent_Visible, "HarmonicContent_Visible"); }
        }
        private bool harmonicPhase_Visible;

        /// <summary>
        /// 是否显示谐波相位
        /// </summary>
        public bool HarmonicPhase_Visible
        {
            get { return harmonicPhase_Visible; }
            set { SetPropertyValue(value, ref harmonicPhase_Visible, "HarmonicPhase_Visible"); }
        }
        private bool harmonicNumber_Visible;

        /// <summary>
        /// 是否显示第几次谐波
        /// </summary>
        public bool HarmonicNumbere_Visible
        {
            get { return harmonicNumber_Visible; }
            set { SetPropertyValue(value, ref harmonicNumber_Visible, "HarmonicNumbere_Visible"); }
        }

        #endregion


        #endregion


        #region 方法

        /// <summary>
        /// 设置谐波
        /// </summary>
        public void SetHarmonic()
        {
            try
            {
                string ua = IsUA ? "1" : "0";
                string ub = IsUB ? "1" : "0";
                string uc = IsUC ? "1" : "0";
                string ia = IsIA ? "1" : "0";
                string ib = IsIB ? "1" : "0";
                string ic = IsIC ? "1" : "0";
                switch (HarmonicType)
                {
                    case "常规谐波":

                        float[] temHarmonicContent = new float[60];
                        float[] temHarmonicPhase = new float[60];
                        if (IsEnableHarmonic)
                        {
                            temHarmonicContent[HarmonicNumber - 2] = HarmonicContent;
                            temHarmonicPhase[HarmonicNumber - 2] = HarmonicPhase;
                            EquipmentData.DeviceManager.ZH3001SetPowerGetHarmonic(ua, ub, uc, ia, ib, ic, temHarmonicContent, temHarmonicPhase, IsEnableHarmonic);
                        }
                        else
                        {
                            EquipmentData.DeviceManager.ZH3001SetPowerGetHarmonic("1", "1", "1", "1", "1", "1", temHarmonicContent, temHarmonicPhase, IsEnableHarmonic);
                        }
                        break;
                    case "方顶波":
                    case "尖顶波":
                    case "次谐波":
                    case "奇谐波":
                        int type = 0;
                        if (IsEnableHarmonic)
                        {
                            switch (HarmonicType)
                            {
                                case "方顶波":
                                    type = 1;
                                    break;
                                case "尖顶波":
                                    type = 2;
                                    break;
                                case "次谐波":
                                    type = 3;
                                    break;
                                case "奇谐波":
                                    type = 4;
                                    break;
                                default:
                                    break;
                            }
                            EquipmentData.DeviceManager.ZH3001SetPowerHarmonic(ua, ub, uc, ia, ib, ic, type);
                        }
                        else
                        {
                            EquipmentData.DeviceManager.ZH3001SetPowerHarmonic("1", "1", "1", "1", "1", "1", 0);
                        }
                        break;
                    case "偶次谐波":
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Utility.Log.LogManager.AddMessage(ex.ToString(), Utility.Log.EnumLogSource.用户操作日志, Utility.Log.EnumLevel.Error);
            }
        }
        #endregion
    }

    public class HarmonicData : ViewModelBase
    {
        private float harmonicContent;

        /// <summary>
        /// 谐波含量
        /// </summary>
        public float HarmonicContent
        {
            get { return harmonicContent; }
            set { SetPropertyValue(value, ref harmonicContent, "HarmonicContent"); }
        }
        private float harmonicPhase;

        /// <summary>
        /// 谐波相位
        /// </summary>
        public float HarmonicPhase
        {
            get { return harmonicPhase; }
            set { SetPropertyValue(value, ref harmonicPhase, "HarmonicPhase"); }
        }
    }
}
