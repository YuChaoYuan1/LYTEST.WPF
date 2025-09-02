using LYTest.ViewModel.CheckController;


namespace LYTest.ViewModel.Debug
{
    /// <summary>
    /// 源
    /// </summary>
    public class PowerViewModel : ViewModelBase
    {
        #region 自由升源信息
        private double ua = 57.7;
        public double Ua
        {
            get { return ua; }
            set { SetPropertyValue(value, ref ua, "Ua"); }
        }
        private double phaseUa;
        public double PhaseUa
        {
            get { return phaseUa; }
            set { SetPropertyValue(value, ref phaseUa, "PhaseUa"); }
        }
        private double ub = 57.7;
        public double Ub
        {
            get { return ub; }
            set { SetPropertyValue(value, ref ub, "Ub"); }
        }
        private double phaseUb = 240;
        public double PhaseUb
        {
            get { return phaseUb; }
            set { SetPropertyValue(value, ref phaseUb, "PhaseUb"); }
        }
        private double uc = 57.7;
        public double Uc
        {
            get { return uc; }
            set { SetPropertyValue(value, ref uc, "Uc"); }
        }
        private double phaseUc = 120;
        public double PhaseUc
        {
            get { return phaseUc; }
            set { SetPropertyValue(value, ref phaseUc, "PhaseUc"); }
        }
        private double ia;
        public double Ia
        {
            get { return ia; }
            set { SetPropertyValue(value, ref ia, "Ia"); }
        }
        private double phaseIa;
        public double PhaseIa
        {
            get { return phaseIa; }
            set { SetPropertyValue(value, ref phaseIa, "PhaseIa"); }
        }
        private double ib = 0;
        public double Ib
        {
            get { return ib; }
            set { SetPropertyValue(value, ref ib, "Ib"); }
        }
        private double phaseIb = 240;
        public double PhaseIb
        {
            get { return phaseIb; }
            set { SetPropertyValue(value, ref phaseIb, "PhaseIb"); }
        }
        private double ic = 0;
        public double Ic
        {
            get { return ic; }
            set { SetPropertyValue(value, ref ic, "Ic"); }
        }
        private double phaseIc = 120;
        public double PhaseIc
        {
            get { return phaseIc; }
            set { SetPropertyValue(value, ref phaseIc, "PhaseIc"); }
        }
        private float freq = 50;

        public float Freq
        {
            get { return freq; }
            set { SetPropertyValue(value, ref freq, "Freq"); }
        }


        private string setting = "三相四线";

        public string Setting
        {
            get { return setting; }
            set { SetPropertyValue(value, ref setting, "Setting"); }
        }

        /// <summary>
        /// 升源
        /// </summary>
        public void PowerOnFree()
        {
            //EquipmentData.DeviceManager.PowerOn(Ua, Ub, Uc, Ia, Ib, Ic, PhaseUa, PhaseUb, PhaseUc, PhaseIa, PhaseIb, PhaseIc, Freq);
            int jxfs = 0;
            if (Setting == "三相四线")
            {
                jxfs = 0;
            }
            else if (Setting == "三相三线")
            {
                jxfs = 1;
            }
            else if (Setting == "单相")
            {
                jxfs = 5;
            }

            Utility.TaskManager.AddDeviceAction(() =>
            {
                EquipmentData.DeviceManager.PowerOn(jxfs, Ua, Ub, Uc, Ia, Ib, Ic, PhaseUa, PhaseUb, PhaseUc, PhaseIa, PhaseIb, PhaseIc, Freq, 1);
            });

        }

        /// <summary>
        /// 关源
        /// </summary>
        public void PowerOffFree()
        {
            Utility.TaskManager.AddDeviceAction(() =>
            {
                EquipmentData.DeviceManager.PowerOff();
            });
        }

        #endregion
    }
}
