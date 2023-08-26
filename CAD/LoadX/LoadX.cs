using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;


//用到了System,System.IO,System.Reflection
namespace LoadX
{
    public class LoadX
    {
        private Action cmd;
        [CommandMethod("DD")]
        public void ReloadX()
        {
            //string dllName = "CadBasic.dll";//生成的需要调用的文件名
            //string className = "CadBasic.cmd";//空间名.类名
            //string methodName = "CmdSum";//method名

            CmdInfo cmdInfo = new CmdInfo();
            string jsonfile = @"C:\Users\yf\source\repos\CAD\LoadX\LoadDllInit.json";
            using (StreamReader file = File.OpenText(jsonfile))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject o = (JObject)JToken.ReadFrom(reader);
                    cmdInfo.DllName = o["DllName"].ToString();
                    cmdInfo.ClassName = o["ClassName"].ToString();
                    cmdInfo.MethodName = o["MethodName"].ToString();
                }

            }


            var adapterFileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);//获取当前目录
            var targetFilePath = Path.Combine(adapterFileInfo.DirectoryName, cmdInfo.DllName);//目录与文件名拼接
            var targetAssembly = Assembly.Load(File.ReadAllBytes(targetFilePath));//文件以二进制的方式加载到程序
            var targetType = targetAssembly.GetType(cmdInfo.ClassName);//定位指定的类
            var targetMethod = targetType.GetMethod(cmdInfo.MethodName);//定位指定的方法
            var targetObject = Activator.CreateInstance(targetType);//创建类
            cmd = () => targetMethod.Invoke(targetObject, null);//cmd指向类中的方法
            try
            {
                cmd?.Invoke();//执行方法
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Tips");
            }
        }

    }
    public class CmdInfo
    {
        public string DllName { get; set; }
        public string ClassName { get; set; }
        public string MethodName { get; set; }
        //public CmdInfo(string DllName, string ClassName, string MethodName)
        //{
        //    this.dllName = DllName;
        //    this.className = ClassName;
        //    this.methodName = MethodName;
        //}
    }
}