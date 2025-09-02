using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IConnection
{
    public class PortInfo
    {
        public int Port = 0;
        /// <summary>
        /// true UDP,false COM
        /// </summary>
        public bool IsUDP = true;
        /// <summary>
        /// IP
        /// </summary>
        public string IP = "";
        /// <summary>
        /// 波特率
        /// </summary>
        public string Setting = "9600,e,8,1";
        /// <summary>
        /// 0无，1有
        /// </summary>
        public int Exist = 0;
    }
}
