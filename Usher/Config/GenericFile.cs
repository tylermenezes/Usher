using System;
using System.IO;
using System.Reflection;
using YamlDotNet;

namespace Usher.Config
{
    public abstract class GenericFile<T>
        where T : GenericFile<T>, new()
    {
        private readonly string _path;
        string _content;

        private static readonly T _instance = new T();
        public static T Instance => _instance;

        protected GenericFile()
        {
            _path = GetPath();
            Reload();
        }

        public void Reload()
        {
            if (!Directory.Exists(Path.GetDirectoryName(_path))) {
                Directory.CreateDirectory(Path.GetDirectoryName(_path));
            }

            if (!File.Exists(_path)) {
                File.WriteAllText(_path, "");
            }

            _content = File.ReadAllText(_path);
            Deserialize(_content);
        }

        public void Save()
        {
            _content = Serialize();
            File.WriteAllText(_path, _content);
        }

        protected abstract string Serialize();
        protected abstract void Deserialize(string s);
        protected virtual string GetPath()
        {
            return Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "config", $"{typeof(T).Name.ToLower()}.yml"
            );
        }
    }
}