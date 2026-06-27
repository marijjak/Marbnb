using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Web.Models;

namespace Web.Data
{
    public class JsonStore<T> where T : BaseEntity
    {
        private readonly string _filePath;
        private readonly object _lock = new object();

        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            DateFormatString = "dd/MM/yyyy",
            Converters = { new StringEnumConverter() }
        };

        public JsonStore(string filePath)
        {
            _filePath = filePath;
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            if (!File.Exists(_filePath))
                File.WriteAllText(_filePath, "[]");
        }

        private List<T> ReadAll()
        {
            var json = File.ReadAllText(_filePath);
            return JsonConvert.DeserializeObject<List<T>>(json, Settings) ?? new List<T>();
        }

        private void WriteAll(List<T> items)
        {
            File.WriteAllText(_filePath, JsonConvert.SerializeObject(items, Settings));
        }

        public List<T> GetAll(bool includeDeleted = false)
        {
            lock (_lock)
            {
                var all = ReadAll();
                return includeDeleted ? all : all.Where(x => !x.IsDeleted).ToList();
            }
        }

        public T GetById(int id, bool includeDeleted = false)
        {
            lock (_lock)
            {
                return ReadAll().FirstOrDefault(x => x.Id == id && (includeDeleted || !x.IsDeleted));
            }
        }

        public T Add(T entity)
        {
            lock (_lock)
            {
                var all = ReadAll();
                entity.Id = all.Count == 0 ? 1 : all.Max(x => x.Id) + 1;
                all.Add(entity);
                WriteAll(all);
                return entity;
            }
        }

        public void Update(T entity)
        {
            lock (_lock)
            {
                var all = ReadAll();
                var idx = all.FindIndex(x => x.Id == entity.Id);
                if (idx >= 0)
                {
                    all[idx] = entity;
                    WriteAll(all);
                }
            }
        }

        public void SoftDelete(int id)
        {
            lock (_lock)
            {
                var all = ReadAll();
                var item = all.FirstOrDefault(x => x.Id == id);
                if (item != null)
                {
                    item.IsDeleted = true;
                    WriteAll(all);
                }
            }
        }

        public void SaveAll(List<T> items)
        {
            lock (_lock)
            {
                WriteAll(items);
            }
        }

        public bool IsEmpty()
        {
            lock (_lock)
            {
                return ReadAll().Count == 0;
            }
        }
    }
}
