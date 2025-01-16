using Newtonsoft.Json;

namespace FiapProcessaVideo.WebApi.Model
{
    public class SnapshotRequest
    {
        [JsonProperty("zip_file_path")]  // Maps to 'zip_file_path' in JSON

        public string ZipFilePath { get; private set; }
    }
}
