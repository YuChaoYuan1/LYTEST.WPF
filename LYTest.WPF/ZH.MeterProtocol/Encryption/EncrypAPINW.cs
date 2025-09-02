using System.Runtime.InteropServices;
using System.Text;

namespace LYTest.MeterProtocol.Encryption
{
    /// <summary>
    /// 南网加密机API
    /// </summary>
    public class EncrypAPINW
    {
        /// <summary>
        /// 连接设备
        /// </summary>
        /// <param name="szType">加密机类型</param>
        /// <param name="cHostIp">加密机IP地址</param>
        /// <param name="uiPort">加密机端口</param>
        /// <param name="timeout">加密机超时时间(单位秒)</param>
        /// <returns>0	成功;其他 失败，见错误代码表</returns>
        [DllImport(@"EncrypNW\MasterStation_HSM.dll", CharSet = CharSet.Ansi)]
        public static extern int OpenDevice(string szType, string cHostIp, int uiPort, int timeout);

        /// <summary>
        /// 关闭设备
        /// </summary>
        /// <returns></returns>
        [DllImport(@"EncrypNW\MasterStation_HSM.dll", CharSet = CharSet.Ansi)]
        public static extern int CloseDevice();

        /// <summary>
        /// 获取随机数以及密文,用于远程身份认证。
        /// </summary>
        /// <param name="Flag">表示电表密钥状态，整型，0: 测试密钥状态；1: 正式密钥状态；</param>
        /// <param name="PutDiv">表示输入的分散因子,字符型,8字节，“0000”+表号；</param>
        /// <param name="OutRand">输出的随机数,字符型,8字节；</param>
        /// <param name="OutEndata">输出的密文,字符型,8字节。</param>
        /// <returns></returns>
        [DllImport(@"EncrypNW\MasterStation_HSM.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int IdentityAuthentication(int Flag, string PutDiv, StringBuilder OutRand, StringBuilder OutEndata);

        /// <summary>
        /// 远程拉闸、合闸、报警等控制数据计算。
        /// </summary>
        /// <param name="Flag">表示电表密钥状态，整型，0: 测试密钥状态；1: 正式密钥状态；</param>
        /// <param name="PutRand">表示输入的随机数,字符型,4字节，电表身份认证成功后返回；</param>
        /// <param name="PutDiv">表示输入的分散因子,字符型,8字节，“0000”+表号；</param>
        /// <param name="PutEsamNo">表示输入的电表安全模块序列号, 字符型, 8字节；</param>
        /// <param name="PutData">表示拉闸、合闸、报警等控制命令明文,字符型,8字节；</param>
        /// <param name="OutEndata">输出的数据长度，字符型，20字节。</param>
        /// <returns></returns>
        [DllImport(@"EncrypNW\MasterStation_HSM.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int UserControl(int Flag, string PutRand, string PutDiv, string PutEsamNo, string PutData, StringBuilder OutEndata);

        /// <summary>
        /// 用于参数信息计算。
        /// </summary>
        /// <param name="Flag">表示电表密钥状态，整型，0: 测试密钥状态；1: 正式密钥状态；</param>
        /// <param name="PutRand">	表示输入的随机数,字符型,4字节，电表身份认证成功后返回；</param>
        /// <param name="PutDiv">表示输入的分散因子,字符型,8字节，“0000”+表号；</param>
        /// <param name="PutApdu">写电表安全模块命令头，字符型，5字节；</param>
        /// <param name="PutData">表示输入的参数信息明文,字符型；</param>
        /// <param name="OutData">输出的数据和MAC。</param>
        /// <returns></returns>
        [DllImport(@"EncrypNW\MasterStation_HSM.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ParameterUpdate(int Flag, string PutRand, string PutDiv, string PutApdu, string PutData, StringBuilder OutData);

        /// <summary>
        /// 用于当前套电价参数计算。
        /// </summary>
        /// <param name="Flag">表示电表密钥状态，整型，0: 测试密钥状态；1: 正式密钥状态；</param>
        /// <param name="PutRand">表示输入的随机数,字符型,4字节，电表身份认证成功后返回；</param>
        /// <param name="PutDiv">表示输入的分散因子,字符型,8字节，“0000”+表号；</param>
        /// <param name="PutApdu">写电表安全模块命令头，字符型，5字节；</param>
        /// <param name="PutData">表示输入的当前套电价参数明文,字符型；</param>
        /// <param name="OutData">输出的数据和MAC。</param>
        /// <returns></returns>
        [DllImport(@"EncrypNW\MasterStation_HSM.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Price1Update(int Flag, string PutRand, string PutDiv, string PutApdu, string PutData, StringBuilder OutData);

        /// <summary>
        /// 用于备用套电价参数计算。
        /// </summary>
        /// <param name="Flag">表示电表密钥状态，整型，0: 测试密钥状态；1: 正式密钥状态；</param>
        /// <param name="PutRand">表示输入的随机数,字符型,4字节，电表身份认证成功后返回；</param>
        /// <param name="PutDiv">表示输入的分散因子,字符型,8字节，“0000”+表号；</param>
        /// <param name="PutApdu">写电表安全模块命令头，字符型，5字节；</param>
        /// <param name="PutData">表示输入的备用套电价参数明文,字符型；</param>
        /// <param name="OutData">输出的数据和MAC。</param>
        /// <returns></returns>
        [DllImport(@"EncrypNW\MasterStation_HSM.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Price2Update(int Flag, string PutRand, string PutDiv, string PutApdu, string PutData, StringBuilder OutData);

        /// <summary>
        /// 用于远程二类参数设置计算。
        /// </summary>
        /// <param name="Flag">表示电表密钥状态，整型，0: 测试密钥状态；1: 正式密钥状态；</param>
        /// <param name="PutRand">表示输入的随机数,字符型,4字节，电表身份认证成功后返回；</param>
        /// <param name="PutDiv">表示输入的分散因子,字符型,8字节，“0000”+表号；</param>
        /// <param name="PutApdu">写电表安全模块的APDU指令头,字符型,5字节；</param>
        /// <param name="PutData">表示输入的二类参数明文,字符型；</param>
        /// <param name="OutEndata">输出的密文和MAC,字符型。</param>
        /// <returns></returns>
        [DllImport(@"EncrypNW\MasterStation_HSM.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ParameterElseUpdate(int Flag, string PutRand, string PutDiv, string PutApdu, string PutData, StringBuilder OutEndata);

        /// <summary>
        /// 用于远程钱包开户/充值，仅正式密钥状态下可以做此操作。
        /// </summary>
        /// <param name="Flag">表示电表密钥状态，整型， 1: 正式密钥状态；</param>
        /// <param name="PutRand">表示输入的随机数,字符型,4字节，电表身份认证成功后返回；</param>
        /// <param name="PutDiv">表示输入的分散因子,字符型,8字节，“0000”+表号；</param>
        /// <param name="PutData">表示输入的参数明文,包含：购电金额+购电次数+客户编号，共9字节，金额、次数均为HEX码；</param>
        /// <param name="OutData">输出的数据，购电金额+购电次数+MAC1+客户编号+MAC2。</param>
        /// <returns></returns>
        [DllImport(@"EncrypNW\MasterStation_HSM.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int IncreasePurse(int Flag, string PutRand, string PutDiv, string PutData, StringBuilder OutData);

        /// <summary>
        /// 用于钱包初始化MAC计算，仅测试密钥状态下可以做此操作。
        /// </summary>
        /// <param name="Flag">表示电表密钥状态，整型，0: 测试密钥状态；</param>
        /// <param name="PutRand">表示输入的随机数,字符型,4字节，电表身份认证成功后返回；</param>
        /// <param name="PutDiv">表示输入的分散因子,字符型,8字节，“0000”+表号；</param>
        /// <param name="PutData">表示输入的数据明文，包含预置金额，4字节，HEX码；</param>
        /// <param name="OutData">输出的数据，预置金额+MAC1+“00000000”+MAC2。</param>
        /// <returns></returns>
        [DllImport(@"EncrypNW\MasterStation_HSM.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int InitPurse(int Flag, string PutRand, string PutDiv, string PutData, StringBuilder OutData);

        /// <summary>
        /// 用于电能表远程密钥更新时，获取密钥信息、密钥密文及MAC。
        /// </summary>
        /// <param name="PutKeySum">密钥总条数，最大值17；</param>
        /// <param name="PutKeyState">目标密钥状态，要更新成正式密钥时传“01” ；要更新成测试密钥时传“00”；</param>
        /// <param name="PutKeyId">指密钥编号，从“00”开始，到“10”结束，编号格式为HEX码，每次最多输出4条密钥，如"00010203"指需要输出“00”、“01”、“02”、“03”四条密钥密文，调用函数时密钥编号从“00”开始，顺序输入；</param>
        /// <param name="PutRand">4字节随机数，电表身份认证成功后返回；</param>
        /// <param name="PutDiv">8字节分散因子，“0000”+表号；</param>
        /// <param name="PutEsamNo">8字节电表安全模块序列号；</param>
        /// <param name="OutData">输出的数据，N*（4字节密钥信息+32字节密钥密文）+4字节MAC，N不大于4。</param>
        /// <returns></returns>
        [DllImport(@"EncrypNW\MasterStation_HSM.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int KeyUpdateV2(int PutKeySum, string PutKeyState, string PutKeyId, string PutRand, string PutDiv, string PutEsamNo, StringBuilder OutData);

        /// <summary>
        /// 用于生成电能表清零命令的密文。
        /// </summary>
        /// <param name="Flag">表示电表密钥状态，整型，0: 测试密钥状态；1: 正式密钥状态；</param>
        /// <param name="PutRand">4字节随机数，电表身份认证成功后返回；</param>
        /// <param name="PutDiv">8字节分散因子，“0000”+表号；</param>
        /// <param name="PutData">清零数据明文，8字节；</param>
        /// <param name="OutData">输出的数据，20字节。</param>
        /// <returns></returns>
        [DllImport(@"EncrypNW\MasterStation_HSM.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int DataClear1(int Flag, string PutRand, string PutDiv, string PutData, StringBuilder OutData);

        /// <summary>
        /// 用于产生红外认证查询所需的随机数。
        /// </summary>
        /// <param name="OutRand1">输出的8字节随机数。</param>
        /// <returns></returns>
        [DllImport(@"EncrypNW\MasterStation_HSM.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int InfraredRand(StringBuilder OutRand1);

        /// <summary>
        /// 用于获取红外认证密文和随机数。注意：红外认证前必须先进行红外认证查询。
        /// </summary>
        /// <param name="Flag">电表密钥状态，整型，0: 测试密钥状态；1: 正式密钥状态；</param>
        /// <param name="PutDiv">8字节分散因子，“0000”+表号；</param>
        /// <param name="PutEsamNo">8字节电表安全模块序列号，电能表红外认证查询命令返回；</param>
        /// <param name="PutRand1">8字节随机数，红外认证查询函数返回；</param>
        /// <param name="PutRand1Endata">8字节随机数1密文，电能表红外认证查询命令返回，密文是由电表返回的字符型数据；</param>
        /// <param name="PutRand2">8字节随机数2，电能表红外认证查询命令返回；</param>
        /// <param name="OutRand2Endata">返回8字节随机数2密文。</param>
        /// <returns></returns>
        [DllImport(@"EncrypNW\MasterStation_HSM.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int InfraredAuth(int Flag, string PutDiv, string PutEsamNo, string PutRand1, string PutRand1Endata, string PutRand2, StringBuilder OutRand2Endata);

        /// <summary>
        /// 用于验证回抄数据(包含状态查询数据)MAC的正确性。
        /// </summary>
        /// <param name="Flag">表示电表密钥状态，整型，0: 测试密钥状态；1: 正式密钥状态；</param>
        /// <param name="PutRand">4字节随机数，身份认证函数返回随机数的前4字节；</param>
        /// <param name="PutDiv">8字节分散因子，“0000”+表号；</param>
        /// <param name="PutApdu">5字节APDU指令头，固定格式为04D686P2LC，其中P2为起始地址，LC = DATA长度+MAC长度+分散因子长度;</param>
        /// <param name="PutData">回抄的数据；</param>
        /// <param name="PutMac">回抄的MAC。</param>
        /// <returns></returns>
        [DllImport(@"EncrypNW\MasterStation_HSM.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int MacCheck(int Flag, string PutRand, string PutDiv, string PutApdu, string PutData, string PutMac);

        /// <summary>
        /// 用于对需要发送给电表安全模块的明文数据，进行MAC的计算。
        /// </summary>
        /// <param name="Flag">表示电表密钥状态，整型，0: 测试密钥状态；1: 正式密钥状态；</param>
        /// <param name="PutRand">4字节随机数，电表身份认证成功后返回；</param>
        /// <param name="PutDiv">8字节分散因子，“0000”+表号；</param>
        /// <param name="PutEsamNo">8字节电表安全模块序列号；</param>
        /// <param name="PutFileID">1字节文件标识；</param>
        /// <param name="PutDataBegin">2字节起始字节；</param>
        /// <param name="PutData">明文数据,文件最大0x95字节；</param>
        /// <param name="OutData">输出的明文数据+4字节MAC数据。</param>
        /// <returns></returns>
        [DllImport(@"EncrypNW\MasterStation_HSM.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int MacWrite(int Flag, string PutRand, string PutDiv, string PutEsamNo, string PutFileID, string PutDataBegin, string PutData, StringBuilder OutData);

        /// <summary>
        /// 用于对需要发送给电表安全模块的明文数据，进行密文+MAC的计算。
        /// </summary>
        /// <param name="Flag">表示电表密钥状态，整型，0: 测试密钥状态；1: 正式密钥状态；</param>
        /// <param name="PutRand">4字节随机数，电表身份认证成功后返回；</param>
        /// <param name="PutDiv">8字节分散因子，“0000”+表号；</param>
        /// <param name="PutEsamNo">8字节电表安全模块序列号；</param>
        /// <param name="PutFileID">1字节文件标识；</param>
        /// <param name="PutDataBegin">2字节起始字节；</param>
        /// <param name="PutData">明文数据，文件最大0x95字节；</param>
        /// <param name="OutData">输出的密文和MAC数据。</param>
        /// <returns></returns>
        [DllImport(@"EncrypNW\MasterStation_HSM.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int EncMacWrite(int Flag, string PutRand, string PutDiv, string PutEsamNo, string PutFileID, string PutDataBegin, string PutData, StringBuilder OutData);

        /// <summary>
        /// 用于生成程序比对数据的密文。
        /// </summary>
        /// <param name="PutKeyid">1字节密钥索引，本套件中支持的密钥索引为05-0a，通信规约中与函数中输入索引需统一；</param>
        /// <param name="PutDiv">8字节分散因子；</param>
        /// <param name="PutData">比对数据块，64字节；</param>
        /// <param name="OutData">输出的密文，64字节。</param>
        /// <returns></returns>
        [DllImport(@"EncrypNW\MasterStation_HSM.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int EncForCompare(string PutKeyid, string PutDiv, string PutData, StringBuilder OutData);

        /// <summary>
        /// 用于对退费数据进行计算，只是正式状态下可以做此操作。
        /// </summary>
        /// <param name="Flag">电表密钥状态，整型， 1: 正式密钥状态；</param>
        /// <param name="PutRand">输入的随机数,字符型,4字节，电表身份认证成功后返回；</param>
        /// <param name="PutDiv">输入的分散因子,字符型,8字节，“0000”+表号；</param>
        /// <param name="PutData">输入的4字节退费金额,字符型，HEX码；</param>
        /// <param name="OutEndata">输出的密文和MAC,字符型。</param>
        /// <returns></returns>
        [DllImport(@"EncrypNW\MasterStation_HSM.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int DecreasePurse(int Flag, string PutRand, string PutDiv, string PutData, StringBuilder OutEndata);

        /// <summary>
        /// 用于费控模式切换，仅正式密钥状态下可以做此操作。
        /// </summary>
        /// <param name="Flag">电表密钥状态，整型，1: 正式密钥状态；</param>
        /// <param name="PutRand">输入的随机数,字符型,4字节，电表身份认证成功后返回；</param>
        /// <param name="PutDiv">输入的分散因子,字符型,8字节，“0000”+表号；</param>
        /// <param name="PutData">输入的参数明文,字符型,包含费控模式状态字+购电金额+购电次数, 金额、次数均为HEX码；</param>
        /// <param name="OutData">输出的数据，费控模式状态字+MAC1+购电金额+购电次数+MAC2。</param>
        /// <returns></returns>
        [DllImport(@"EncrypNW\MasterStation_HSM.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SwitchChargeMode(int Flag, string PutRand, string PutDiv, string PutData, StringBuilder OutData);
    }
}
