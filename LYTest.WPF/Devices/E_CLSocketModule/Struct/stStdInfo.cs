using E_CLSocketModule.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace E_CLSocketModule.Struct
{
    /// <summary>
    /// 读取的电源信息
    /// </summary>
    public struct stStdInfo
    {
        public Cus_EmClfs Clfs;  //	 接线方式	
        public byte Flip_ABC;     //   	'相位开关控制	
        public float Freq;//	'频率	
        public byte Scale_Ua;// 	'Ua档位 	
        public byte Scale_Ub;// 	'Ub档位 	
        public byte Scale_Uc;// 	'Uc档位 	
        public byte Scale_Ia;// 	'Ia档位 	
        public byte Scale_Ib;// 	'Ib档位 	
        public byte Scale_Ic;// 	'Ic档位 	
        public float Ua;//	'UA 
        public float Ia;//	'Ia 	
        public float Ub;//	'UB  	
        public float Ib;// Ib 	
        public float Uc;// 	'UC 	
        public float Ic;// 'Ic 	
        public float Phi_Ua;// 	'Ua相位 	
        public float Phi_Ia;// 	'Ia相位 	
        public float Phi_Ub;//	'UB相位 	
        public float Phi_Ib;// 	'Ib相位 	
        public float Phi_Uc;// 	'UC相位 	
        public float Phi_Ic;// 	'Ic相位 	
        public float PhiAngle_A; //A 相相角
        public float PhiAngle_B; //B 相相角
        public float PhiAngle_C; //C 相相角
        public float SAngle;     //功率相角
        public float Pa;// 	'A相有功功率 	
        public float Pb;// 	'B相有功功率	
        public float Pc;// 	'C相有功功率	
        public float Qa;//	'A相无功功率	
        public float Qb;//	'B相无功功率	
        public float Qc;//	'C相无功功率	
        public float Sa;//	'A相视在功率	
        public float Sb;// 	'B相视在功率	
        public float Sc;// 	'C相视在功率	
        public float P;//	总有功功率	
        public float Q;//	总无功功率	
        public float S;//	总视在功功率	
        public float PowerFactor_A; //A相功率因数
        public float PowerFactor_B; //B相功率因数
        public float PowerFactor_C; //C相功率因数
        public float COS;//	有功功率因数	
        public float SIN;//无功功率因数	
    }
}
