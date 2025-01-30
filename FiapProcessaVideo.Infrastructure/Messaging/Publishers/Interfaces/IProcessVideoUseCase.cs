using FiapProcessaVideo.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapProcessaVideo.Infrastructure.Messaging.Publishers.Interfaces
{
    public interface IProcessVideoUseCase
    {
        Task<string> Execute(Video video);
    }
}
