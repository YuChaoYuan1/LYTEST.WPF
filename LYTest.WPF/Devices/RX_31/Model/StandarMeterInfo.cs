using ZH.Rx_31.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZH.Rx_31.Model
{
    public class StandarMeterInfo
    {

        public WireMode Clfs { get; set; }       //接线方式	
        public byte Flip_ABC { get; set; }       //相位开关控制	
        public float Freq { get; set; }          //频率	

        public byte Scale_Ua { get; set; }        //Ua档位 	
        public byte Scale_Ub { get; set; }        //Ub档位 	
        public byte Scale_Uc { get; set; }        //Uc档位 	
        public byte Scale_Ia { get; set; }       //a档位 	
        public byte Scale_Ib { get; set; }       //Ib档位 	
        public byte Scale_Ic { get; set; }        //Ic档位 	

        public float Ua { get; set; }             //UA 
        public float Ia { get; set; }             //Ia 	
        public float Ub { get; set; }             //UB  	
        public float Ib { get; set; }             //Ib 	
        public float Uc { get; set; }             //UC 	
        public float Ic { get; set; }             //Ic 	

        public float Phi_Ua { get; set; }         //Ua相位 	
        public float Phi_Ia { get; set; }         //Ia相位 	
        public float Phi_Ub { get; set; }         //UB相位 	
        public float Phi_Ib { get; set; }         //Ib相位 	
        public float Phi_Uc { get; set; }        //UC相位 	
        public float Phi_Ic { get; set; }        //Ic相位 	

        public float Pa { get; set; } //A相有功功率 	
        public float Pb { get; set; } //B相有功功率	
        public float Pc { get; set; } //C相有功功率	

        public float Qa { get; set; } //A相无功功率	
        public float Qb { get; set; } //B相无功功率	
        public float Qc { get; set; } //C相无功功率	

        public float Sa { get; set; } //A相视在功率	
        public float Sb { get; set; } //B相视在功率	
        public float Sc { get; set; } //C相视在功率	

        public float P { get; set; } //总有功功率	
        public float Q { get; set; } //总无功功率	

        public float S { get; set; } //总视在功功率	

        public float COS { get; set; } //有功功率因数	
        public float SIN { get; set; } //无功功率因数	

    }
}
