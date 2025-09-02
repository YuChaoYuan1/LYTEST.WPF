using System.Runtime.InteropServices;
using System.Text;

namespace LYTest.MeterProtocol.Encryption
{
    /// <summary>
    /// 国家电网通用加密函数 ，适用于09密钥，13密钥，面向对象698密钥
    /// </summary>
    public class EncrypAPIGW
    {
        //1. 前言
        //本接口用于电能表全检和公钥下抽检。使用过程中需要外接检测操作员 USBkey 登录检测服务器，
        //接口函数在WinSocketServer.dll 中,该dll 可以自行 更改名称后调用。
        //部分全检函数私钥下操作需要申请后作特殊授权才能开放功能。


        //3. 接口说明
        //特殊授权指对普通操作员 USBKey 进行特殊处理以达到授权目的，经授权后的用户才能取得对应的操作权限。
        //特殊授权的USBKey 需要专人负责，制定严格的安全管理制度。

        #region 3.1.通用函数

        /// <summary>
        /// 功能描述：用于获取登陆服务器的权限，兼容 09 版电能表使用的函数。
        /// </summary>
        /// <returns> 0 成功， 其他 失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int OpenUsbkey();

        /// <summary>
        /// 功能描述：用于登陆服务器，兼容 09 版电能表使用的函数。
        /// </summary>
        /// <param name="ip">字符型，代表要登陆的服务器IP</param>
        /// <param name="port">短整形，代表要登陆的服务器的端口号</param>
        /// <param name="pwdLen">密码长度，整形</param>
        /// <param name="pwd">USB密码，整形</param>
        /// <returns>0 成功 ， 其他 失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int LgServer(string ip, int port, int pwdLen, string pwd);

        /// <summary>
        /// 功能描述：断开与服务器连接，兼容 09 版电能表使用的函数。
        /// </summary>
        /// <returns>0 成功，其他 失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int LgoutServer();

        /// <summary>
        ///  功能描述：释放服务器的登陆权限，兼容 09 版电能表使用的函数。
        /// </summary>
        /// <returns>0 成功，其他 失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int ClseUsbkey();

        /// <summary>
        /// 功能描述：用于产生随机数，也可以不调用本函数自己产生随机数
        /// </summary>
        /// <param name="randLen">8或者16字节输出随机数的长度</param>
        /// <param name="outRand1">8字节随机数</param>
        /// <returns>0 成功，其他 失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int CreateRand(int randLen, StringBuilder outRand1);

        //===================================================================
        //直连厂家检测密码机，接口函数在 SoketApi.dll 中，需要对DLL改名后使用
        //===================================================================

