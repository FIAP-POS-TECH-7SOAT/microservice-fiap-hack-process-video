namespace FiapProcessaVideo.Domain
{
    public sealed class Snapshot
    {
        // Timestamp do vídeo no momento em que o snapshot foi tirado
        public TimeSpan Timestamp { get; private set; }

        // Caminho para o arquivo de imagem do frame
        public string FilePath { get; private set; }

        public Snapshot(TimeSpan timestamp, string filePath)
        {
            Timestamp = timestamp;
            FilePath = filePath;
        }
    }

}
