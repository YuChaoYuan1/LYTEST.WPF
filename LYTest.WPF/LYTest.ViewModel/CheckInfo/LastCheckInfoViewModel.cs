using LYTest.DAL;
using LYTest.DAL.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LYTest.MeterProtocol.Encryption;

namespace LYTest.ViewModel.CheckInfo
{

    /// <summary>
    /// 程序退出时的状态
    /// 在程序开始运行时加载状态
    /// </summary>
    public class LastCheckInfoViewModel : ViewModelBase
    {
        private string equipmentNo;
        /// 台体编号
        /// <summary>
        /// 台体编号
        /// </summary>
        public string EquipmentNo
        {
            get { return equipmentNo; }
            set
            {
                SetPropertyValue(value, ref equipmentNo, "EquipmentNo");
            }
        }
        private int schemaId = 1;
        /// 方案编号
        /// <summary>
        /// 方案编号
        /// </summary>
        public int SchemaId
        {
            get { return schemaId; }
            set
            {
                if (value != schemaId)
                {
                    SetPropertyValue(value, ref schemaId, "SchemaId");
                    SaveCurrentCheckInfo();
                }
            }
        }
        private int checkIndex = -1;
        /// 检定点序号
        /// <summary>
        /// 检定点序号
        /// </summary>
        public int CheckIndex
        {
            get { return checkIndex; }
            set
            {
                if (value != checkIndex)
                {
                    SetPropertyValue(value, ref checkIndex, "CheckIndex");
                    SaveCurrentCheckInfo();
                }
            }
        }
        private string protectedVoltage = "100V";
        /// 保护电压
        /// <summary>
        /// 保护电压
        /// </summary>
        public string ProtectedVoltage
        {
            get { return protectedVoltage; }
            set
            {
                if (value != protectedVoltage)
                {
                    SetPropertyValue(value, ref protectedVoltage, "ProtectedVoltage");
                    SaveCurrentCheckInfo();
                }
            }
        }
        public float ProtectedMaxVoltage
        {
            get
            {
                return float.Parse(ProtectedVoltage.Trim('V'));
            }
        }
        public float ProtectedMaxCurrent
        {
            get
            {
                return float.Parse(ProtectedCurrent.Trim('A'));
            }
        }

        private string protectedCurrent = "6A";
        /// 保护电流
        /// <summary>
        /// 保护电流
        /// </summary>
        public string ProtectedCurrent
        {
            get { return protectedCurrent; }
            set
            {
                if (value != protectedCurrent)
                {
                    SetPropertyValue(value, ref protectedCurrent, "ProtectedCurrent");
                    SaveCurrentCheckInfo();
                }
            }
        }
        private string testPerson;
        /// 检验员
        /// <summary>
        /// 检验员
        /// </summary>
        public string TestPerson
        {
            get
            {
                if (User.UserViewModel.Instance.CurrentUser == null)
                {
                    return testPerson;
                }
                return User.UserViewModel.Instance.CurrentUser.GetProperty("USER_NAME") as string;
            }
        }
        private string auditPerson;
        /// 核验员
        /// <summary>
        /// 核验员
        /// </summary>
        public string AuditPerson
        {
            get { return auditPerson; }
            set
            {
                if (value != auditPerson)
                {
                    SetPropertyValue(value, ref auditPerson, "AuditPerson");
                    SaveCurrentCheckInfo();
                }
            }
        }
        /// 加载数据库存储的最后一次检定信息
        /// <summary>
        /// 加载数据库存储的最后一次检定信息
        /// </summary>
        public void LoadLastCheckInfo()
        {
            List<DynamicModel> models = DALManager.ApplicationDbDal.GetList(EnumAppDbTable.T_LAST_INFO.ToString(), string.Format("LAST_DEVICE_ID='{0}'", EquipmentData.Equipment.ID));
            if (models == null || models.Count == 0)
            {
                checkIndex = -1;
                schemaId = 1;
                protectedCurrent = "100A";
                protectedVoltage = "264V";
                auditPerson = "admin";
                testPerson = "admin";
                equipmentNo = "001";
                SaveCurrentCheckInfo();
            }
            else
            {
                DynamicModel model = models[0];
                equipmentNo = model.GetProperty("LAST_DEVICE_ID") as string;                        //台体编号
                int.TryParse(model.GetProperty("LAST_SCHEMA_ID") as string, out schemaId);          //方案编号
                int.TryParse(model.GetProperty("LAST_CHECK_INDEX") as string, out checkIndex);      //检定编号
                protectedVoltage = model.GetProperty("LAST_PROTECT_U") as string;                   //保护电压
                protectedCurrent = model.GetProperty("LAST_PROTECT_I") as string;                   //保护电流
                auditPerson = model.GetProperty("LAST_AUDIT_PERSON") as string;                     //核验员
                testPerson = model.GetProperty("LAST_TEST_PERSON") as string;                       //检验员

            }
        }





        /// 保存当前检定信息
        /// <summary>
        /// 保存当前检定信息
        /// </summary>
        public void SaveCurrentCheckInfo()
        {
            DynamicModel model = new DynamicModel();
            model.SetProperty("LAST_DEVICE_ID", EquipmentData.Equipment.ID);
            if (EquipmentData.Schema.SchemaId == 0)
                model.SetProperty("LAST_SCHEMA_ID", schemaId);
            else
                model.SetProperty("LAST_SCHEMA_ID", EquipmentData.Schema.SchemaId);

            if (EquipmentData.Controller.Index == -1)
                model.SetProperty("LAST_CHECK_INDEX", checkIndex);
            else
                model.SetProperty("LAST_CHECK_INDEX", EquipmentData.Controller.Index);

            model.SetProperty("LAST_PROTECT_U", ProtectedVoltage);
            model.SetProperty("LAST_PROTECT_I", ProtectedCurrent);
            model.SetProperty("LAST_TEST_PERSON", TestPerson);
            model.SetProperty("LAST_AUDIT_PERSON", AuditPerson);
 
            if (DALManager.ApplicationDbDal.GetCount(EnumAppDbTable.T_LAST_INFO.ToString(), string.Format("LAST_DEVICE_ID='{0}'", equipmentNo)) == 0)
            {
                DALManager.ApplicationDbDal.Insert(EnumAppDbTable.T_LAST_INFO.ToString(), model);
            }
            else
            {
                DALManager.ApplicationDbDal.Update(EnumAppDbTable.T_LAST_INFO.ToString(), 
               string.Format("LAST_DEVICE_ID='{0}'", equipmentNo), model, new List<string>
              { "LAST_SCHEMA_ID", "LAST_CHECK_INDEX", "LAST_PROTECT_U", "LAST_PROTECT_I",
                   "LAST_TEST_PERSON", "LAST_AUDIT_PERSON" });
            }
        }
    }
}
