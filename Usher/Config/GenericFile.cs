using System;
using System.IO;
using System.Reflection;
using YamlDotNet;

namespace Usher.Config
{
    public abstract class GenericFile<T>
        where T : GenericFile<T>, new()
    {
        private string path;
        string content;

        private static T _instance = new T();
        public static T Instance
        {
            get
            {
                return _instance;
            }
        }

        protected GenericFile()
        {
            path = getPath();
            Reload();
        }

        public void Reload()
        {
            if (!Directory.Exists(Path.GetDirectoryName(path))) {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            if (!File.Exists(path)) {
                File.WriteAllText(path, "");
            }

            content = File.ReadAllText(path);
            deserialize(content);
        }

        public void Save()
        {
            content = serialize();
            File.WriteAllText(path, content);
        }

        protected abstract string serialize();
        protected abstract void deserialize(string s);
        protected virtual string getPath()
        {
            return Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "config", String.Format("{0}.yml", typeof(T).Name.ToLower())
            );
        }
    }
}