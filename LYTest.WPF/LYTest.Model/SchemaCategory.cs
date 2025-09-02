using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LYTest.Model
{
    public class SchemaCategory
    {
        public SchemaCategory(string name, string paraNo, byte rights)
        {
            Name = name;
            ParaNo = paraNo;
            Rights = rights;
        }
        public string Name { get; }
        public string ParaNo { get; }
        /// <summary>
        /// 0x07->0b00000111->bit0全检bit1抽检bit2全性能
        /// </summary>
        public byte Rights { get; }
    }
}
