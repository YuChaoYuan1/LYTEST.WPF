namespace LYTest.Core.Model.DnbModel.DnbInfo
{
    public class MeterRegister : MeterBase
    {
        /// <summary>
        /// ����
        /// </summary>
        public string FL { get; set; }
        /// <summary>
        /// ����ǰ�ܵ���
        /// </summary>
        public string TopTotalPower { get; set; }

        /// <summary>
        /// ������ܵ���
        /// </summary>
        public string EndTotalPower { get; set; }
        /// <summary>
        /// �ܵ�����ֵ
        /// </summary>
        public string TotalPowerD { get; set; }

        /// <summary>
        /// ����ǰ���ʵ���
        /// </summary>
        public string TopFLPower { get; set; }

        /// <summary>
        /// �������ʵ���
        /// </summary>
        public string EndFLPower { get; set; }

        /// <summary>
        /// ���ʵ�����ֵ
        /// </summary>
        public string FLPowerD { get; set; }

        /// <summary>
        /// ������
        /// </summary>
        public string CombError { get; set; }


        /// <summary>
        /// ����
        /// </summary>
        public string Result { get; set; }

    }
}
