//using System;
//using System.CodeDom;
//using System.CodeDom.Compiler;
//using System.IO;
//using System.Net;
//using System.Reflection;
//using System.Text;
//using System.Web.Services.Description;
//using System.Web.Services.Discovery;
//using System.Xml.Schema;

//namespace LYTest.Utility
//{
//    /// <summary>
//    /// WebService操作类
//    /// </summary>
//    public static class WebServiceHelper
//    {
//        //定义一个命名空间，可取比较随意的名字
//        //类似于手动添加webservice服务时添加的命名空间名称
//        private static readonly string namespaceTemp = "nameSpaceTemp";
//        private static string currentUrl = "";
//        private static Assembly currentAssembly;

//        public static void InitialAssembly(string urls)
//        {
//            string url = urls.Split('?')[0];
//            #region 获取服务描述语言(WSDL)
//            WebClient client = new WebClient();
//            Stream stream = client.OpenRead(url + "?WSDL");//【1】
//            ServiceDescription serviceDescription = ServiceDescription.Read(stream);//【2】
//            #endregion

//            #region 将从服务器获取的WSDL导入为命名空间
//            ServiceDescriptionImporter serviceImporter = new ServiceDescriptionImporter();//【3】
//            serviceImporter.ProtocolName = "soap";
//            serviceImporter.Style = ServiceDescriptionImportStyle.Client;
//            serviceImporter.AddServiceDescription(serviceDescription, "", "");

//            #region 这里是找了好久找到的
//            //作用是在命名空间里面加入了xsd架构，不然有的wsdl不能解析
//            DiscoveryClientProtocol dcp = new DiscoveryClientProtocol();
//            dcp.DiscoverAny(url + "?WSDL");
//            dcp.ResolveAll();

//            foreach (object osd in dcp.Documents.Values)
//            {
//                if (osd is ServiceDescription) serviceImporter.AddServiceDescription((ServiceDescription)osd, null, null);
//                if (osd is XmlSchema) serviceImporter.Schemas.Add((XmlSchema)osd);
//            }
//            #endregion

//            //添加上面定义的命名空间
//            CodeNamespace serviceNameSpace = new CodeNamespace(namespaceTemp);//【4】
//            //生成客户端代理类代码
//            CodeCompileUnit compilerUnit = new CodeCompileUnit();//【5】
//            //将新建的命名空间加入到编译程序集
//            compilerUnit.Namespaces.Add(serviceNameSpace);
//            serviceImporter.Import(serviceNameSpace, compilerUnit);
//            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
//            //设定编译器的参数
//            CompilerParameters compiler = new CompilerParameters();//【8】
//            compiler.GenerateExecutable = false;
//            compiler.GenerateInMemory = true;
//            compiler.ReferencedAssemblies.Add("System.dll");
//            compiler.ReferencedAssemblies.Add("System.XML.dll");
//            compiler.ReferencedAssemblies.Add("System.Web.Services.dll");
//            compiler.ReferencedAssemblies.Add("System.Data.dll");
//            //编译代理类
//            CompilerResults compileResult = provider.CompileAssemblyFromDom(compiler, compilerUnit);//【9】
//            if (compileResult.Errors.HasErrors)
//            {
//                StringBuilder stringBuilder = new StringBuilder();
//                foreach (CompilerError ce in compileResult.Errors)
//                {
//                    stringBuilder.Append(ce);
//                    stringBuilder.Append(Environment.NewLine);
//                }
//                throw new Exception(stringBuilder.ToString());
//            }
//            #endregion

//            //要生成的程序集
//            currentAssembly = compileResult.CompiledAssembly;
//            currentUrl = url;
//        }

//        private static void RefreshAssembly(string url)
//        {
//            if (currentUrl == url && currentAssembly != null)
//            {
//                return;
//            }
//            InitialAssembly(url);
//        }
//        /// <summary>
//        /// 动态调用WebService
//        /// </summary>
//        /// <param name="url">WebService地址</param>
//        /// <param name="classname">类名</param>
//        /// <param name="methodname">方法名(模块名)</param>
//        /// <param name="args">参数列表</param>
//        /// <returns>object</returns>
//        public static object InvokeWebService(string url, string classname, string methodname, object[] args)
//        {
//            RefreshAssembly(url);
            
//            Type serviceType = currentAssembly.GetType(namespaceTemp + "." + classname, true, true);
//            var obj = Activator.CreateInstance(serviceType);//【10】
//            MethodInfo methodInfor = serviceType.GetMethod(methodname);//【11】
//            return methodInfor.Invoke(obj, args);
//        }
//    }
//}