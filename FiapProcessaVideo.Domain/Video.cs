namespace FiapProcessaVideo.Domain
{
    public sealed class Video
    {
        // Diretório do arquivo a ser processado
        public string FilePath { get; private set; }

        // Duração do vídeo em segundos
        public TimeSpan Duration { get; private set; }

        // Pasta de saída para dos snapshots
        public string OutputFolder { get; private set; }

        // Intervalo entre os snapshots capturados
        public TimeSpan SnapshotInterval { get; private set; }

        // Data de processamento
        public DateTime ProcessedAt { get; private set; }

        // Status
        public string Status { get ; private set; }

        // Snapshot processados
        public List<Snapshot> Snapshots { get; private set; }

        // Construtor para inicialização
        private Video(string filePath, TimeSpan duration, TimeSpan snapshotInterval)
        {
            FilePath = filePath;
            Duration = duration;
            SnapshotInterval = snapshotInterval;
            Snapshots = new List<Snapshot>(); 
        }

        // Método Load para inicializar a instância do vídeo
        public static Video Load(string filePath, TimeSpan duration, TimeSpan snapshotInterval)
        {
            var video = new Video(filePath, duration, snapshotInterval);

            return video;
        }

        public void SetOutputFolder(string outputFolder)
        {
            OutputFolder = outputFolder;
        }

        // Método para adicionar um snapshot processado
        public void AddSnapshot(Snapshot snapshot)
        {
            Snapshots.Add(snapshot);
        }
    }

}
