using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationManager.Core.Models;
// REIKALAVIMAS: "Sukūrėte ir pritaikėte savo sąsają (interface) (0.5 t.)"
// IApplicationRepo apibrėžia aplikacijų saugojimo operacijas.
// Ją realizuoja JsonFileApplicationRepo.cs
namespace ApplicationManager.Core.Repos
{
    public interface IApplicationRepo
    {
        IReadOnlyCollection<Application> GetAll();
        Application? GetById(int id);
        void Add(Application application);
        void Update(Application application);
        bool Remove(int id);
    }
}