        [DllImport(@"EncrypGW\WinSocketServer.dll", EntryPoint = "OpenDevice", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int OpenDevice();

        /// <summary>
        /// 连接密码机，用于连接服务器或密码机，新增
        /// </summary>
        /// <param name="ip">ip 地址字符型</param>
        /// <param name="port">密码机端口号,短整型</param>
        /// <param name="time">字符型，单位秒</param>
        /// <returns></returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int ConnectDevice(string ip, string port, string time);

        /// <summary>
        /// 断开密码机，用于断开服务器或密码机
        /// </summary>        
        /// <returns>0 成功，其他 失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int CloseDevice();

        //==========================================================================
        //未使用函数
        //==========================================================================
        /// <summary>
        /// 未使用
        /// </summary>
        [DllImport(@"EncrypGW\WinSocketServer.dll", EntryPoint = "SetIp", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int SetIp(string ip, string port, string time);

        ///3.1.1. 获取登录服务器权限函数
        /// <summary>
        /// 未使用 功能描述：用于登陆服务器的权限
        /// </summary>        
        /// <returns>0 成功 ， 其他 失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", EntryPoint = "IntOpenUsbkey", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int IntOpenUsbkey();
        #endregion


        #region 3.2. 2009 版规范电能表函数,本节函数主要用于兼容 2009 版规范电能表已开发的软件。

        ///3.2.1. 身份认证函数
        /// <summary>
        /// 功能描述：从密码机获取随机数以及密文,用于远程身份认证，公钥下函数内部分散因子默认为“0000000000000001”，只用于2009 版规范电能表。
        /// </summary>
        /// <param name="flag">整形，0公钥状态，1私钥状态</param>
        /// <param name="PutDiv">输入的分散因子，字符型，长度16，“0000”+表号</param>
        /// <param name="outRand">输出的随机数1，字符型，长度16</param>
        /// <param name="outEndata">输出的密文，字符型，长度16</param>
        /// <param name="nameId">厂家名称，可以为空，主要用于兼容2009 版电表身份认证</param>
        /// <returns>0 成功，其他 失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int IdentityAuthentication(int flag, string PutDiv, StringBuilder outRand, StringBuilder outEndata, string nameId);

        ///3.2.2. 控制命令加密函数
        /// <summary>
        /// 功能描述：用户获取控制命令的密文
        /// </summary>
        /// <param name="flag">整形，0公钥状态，1私钥状态（需要特殊授权）</param>
        /// <param name="PutRand">输入的随机数2，字符型，长度8</param>
        /// <param name="PutDiv">输入的分散因子，字符型，长度16，“0000”+表号</param>
        /// <param name="PutEsamNo">输入的ESAM序列号，复位信息后的8字节，字符型，长度16</param>
        /// <param name="PutData">跳闸或者合闸的控制命令明文，字符型</param>
        /// <param name="OutEndata">输出的密文，字符型</param>
        /// <returns>0 成功，其他 失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int UserControl(int flag, string PutRand, string PutDiv, string PutEsamNo, string PutData, StringBuilder OutEndata);

        ///3.2.3. 一类参数 MAC 计算函数
        /// <summary>
        /// 功能描述：用于电能表一类参数MAC计算函数
        /// </summary>
        /// <param name="Flag">整形，0公钥状态，1私钥状态（需要特殊授权）</param>
        /// <param name="PutRand">输入的随机数2，字符型，长度8</param>
        /// <param name="PutDiv">输入的分散因子，字符型，长度16，“0000”+表号</param>
        /// <param name="PutApdu">一类参数设置的写ESAM命令头，字符型，长度10</param>
        /// <param name="PutData">输入的一类参数明文，字符型</param>
        /// <param name="OutEndata">输出的 明文数据+MAC数据，字符型，长度明文数据长度+8</param>
        /// <returns>0 成功，其他 失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int ParameterUpDate(int Flag, string PutRand, string PutDiv, string PutApdu, string PutData, StringBuilder OutEndata);

        ///3.2.4. 二类参数加密函数
        /// <summary>
        /// 功能描述：用于远程二类参数设置加密
        /// </summary>
        /// <param name="Flag">整形，0公钥状态，1私钥状态（<font color='red'>需要特殊授权</font>）</param>
        /// <param name="PutRand">输入的随机数2，字符型，长度8</param>
        /// <param name="PutDiv">输入的分散因子，字符型，长度16，“0000”+表号</param>
        /// <param name="PutApdu">输入的指令数据，字符型，长度10</param>
        /// <param name="PutData">输入的二类参数明文，字符型</param>
        /// <param name="OutEndata">输出的密文，字符型</param>
        /// <returns>0成功，其他失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int ParameterElseUpdate(int Flag, string PutRand, string PutDiv, string PutApdu, string PutData, StringBuilder OutEndata);

        /// <summary>
        /// 功能描述：用于第一套费率参数MAC计算, 第一套费率电价设置函数
        /// </summary>
        /// <param name="Flag">整形，0公钥状态，1私钥状态（需要特殊授权）</param>
        /// <param name="PutRand">输入的随机数2，字符型，长度8</param>
        /// <param name="PutDiv">输入的分散因子，字符型，长度16，“0000”+表号</param>
        /// <param name="PutApdu">输入的指令数据，字符型，长度10</param>
        /// <param name="PutData">输入的第一套费率参数明文，字符型</param>
        /// <param name="OutEndata">输出的 明文数据+MAC数据，字符型，长度明文数据长度+8</param>
        /// <returns>0 成功，其他 失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int ParameterUpDate1(int Flag, string PutRand, string PutDiv, string PutApdu, string PutData, StringBuilder OutEndata);

        ///3.2.6. 第二套费率电价设置函数
        /// <summary>
        /// 功能描述：该函数用于2009版规范中的第而套费率电价设置，也用于2013版规范中的备用套电价参数MAC计算
        /// </summary>
        /// <param name="Flag">整形，0公钥状态，1私钥状态（需要特殊授权）</param>
        /// <param name="PutRand">输入的随机数2，字符型，长度8</param>
        /// <param name="PutDiv">输入的分散因子，字符型，长度16，“0000”+表号</param>
        /// <param name="PutApdu">输入的指令数据，字符型，长度10</param>
        /// <param name="PutData">输入的第二套费率参数或者当前套电价参数明文，字符型</param>
        /// <param name="OutEndata">输出的 明文数据+MAC数据，字符型，长度明文数据长度+8</param>
        /// <returns>0 成功，其他 失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int ParameterUpDate2(int Flag, string PutRand, string PutDiv, string PutApdu, string PutData, StringBuilder OutEndata);

        ///3.2.7. 密钥清零函数
        /// <summary>
        /// 功能描述：获取远程密钥和主控密钥的密钥信息和密钥密文。
        /// </summary>
        /// <param name="Flag">表示当前密钥状态，0，公钥状态下清零，1私钥状态下清零</param>
        /// <param name="PutRand">输入的随机数2,字符型,长度16</param>
        /// <param name="PutDiv">表示输入的分散因子，字符型，长度16 “0000”+表号</param>
        /// <param name="PutEsamNo">表示输入的Esam序列号，复位信息的后8字节，字符型，长度16。</param>
        /// <param name="PutKeyinfo1">输入的主控密钥密钥信息明文,字符型</param>
        /// <param name="Outkey1">输出的主控密钥密文,字符型,长度64</param>
        /// <param name="OutKeyinfo1">输出的主控密钥信息,字符型,长度8</param>
        /// <returns>0 成功，其他 失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int ClearKeyInfo(int Flag, string PutRand, string PutDiv, string PutEsamNo, string PutKeyinfo1, StringBuilder Outkey1, StringBuilder OutKeyinfo1);

        ///3.2.8. 获取远程密钥函数
        /// <summary>
        /// 功能描述：获取远程密钥和主控密钥的密钥信息和密钥密文。
        /// </summary>
        /// <param name="Flag">整形，为0时修改，为1时恢复，需特殊授权</param>
        /// <param name="PutRand">输入的随机数2，字符型，长度16</param>
        /// <param name="PutDiv">输入的分散因子，字符型，长度16，“0000”+表号</param>
        /// <param name="PutEsamNo">输入的ESAM序列号，复位信息的后8字节，字符型，长度16</param>
        /// <param name="PutKeyinfo1">输入的主控密钥信息明文，字符型</param>
        /// <param name="PutKeyinfo2">输入的远程控制密钥信息明文，字符型</param>
        /// <param name="PutKeyinfo3">输入的二类参数设置密钥信息明文，字符型</param>
        /// <param name="PutKeyinfo4">输入的远程身份认证密钥信息明文，字符型</param>
        /// <param name="OutKey1">输出的主控密钥密文，字符型，长度64</param>
        /// <param name="OutKeyinfo1">输出的主控密钥信息，字符型，长度8</param>
        /// <param name="OutKey2">输出的远程控制密钥密文，字符型，长度64</param>
        /// <param name="OutKeyinfo2">输出的远程控制密钥信息，字符型，长度8</param>
        /// <param name="OutKey3">输出的二类参数设置密钥密文，字符型，长度64</param>
        /// <param name="OutKeyinfo3">输出的二类参数设置密钥信息，字符型，长度8</param>
        /// <param name="OutKey4">输出的远程身份认证密钥密文，字符型，长度64</param>
        /// <param name="OutKeyinfo4">输出的远程身份认证密钥信息，字符型，长度8</param>
        /// <returns>0 成功，其他 失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int KeyUpdate(int Flag, string PutRand, string PutDiv, string PutEsamNo, string PutKeyinfo1, string PutKeyinfo2,
            string PutKeyinfo3, string PutKeyinfo4, StringBuilder OutKey1, StringBuilder OutKeyinfo1, StringBuilder OutKey2, StringBuilder OutKeyinfo2, StringBuilder OutKey3, StringBuilder OutKeyinfo3,
            StringBuilder OutKey4, StringBuilder OutKeyinfo4);
        #endregion

        #region 3.3. 2013 版规范电能表函数建议 2013 版规约电能表使用3.3 中的函数，3.3.1—3.3.6 函数功能同3.2.1—3.2.6 功能，只是函数名称不一样。

        ///3.3.1. 身份认证函数
        /// <summary>
        /// 功能描述：从密码机获取随机数以及密文,用于远程身份认证。分散因子用实际表号。2013
        /// </summary>
        /// <param name="Flag">整型, 0:公钥状态;1,私钥状态</param>
        /// <param name="PutDiv">输入的分散因子,字符型,长度16, “0000”+表号</param>
        /// <param name="OutRand">输出的随机数1,字符型,长度16</param>
        /// <param name="OutEndata">输出的密文,字符型,长度16</param>
        /// <returns>0 成功，其他 失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int Meter_Formal_IdentityAuthentication(int Flag, string PutDiv, StringBuilder OutRand, StringBuilder OutEndata);

        ///3.3.2. 控制命令加密函数
        /// <summary>
        /// 功能描述：用户获取控制命令密文。
        /// </summary>
        /// <param name="Flag">整型, 0:公钥状态;1,私钥状态(需要特殊授权)</param>
        /// <param name="PutRand">输入的随机数2,字符型,长度8</param>
        /// <param name="PutDiv">输入的分散因子,字符型,长度16,“0000”+表号</param>
        /// <param name="PutEsamNo">输入的ESAM 序列号,复位信息的后8 字节,字符型,长度16</param>
        /// <param name="PutData">跳闸或合闸控制命令明文,字符型</param>
        /// <param name="OutEndata">输出的密文,字符型</param>
        /// <returns>0 成功，其他 失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int Meter_Formal_UserControl(int Flag, string PutRand, string PutDiv, string PutEsamNo, string PutData, StringBuilder OutEndata);

        ///3.3.3. 一类参数 MAC 计算函数
        /// <summary>
        /// 功能描述：用于电能表一类参数MAC 计算函数，。
        /// </summary>
        /// <param name="Flag">整型, 0:公钥状态;1,私钥状态(需要特殊授权)</param>
        /// <param name="PutRand">输入的随机数2,字符型,长度8</param>
        /// <param name="PutDiv">输入的分散因子,字符型,长度16, “0000”+表号</param>
        /// <param name="PutApdu">一类参数设置的写Esam 命令头,字符型,长度10</param>
        /// <param name="PutData">输入的一类参数明文,字符型</param>
        /// <param name="OutEndata">输出的 明文数据+MAC数据，字符型，长度明文数据长度+8</param>
        /// <returns>0 成功，其他 失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int Meter_Formal_ParameterUpdate(int Flag, string PutRand, string PutDiv, string PutApdu, string PutData, StringBuilder OutEndata);

        ///3.3.4. 二类参数加密函数
        /// <summary>
        /// 功能描述：用于远程二类参数设置加密。
        /// </summary>
        /// <param name="Flag">整型, 0:公钥状态;1,私钥状态(需要特殊授权)</param>
        /// <param name="PutRand">输入的随机数2,字符型,长度8</param>
        /// <param name="PutDiv">输入的分散因子,字符型,长度16, “0000”+表号</param>
        /// <param name="PutApdu">输入的指令数据,字符型,长度10</param>
        /// <param name="PutData">输入的二类参数明文,字符型</param>
        /// <param name="OutEndata">输出的密文,字符型</param>
        /// <returns>0 成功，其他 失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int Meter_Formal_ParameterElseUpdate(int Flag, string PutRand, string PutDiv, string PutApdu, string PutData, StringBuilder OutEndata);

        ///3.3.5. 第一套费率电价设置函数
        /// <summary>
        /// 功能描述：只用于第一套费率参数MAC 计算,只用于2009 版规范电能表。
        /// </summary>
        /// <param name="Flag">整型, 0:公钥状态;1,私钥状态(需要特殊授权)</param>
        /// <param name="PutRand">输入的随机数2,字符型,长度8</param>
        /// <param name="PutDiv">输入的分散因子,字符型,长度16,“0000”+表号</param>
        /// <param name="PutApdu">输入的指令数据,字符型,长度10</param>
        /// <param name="PutData">输入的第一套费率参数明文,字符型</param>
        /// <param name="OutEndata">输出的 明文数据长度+MAC数据，字符型，长度明文数据长度+8</param>
        /// <returns>0 成功，其他 失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int Meter_Formal_ParameterUpdate1(int Flag, string PutRand, string PutDiv, string PutApdu, string PutData, StringBuilder OutEndata);

        ///3.3.6. 第二套费率电价设置函数
        /// <summary>
        /// 功能描述：该函数用于2009 版规范中的第二套费率电价设置，也用于2013 版规范中的备用套电价参数MAC 计算。
        /// </summary>
        /// <param name="Flag">整型, 0:公钥状态;1,私钥状态(需要特殊授权)</param>
        /// <param name="PutRand">输入的随机数2,字符型,长度8</param>
        /// <param name="PutDiv">输入的分散因子,字符型,长度16,“0000”+表号</param>
        /// <param name="PutApdu">输入的指令数据,字符型,长度10</param>
        /// <param name="PutData">输入的第二套费率参数或当前套电价参数明文,字符型</param>
        /// <param name="OutEndata">输出的 明文数据长度+MAC数据，字符型，长度明文数据长度+8</param>
        /// <returns>0 成功，其他 失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int Meter_Formal_ParameterUpdate2(int Flag, string PutRand, string PutDiv, string PutApdu, string PutData, StringBuilder OutEndata);

        ///3.3.7. 钱包初始化
        /// <summary>
        /// 功能描述：用于本地费控电能表钱包数据MAC计算
        /// </summary>
        /// <param name="Flag">整型，0公钥状态</param>
        /// <param name="PutRand">随机数2，电表身份认证成功返回，4字节</param>
        /// <param name="PutDiv">分散因子，8字节，“0000”+表号</param>
        /// <param name="PutData">输入的数据明文，包含预置金额</param>
        /// <param name="OutData">输出的数据，预置金额+MAC1+"00000000"+MAC2</param>
        /// <returns>0 成功，其他 失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int Meter_Formal_InintPurse(int Flag, string PutRand, string PutDiv, string PutData, StringBuilder OutData);

        ///3.3.8. 电表清零函数
        /// <summary>
        /// 功能描述:用于远程费控电能表清零
        /// </summary>
        /// <param name="Flag">整形，0公钥状态，1私钥状态（需要特殊授权）</param>
        /// <param name="PutRand">随机数2，电表身份认证成功返回，4字节</param>
        /// <param name="PutDiv">分散因子，8字节，“0000”+表号</param>
        /// <param name="PutData">入参，清零数据</param>
        /// <param name="OutData">20字节密文</param>
        /// <returns>0 成功，其他 失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int Meter_Formal_DataClear1(int Flag, string PutRand, string PutDiv, string PutData, StringBuilder OutData);

        ///3.3.9. 事件或需量清零函数
        /// <summary>
        /// 功能描述:用于电能表事件或者需量清零
        /// </summary>
        /// <param name="Flag">整形，0公钥状态，1私钥状态（需要特殊授权）</param>
        /// <param name="PutRand">随机数2，电表身份认证成功返回，4字节</param>
        /// <param name="PutDiv">分散因子，8字节，“0000”+表号</param>
        /// <param name="PutData">入参，清零数据</param>
        /// <param name="OutData">20字节密文</param>
        /// <returns>0 成功，其他 失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", EntryPoint = "Meter_Formal_DataClear2", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int Meter_Formal_DataClear2(int Flag, string PutRand, string PutDiv, string PutData, StringBuilder OutData);

        ///3.3.10. 红外认证函数
        /// <summary>
        /// 功能描述：用于获取红外认证密文和随机数2，红外认证前先进行红外查询
        /// </summary>
        /// <param name="Flag">0公钥状态</param>
        /// <param name="PudDiv">分散因子，8字节，“0000”+表号</param>
        /// <param name="PutEsamNo">8字节ESAM序列号，电能表红外查询命令返回</param>
        /// <param name="PutRand1">8字节随机数1，创建随机数函数返回</param>
        /// <param name="InRand1Endata">8字节随机数1密文，电能表红外查询命令返回</param>
        /// <param name="PutRand2">8字节随机数2，电能表红外查询命令返回</param>
        /// <param name="OutRand2Endata">返回8字节随机数2密文</param>
        /// <returns>0 成功，其他 失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int Meter_Formal_InfraredAuth(int Flag, string PudDiv, string PutEsamNo, string PutRand1, string PutRand1Endata, string PutRand2, StringBuilder OutRand2Endata);

        ///3.3.11. 数据回抄
        /// <summary>
        /// 功能描述:用于数据回抄MAC校验
        /// </summary>
        /// <param name="Flag">0公钥状态</param>
        /// <param name="PutRand">随机数1的高4字节</param>
        /// <param name="PutDiv">分散因子，8字节，“0000”+表号</param>
        /// <param name="PutApdu">命令头，5字节（04d686+起始地址+LEN，LEN为数据长度+0x0C）</param>
        /// <param name="PutData">数据回抄返回的数据</param>
        /// <param name="PutMac">4字节数据回抄返回的MAC</param>
        /// <returns>0成功，其他失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int Meter_Formal_MacCheck(int Flag, string PutRand, string PutDiv, string PutApdu, string PutData, string PutMac);

        ///3.3.12. 密钥更新函数
        /// <summary>
        /// 功能描述：用于电能表远程密钥更新，2013年标准电能表密钥更新本地表和远程表都采用通信方式完成，共20条密钥，分5次调用本函数，所得密钥分5次下发给电能表，密钥更新需要先抄读芯片发行信息文件数据
        /// </summary>
        /// <param name="PutKeySum">密钥总条数，固定为20</param>
        /// <param name="PutKeystate">密钥状态，“00”，密钥恢复（需特殊授权），“01”密钥下装</param>
        /// <param name="PutKeyid">密钥编号，0x00-0x13，每次最多输出4条密钥“00010203”</param>
        /// <param name="PutRand">随机数2，电表身份认证成功返回，4字节</param>
        /// <param name="PutDiv">8字节分散因子，“0000”+表号</param>
        /// <param name="PutEsamNo">8字节ESAM序列号</param>
        /// <param name="PutChiplnfor">芯片发行信息文件数据，从电表ESAM抄读得到，005AH字节</param>
        /// <param name="OutData">出参，4*（4字节密钥信息+32字节密钥密文）+4字节MAC</param>
        /// <returns>0成功，其他失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int Meter_Formal_KeyUpdateV2(int PutKeySum, string PutKeystate, string PutKeyid, string PutRand, string PutDiv, string PutEsamNo,
            string PutChiplnfor, StringBuilder OutData);

        #endregion

        #region   698电表
        /// <summary>
        ///6.5.1.  主站会话协商  对称密码连接认证机制， 用于主站与设备进行会话协商时产生密文 1 和客户机签名 1，该过程在建立应用连接时完成。
        /// </summary>
        /// <param name="iKeyState"></param>
        /// <param name="cDiv"> 分散因子（ 8Byte）， iKeyState=0， cDiv 为芯片序列号；iKeyState=1， cDiv 为表号；</param>
        /// <param name="cASCTR"></param>
        /// <param name="cFLG">应用密钥产生标识， 1Byte，默认”01”;</param>
        /// <param name="cOutRandHost">主站随机数（ 16Byte）</param>
        /// <param name="cOutSessionInit">会话协商数据，建立应用连接中的密文 1；</param>
        /// <param name="cOutSign">协商数据签名(4Byte)，建立应用连接中的客户机签名 1；</param>
        /// <returns>返回 0 =成功 1= 失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", EntryPoint = "Obj_Meter_Formal_InitSession", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int Obj_Meter_Formal_InitSession(int iKeyState, string cDiv, string cASCTR, string cFLG, StringBuilder cOutRandHost, StringBuilder cOutSessionInit, StringBuilder cOutSign);

        /// <summary>
        ///6.5.2.  主站会话协商验证
        /// </summary>   对称密码连接认证机制，用于主站验证设备会话协商时返回的数据，验证成功主站产生会话密钥。
        /// <param name="iKeyState"></param>
        /// <param name="cDiv">分散因子（ 8Byte）， iKeyState=0， cDiv 为芯片序列号；iKeyState=1， cDiv 为表号，表号不足 8 字节前补 0；</param>
        /// <param name="cRandHost">主站随机数 R1（ 16Byte）</param>
        /// <param name="cSessionData">终端返回的会话协商数据(48Byte)，建立应用连接中的密文 2；</param>
        /// <param name="cSign">终端返回的会话协商数据签名(4Byte) ，建立应用连接中的客户机签名 2；</param>
        /// <param name="cOutSessionKey">会话密钥</param>
        /// <returns></returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", EntryPoint = "Obj_Meter_Formal_VerifySession", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int Obj_Meter_Formal_VerifySession(int iKeyState, string cDiv, string cRandHost, string cSessionData, string cSign, StringBuilder cOutSessionKey);

        /// <summary>
        /// 6.5.3.  抄读数据验证
        /// </summary> 主站验证设备返回的抄读数据，具体指抄读电能表返回的数据。
        /// <param name="iKeyState"></param>
        /// <param name="iOperateMode"></param>
        /// <param name="cMeterNo"></param>
        /// <param name="cRandHost">主站随机数(16Byte</param>
        /// <param name="cReadData">抄读数据</param>
        /// <param name="cMac">数据</param>
        /// <param name="cOutData">明文抄读数据， iOperateMode=1，为空</param>
        /// <returns></returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", EntryPoint = "Obj_Meter_Formal_VerifyReadData", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int Obj_Meter_Formal_VerifyReadData(int iKeyState, string iOperateMode, string cMeterNo, string cRandHost, string cReadData, string cMac, StringBuilder cOutData);

        /// <summary>
        ///6.5.4.  上报数据验证 
        /// </summary> 设备主动上报数据时，主站验证数据的合法性。
        /// <param name="iKeyState"></param>
        /// <param name="iOperateMode"></param>
        /// <param name="cMeterNo"></param>
        /// <param name="cRandT">终端随机数(12B)</param>
        /// <param name="cReportData">上报数据</param>
        /// <param name="cMac">MAC 数据</param>
        /// <param name="cOutData">明文数据， iOperateMode=1，为空</param>
        /// <param name="cOutRSPCTR">主动上报随机数</param>
        /// <returns></returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", EntryPoint = "Obj_Meter_Formal_VerifyReportData", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int Obj_Meter_Formal_VerifyReportData(int iKeyState, string iOperateMode, string cMeterNo, string cRandT, string cReportData, string cMac, StringBuilder cOutData, StringBuilder cOutRSPCTR);

        /// <summary>
        ///6.5.5.  上报数据返回报文加密
        /// </summary> 用于设备主动上报主站返回帧数据加密计算。
        /// <param name="iKeyState"></param>
        /// <param name="iOperateMode"></param>
        /// <param name="cMeterNo"></param>
        /// <param name="RandHost"></param>
        /// <param name="cReportData"></param>
        /// <param name="OutSID"></param>
        /// <param name="OutAttachData"></param>
        /// <param name="cOutData"></param>
        /// <param name="ucOutMac"></param>
        /// <returns></returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", EntryPoint = "Obj_Meter_Formal_GetResponseData", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int Obj_Meter_Formal_GetResponseData(int iKeyState, string iOperateMode, string cMeterNo, string RandHost, string cReportData, StringBuilder OutSID, StringBuilder OutAttachData, StringBuilder cOutData, StringBuilder ucOutMac);

        /// <summary>
        ///6.5.6.  安全传输加密
        /// </summary> 用于对具体业务数据进行数据加密计算。
        /// <param name="iOperateMode"></param>
        /// <param name="cESAMID">此处可为空</param>
        /// <param name="cSessionKey">会话密钥</param>
        /// <param name="cTaskType">参数类型：4，安全模式参数、会话时效门限；5，电价、电价切换时间、费率时段、对时；8，拉闸任务；3，除上述操作外的数据加密，密钥更新、写 ESAM 操作和钱包操作数据下发通过此函数进行安全计算。</param>
        /// <param name="cTaskData">数据明文； NByte</param>
        /// <param name="cOutSID"></param>
        /// <param name="cOutAttachData"></param>
        /// <param name="cOutData"></param>
        /// <param name="cOutMAC"></param>
        /// <returns></returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", EntryPoint = "Obj_Meter_Formal_GetSessionData", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int Obj_Meter_Formal_GetSessionData(int iOperateMode, string cESAMID, string cSessionKey, int cTaskType, string cTaskData, StringBuilder cOutSID, StringBuilder cOutAttachData, StringBuilder cOutData, StringBuilder cOutMAC);

        /// <summary>
        ///6.5.6.  安全传输加密
        /// </summary> 用于对具体业务数据进行数据加密计算。
        /// <param name="iOperateMode"></param>
        /// <param name="cESAMID">此处可为空</param>
        /// <param name="cSessionKey">会话密钥</param>
        /// <param name="cTaskData">数据明文； NByte</param>
        /// <param name="cOutSID"></param>
        /// <param name="cOutAttachData"></param>
        /// <param name="cOutData"></param>
        /// <param name="cOutMAC"></param>
        /// <returns></returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", EntryPoint = "Obj_Meter_Formal_GetMeterSetData", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int Obj_Meter_Formal_GetMeterSetData(int iOperateMode, string cESAMID, string cSessionKey, string cTaskData, StringBuilder cOutSID, StringBuilder cOutAttachData, StringBuilder cOutData, StringBuilder cOutMAC);

        /// <summary>
        ///6.5.7.  安全传输解密
        /// </summary>  用于电能表返回帧数据解密验证。
        /// <param name="iKeyState"></param>
        /// <param name="iOperateMode"></param>
        /// <param name="cESAMID"></param>
        /// <param name="cSessionKey"></param>
        /// <param name="cTaskData">数据</param>
        /// <param name="cMac">MAC 数据</param>
        /// <param name="cOutData">数据明文</param>
        /// <returns></returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", EntryPoint = "Obj_Meter_Formal_VerifyMeterData", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int Obj_Meter_Formal_VerifyMeterData(int iKeyState, int iOperateMode, string cESAMID, string cSessionKey, string cTaskData, string cMac, StringBuilder cOutData);

        /// <summary>
        /// 6.5.8. 广播数据加密
        /// </summary> 用于广播数据加密计算。
        /// <param name="iKeyState"></param>
        /// <param name="iOperateMode"></param>
        /// <param name="cESAMID">此处可为空</param>
        /// <param name="cBrdCstAddr">广播地址</param>
        /// <param name="AGSEQ">广播应用通信序列号， 4Byte</param>
        /// <param name="cBrdCstData">广播数据明文；</param>
        /// <param name="cOutSID"></param>
        /// <param name="cOutAttachData"></param>
        /// <param name="cOutData"></param>
        /// <param name="cOutMac"></param>
        /// <returns></returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", EntryPoint = "Obj_Meter_Formal_GetGrpBrdCstData", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int Obj_Meter_Formal_GetGrpBrdCstData(int iKeyState, int iOperateMode, string cESAMID, string cBrdCstAddr, string AGSEQ, string cBrdCstData, StringBuilder cOutSID, StringBuilder cOutAttachData, StringBuilder cOutData, StringBuilder cOutMac);

        /// <summary>
        ///6.5.9.  设置 ESAM 参数
        /// </summary> 用于设置表号、当前套电价文件、备用套电价文件、 ESAM 存储标识。
        /// <param name="iKeyState"></param>
        /// <param name="iOperateMode"></param>
        /// <param name="cESAMID"></param>
        /// <param name="cSessionKey"></param>
        /// <param name="cMeterNo">表号(8Byte)，不够 8Byte 前面填充 0</param>
        /// <param name="cESAMRand"></param>
        /// <param name="cData">4ByteOAD + 1Byte 内容 LEN + 内容</param>
        /// <param name="OutSID"></param>
        /// <param name="OutAddData"></param>
        /// <param name="OutData"></param>
        /// <param name="OutMAC"></param>
        /// <returns></returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", EntryPoint = "Obj_Meter_Formal_SetESAMData", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int Obj_Meter_Formal_SetESAMData(int iKeyState, int iOperateMode, string cESAMID, string cSessionKey, string cMeterNo, string cESAMRand, string cData, StringBuilder OutSID, StringBuilder OutAddData, StringBuilder OutData, StringBuilder OutMAC);

        /// <summary>
        /// 6.5.10. 钱包操作
        /// </summary>用于设置表号、当前套电价文件、备用套电价文件、 ESAM 存储标识。
        /// <param name="iOperateMode"></param>
        /// <param name="cESAMID"></param>
        /// <param name="cSessionKey"></param>
        /// <param name="cTaskType">任务序编号， 9，钱包初始化； 10，钱包充值； 11，钱包退费</param>
        /// <param name="cTaskData">数据明文，包含预置金额， 4Byte cOutSID</param>
        /// <param name="cOutSID"></param>
        /// <param name="cOutAttachData"></param>
        /// <param name="cOutData"></param>
        /// <param name="cOutMAC"></param>
        /// <returns></returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", EntryPoint = "Obj_Meter_Formal_GetPurseData", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int Obj_Meter_Formal_GetPurseData(int iOperateMode, string cESAMID, string cSessionKey, int cTaskType, string cTaskData, StringBuilder cOutSID, StringBuilder cOutAttachData, StringBuilder cOutData, StringBuilder cOutMAC);

        /// <summary>
        ///6.5.11.  电能表对称密钥更新
        /// </summary> 用于对称密钥更新。
        /// <param name="iKeyState">密钥更新的目标状态， 1，代表更新到私钥， 0 代表恢复到初始密钥；</param>
        /// <param name="cESAMID"></param>
        /// <param name="cSessionKey"></param>
        /// <param name="cMeterNo">表号(8Bytes)</param>
        /// <param name="cKeyType">密钥类型，此处为 00，应用密钥</param>
        /// <param name="cOutSID"></param>
        /// <param name="cOutAttachData"></param>
        /// <param name="cOutTrmKeyData"></param>
        /// <param name="cOutMAC"></param>
        /// <returns></returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", EntryPoint = "Obj_Meter_Formal_GetTrmKeyData", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int Obj_Meter_Formal_GetTrmKeyData(int iKeyState, string cESAMID, string cSessionKey, string cMeterNo, string cKeyType, StringBuilder cOutSID, StringBuilder cOutAttachData, StringBuilder cOutTrmKeyData, StringBuilder cOutMAC);
        //public static extern int Obj_Meter_Formal_GetTrmKeyData(int iKeyState, string cESAMID, string cSessionKey, string cMeterNo, string cKeyType, StringBuilder cOutSID, StringBuilder cOutAttachData, StringBuilder cOutTrmKeyData, StringBuilder cOutMAC);
        /// <summary>
        /// 6.5.12. 电表对称密钥初始化 
        /// </summary> 用于对电能表对称密钥进行初始化，须先对密钥进行初始化，此函数暂时不需要使用。
        /// <param name="iKeyState"></param>
        /// <param name="cESAMID"></param>
        /// <param name="cSessionKey"></param>
        /// <param name="cMeterNo"></param>
        /// <param name="cKeyType"></param>
        /// <param name="cOutSID"></param>
        /// <param name="cOutAttachData"></param>
        /// <param name="cOutTrmKeyData"></param>
        /// <param name="cOutMAC"></param>
        /// <returns></returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", EntryPoint = "Obj_Meter_Formal_InitTrmKeyData", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int Obj_Meter_Formal_InitTrmKeyData(int iKeyState, string cESAMID, string cSessionKey, string cMeterNo, string cKeyType, StringBuilder cOutSID, StringBuilder cOutAttachData, StringBuilder cOutTrmKeyData, StringBuilder cOutMAC);


        // 错误码          错误描述            错误码         错误描述
        //--------------------------------------------------------------------------------------------
        //  48          无设备或设备无效       56           创建 socket 句柄失败
        //  57          连接服务器失败         64           客户端发送数据失败
        //  65          客户端接收数据失败     100          打开设备失败
        //  160         连接密码机失败         161          操作权限不够
        //  162         USBKey 不是操作员      163          服务器发送数据失败
        //  164         服务端接收报文失败     165          密码机加密数据失败
        //  166         密码机导出密钥失败     167          密码机计算 MAC 失败
        //  168         服务器已断开连         169          数据无效
        //  170         密码机收发报文错误     171          密码机故障
        //  172         数据库出错             700-712      客户端导出密钥失败
        //  800-810     计算 MAC 失败          900-910      加密数据失败
        //  1000-1010   数据长度错             1100         系统认证错误
        //  1107        USBKey 权限不正确      1108-1111    操作 USBKey 失败
        //  1206        签名数据错误           45           密码机密钥错
        //--------------------------------------------------------------------------------------------
        #endregion

        #region 物联表
        /// <summary>
        /// 主站会话协商（计量芯）
        /// </summary>
        /// <param name="iKeyState"></param>
        /// <param name="cTESAMID">ESAM 序列号(8Byte)</param>
        /// <param name="cDiv">表号(8Byte)，表号不足 8 字节前补 0</param>
        /// <param name="cASCTR"></param>
        /// <param name="cFLG">应用密钥产生标识，1Byte，默认”01”</param>
        /// <param name="cOutRandHost">主站随机数(16Byte)</param>
        /// <param name="cOutSessionInit">会话协商数据，建立应用连接中的密文 1；</param>
        /// <param name="cOutMac">协商数据签名(4Byte)，建立应用连接中的客户机签名 1</param>
        /// <returns>返回 0 成功,其他 失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", EntryPoint = "Obj_JL_Formal_InitSession", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int Obj_JL_Formal_InitSession(int iKeyState, string cTESAMID, string cDiv, string cASCTR, string cFLG, StringBuilder cOutRandHost, StringBuilder cOutSessionInit, StringBuilder cOutMac);

        /// <summary>
        /// 主站会话协商验证（计量芯）
        /// </summary>
        /// <param name="iKeyState"></param>
        /// <param name="cTESAMID">ESAM 序列号(8Byte)</param>
        /// <param name="cDiv">表号(8Byte)，表号不足 8 字节前补 0</param>
        /// <param name="cRandHost">主站随机数 R1（16Byte）</param>
        /// <param name="cSessionData">终端返回的会话协商数据(48Byte)，建立应用连接中的密文 2</param>
        /// <param name="cSign">终端返回的会话协商数据签名(4Byte) ，建立应用连接中的客户机签名 2</param>
        /// <param name="cOutSessionKey">会话密钥</param>
        /// <returns>返回 0 成功,其他 失败</returns>
        [DllImport(@"EncrypGW\WinSocketServer.dll", EntryPoint = "Obj_JL_Formal_VerifySession", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        unsafe public static extern int Obj_JL_Formal_VerifySession(int iKeyState, string cTESAMID, string cDiv, string cRandHost, string cSessionData, string cSign, StringBuilder cOutSessionKey);

        #endregion

    }
}
