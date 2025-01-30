using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapProcessaVideo.Infrastructure.Messaging.Model.Shared
{
    public class AmqpQueues
    {
        [JsonProperty("FILE_QUEUE")]
        public FileQueue FileQueue { get; set; }
    }
}
