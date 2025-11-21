using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ApplicationManager.Core.Models;

namespace ApplicationManager.Core.Repos
{
    public sealed class JsonFileApplicationRepo : IApplicationRepo
    {
        private readonly string _filePath;
        private readonly List<Application> _applications = new();
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };
        public JsonFileApplicationRepo(string filePath)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            LoadFromFile();
        }
        public IReadOnlyCollection<Application> GetAll() => _applications.AsReadOnly();
        public Application? GetById(int id) => _applications.FirstOrDefault(a => a.Id == id);
        public void Add(Application application)
        {
            if (application is null) throw new ArgumentNullException(nameof(application));
            _applications.Add(application);
            SaveToFile();
        }
        public void Update(Application application)
        {
            if (application is null) throw new ArgumentNullException(nameof(application));

            var index = _applications.FindIndex(a => a.Id == application.Id);
            if (index != -1)
            {
                _applications[index] = application;
                SaveToFile();
            }
        }
        public bool Remove(int id)
        {
            var app = GetById(id);
            if (app is null) return false;

            var removed = _applications.Remove(app);
            if (removed)
            {
                SaveToFile();
            }
            return removed;
        }
        private void LoadFromFile()
        {
            if (!File.Exists(_filePath))
                return;

            var json = File.ReadAllText(_filePath);
            if (string.IsNullOrWhiteSpace(json))
                return;

            var data = JsonSerializer.Deserialize<List<Application>>(json, _jsonOptions);
            if (data is not null)
            {
                _applications.Clear();
                _applications.AddRange(data);
            }
        }
        private void SaveToFile()
        {
            var json = JsonSerializer.Serialize(_applications, _jsonOptions);
            File.WriteAllText(_filePath, json);
        }
    }
}
