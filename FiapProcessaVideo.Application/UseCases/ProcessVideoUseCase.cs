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
        private readonly string _bucketName;

        public ProcessVideoUseCase(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;

            // Fetch bucket name from environment variables
            _bucketName = Environment.GetEnvironmentVariable("S3_BUCKET_NAME");
        }

        public async Task<string> Execute(Video video)
        {
            string videoKey = video.VideoKey;
            string projectRoot = Directory.GetCurrentDirectory();

            // 1. Generate the presigned URL for the video file in S3
            string videoUrl = await GeneratePresignedUrlAsync(_bucketName, videoKey);

            // 2. Download the file directly using the presigned URL
            string downloadPath = Path.Combine(projectRoot, "downloads");

            // 1. Baixar o arquivo de vídeo do S3
            //string downloadPath = Path.Combine(projectRoot, "downloads");
            Directory.CreateDirectory(downloadPath);
            var videoPath = Path.Combine(downloadPath, videoKey);

            await DownloadFileFromUrlAsync(videoUrl, videoPath);
            //await DownloadFileFromS3Async(_bucketName, videoKey, videoPath);

            //// 2. Criar pasta temporária para os frames
            var outputFolder = Path.Combine(projectRoot, "snapshots");
            Directory.CreateDirectory(outputFolder);

            var snapshotsId = Guid.NewGuid().ToString();
            var newSnapshotsFolder = Path.Combine(outputFolder, snapshotsId);
            Directory.CreateDirectory(newSnapshotsFolder);

            //// 3. Processar o vídeo
            // var videoInfo = FFProbe.Analyse(videoPath);
            // var duration = videoInfo.Duration;
            // var interval = TimeSpan.FromSeconds(20);

            var tasks = new List<Task>();

            // 4. Stream the file and take snapshots simultaneously
            using (var client = new HttpClient())
            using (var stream = await client.GetStreamAsync(videoUrl))  // Stream from the presigned URL
            {
                // Assuming FFmpeg accepts a stream as input for processing
                var videoInfo = FFProbe.AnalyseStream(stream);  // Use FFProbe with the stream to get duration etc.
                var duration = videoInfo.Duration;
                var interval = TimeSpan.FromSeconds(20);

                for (var currentTime = TimeSpan.Zero; currentTime < duration; currentTime += interval)
                {
                    var timeForTask = currentTime;
                    Console.WriteLine($"Processando frame: {currentTime}");
                    var outputPath = Path.Combine(newSnapshotsFolder, $"frame_at_{currentTime.TotalSeconds}.jpg");

                    tasks.Add(Task.Run(() =>
                    {
                        try
                        {
                            // Capture snapshot at a specific time in the stream
                            FFMpeg.SnapshotFromStream(stream, outputPath, new Size(1920, 1080), timeForTask);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing frame at {currentTime}: {ex.Message}");
                        }
                    }));
                }

                // Wait for all tasks to complete (snapshot processing)
                await Task.WhenAll(tasks);
            }

            // 4. Criar arquivo e diretório dos arquivos ZIP
            string zipFileName = $"images-{Guid.NewGuid()}.zip";
            string zipDirectory = Path.Combine(projectRoot, "zips");
            string zipFilePath = Path.Combine(zipDirectory, zipFileName);
            Directory.CreateDirectory(zipDirectory);
            ZipFile.CreateFromDirectory(newSnapshotsFolder, zipFilePath);

            // 5. Fazer upload do ZIP para o S3
            var zipKey = Path.Combine("processed", zipFileName);
            await UploadFileToS3Async(_bucketName, zipKey, zipFilePath);

            // 6. Limpeza de arquivos temporários
            File.Delete(videoPath);
            File.Delete(zipFilePath);
            Directory.Delete(newSnapshotsFolder, true);

            return zipKey; // Retorna o caminho do arquivo ZIP no S3
        }

        private async Task<string> GeneratePresignedUrlAsync(string bucketName, string key)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = key,
                Expires = DateTime.UtcNow.AddMinutes(15) // Set expiration as needed
            };

            string url = _s3Client.GetPreSignedURL(request);
            return url;
        }

        private async Task DownloadFileFromUrlAsync(string fileUrl, string filePath)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(fileUrl);
                response.EnsureSuccessStatusCode();

                await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                await response.Content.CopyToAsync(fileStream);
            }
        }

        // private async Task DownloadFileFromS3Async(string bucketName, string key, string filePath)
        // {
        //     var request = new GetObjectRequest
        //     {
        //         BucketName = bucketName,
        //         Key = key
        //     };

        //     using (var response = await _s3Client.GetObjectAsync(request))
        //     {
        //         await using var responseStream = response.ResponseStream;
        //         await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        //         await responseStream.CopyToAsync(fileStream);
        //     }
        // }

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
