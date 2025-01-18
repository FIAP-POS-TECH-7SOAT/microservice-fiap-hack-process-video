using FiapProcessaVideo.Domain;
using System.Drawing;
using System.IO.Compression;
using FFMpegCore;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Amazon.S3.Model;

namespace FiapProcessaVideo.Application.UseCases
{
    public interface IProcessVideoUseCase
    {
        Task<string> Execute(Video video);
    }

    public class ProcessVideoUseCase : IProcessVideoUseCase
    {
        private readonly IAmazonS3 _s3Client;

        public ProcessVideoUseCase(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }
        
        public async Task<string> Execute(Video video)
        {
            string bucketName = "";
            string videoKey = "Marvel_DOTNET_CSHARP.mp4";

            // 1. Baixar o arquivo de vídeo do S3
            var videoPath = Path.Combine(video.FilePath, videoKey);
            await DownloadFileFromS3Async(bucketName, videoKey, videoPath);

            //// 2. Criar pasta temporária para os frames
            var outputFolder = Path.Combine(video.FilePath, "snapshots");
            Directory.CreateDirectory(outputFolder);

            //// 3. Processar o vídeo
            var videoInfo = FFProbe.Analyse(videoPath);
            var duration = videoInfo.Duration;
            var interval = TimeSpan.FromSeconds(20);

            for (var currentTime = TimeSpan.Zero; currentTime < duration; currentTime += interval)
            {
                Console.WriteLine($"Processando frame: {currentTime}");
                var outputPath = Path.Combine(outputFolder, $"frame_at_{currentTime.TotalSeconds}.jpg");
                FFMpeg.Snapshot(videoPath, outputPath, new Size(1920, 1080), currentTime);
            }

            //// 4. Criar arquivo ZIP
            //var zipFilePath = Path.Combine(Path.GetTempPath(), "images.zip");
            //ZipFile.CreateFromDirectory(outputFolder, zipFilePath);

            //// 5. Fazer upload do ZIP para o S3
            //var zipKey = Path.Combine("processed", "images.zip");
            //await UploadFileToS3Async(bucketName, zipKey, zipFilePath);

            //// 6. Limpeza de arquivos temporários
            //File.Delete(videoPath);
            //Directory.Delete(outputFolder, true);
            //File.Delete(zipFilePath);

            return "zipKey"; // Retorna o caminho do arquivo ZIP no S3
        }

        private async Task DownloadFileFromS3Async(string bucketName, string key, string filePath)
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            using (var response = await _s3Client.GetObjectAsync(request))
            {
                await using var responseStream = response.ResponseStream;
                await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                await responseStream.CopyToAsync(fileStream);
            }
        }

        private async Task UploadFileToS3Async(string bucketName, string key, string filePath)
        {
            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                FilePath = filePath
            };

            await _s3Client.PutObjectAsync(putRequest);
        }
    }
}
