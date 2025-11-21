using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationManager.Core.Models;

namespace ApplicationManager.Core.Repos
{
    public sealed class InMemoryAllocationRepo : IApplicationRepo
    {
        private readonly List<Application> _applications = new();
        public IReadOnlyCollection<Application> GetAll() => _applications.AsReadOnly();
        public Application? GetById(int id) => _applications.FirstOrDefault(a => a.Id == id);
        public void Add(Application application)
        {
            _applications.Add(application);
        }
        public void Update(Application application)
        {
            var index = _applications.FindIndex(a => a.Id == application.Id);
            if (index != -1)
            {
                _applications[index] = application;
            }
        }
        public bool Remove(int id)
        {
            var application = GetById(id);
            return application != null && _applications.Remove(application);
        }
    }
}
