using Kinocot.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kinocot
{
    public class PluginLorder
    {
        // プラグインのリスト
        public List<IPlugin> plugins = new List<IPlugin>();
        private string path;

        public PluginLorder()
        {
            path = Directory.GetCurrentDirectory() + @"\Plugins";
            plugins = GetPlugins(path);
        }

        private List<IPlugin> GetPlugins(string path)
        {
            var plugins = new List<IPlugin>();
            // ディレクトリ内のDLLファイルパスを取得
            foreach (string dll in Directory.GetFiles(path, "*.dll"))
            {
                // ファイルパスからアセンブリを読み込む
                Assembly asm = Assembly.LoadFrom(dll);
                // アセンブリで定義されている型を取得
                foreach (Type type in asm.GetTypes())
                {
                    // 非クラス型、非パブリック型、抽象クラスはスキップ
                    if (!type.IsClass || !type.IsPublic || type.IsAbstract) continue;
                    // 型に実装されているインターフェイスから IPlugin を取得
                    Type t = type.GetInterfaces().FirstOrDefault((_t) => _t == typeof(IPlugin));
                    // default(IPlugin) と等しい場合は未実装なのでスキップ
                    if (t == default(IPlugin)) continue;
                    // 取得した型のインスタンスを作成
                    object obj = Activator.CreateInstance(type);
                    plugins.Add((IPlugin)obj);
                }
            }
            return plugins;
        }
    }
}
