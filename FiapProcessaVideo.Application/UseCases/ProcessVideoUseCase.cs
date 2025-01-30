﻿using FiapProcessaVideo.Domain;
using System.Drawing;
using System.IO.Compression;
using FFMpegCore;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Amazon.S3.Model;
using FiapProcessaVideo.Infrastructure.Messaging.Publishers;
using FiapProcessaVideo.Infrastructure.Messaging.Model;

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
        private readonly NotificationPublisher _notificationPublisher;

        public ProcessVideoUseCase(IAmazonS3 s3Client, NotificationPublisher notificationPublisher)
        {
            _s3Client = s3Client;
            _notificationPublisher = notificationPublisher;
            // Fetch bucket name from environment variables
            _bucketName = Environment.GetEnvironmentVariable("S3_BUCKET_NAME");
        }

        public async Task<string> Execute(Video video)
        {
            string videoKey = video.VideoKey;
            string projectRoot = Directory.GetCurrentDirectory();

            // 1. Baixar o arquivo de vídeo do S3
            string downloadPath = Path.Combine(projectRoot, "downloads");
            Directory.CreateDirectory(downloadPath);
            var videoPath = Path.Combine(downloadPath, videoKey);
            await DownloadFileFromS3Async(_bucketName, videoKey, videoPath);

            //// 2. Criar pasta temporária para os frames
            var outputFolder = Path.Combine(projectRoot, "snapshots");
            Directory.CreateDirectory(outputFolder);

            var snapshotsId = Guid.NewGuid().ToString();
            var newSnapshotsFolder = Path.Combine(outputFolder, snapshotsId);
            Directory.CreateDirectory(newSnapshotsFolder);

            //// 3. Processar o vídeo
            var videoInfo = FFProbe.Analyse(videoPath);
            var duration = videoInfo.Duration;
            var interval = TimeSpan.FromSeconds(20);

            for (var currentTime = TimeSpan.Zero; currentTime < duration; currentTime += interval)
            {
                Console.WriteLine($"Video Id: {video.Id} - Processing frame: {currentTime}");
                var outputPath = Path.Combine(newSnapshotsFolder, $"frame_at_{currentTime.TotalSeconds}.jpg");
                FFMpeg.Snapshot(videoPath, outputPath, new Size(1920, 1080), currentTime);
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

            VideoUploadedEvent videoUploadedEvent = 

            _notificationPublisher.PublishNotificationCreated()

            return zipKey; // Retorna o caminho do arquivo ZIP no S3
        }

        private async Task DownloadFileFromS3Async(string bucketName, string key, string filePath)
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            try
            {
                using (var response = await _s3Client.GetObjectAsync(request))
                {
                    await using var responseStream = response.ResponseStream;
                    await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                    await responseStream.CopyToAsync(fileStream);
                }
            }
            catch (AmazonS3Exception s3Exception)
            {
                // Handle specific S3 error, such as file not found
                if (s3Exception.ErrorCode == "NoSuchKey")
                {
                    Console.WriteLine($"File not found in the bucket. Key: {key}");
                }
                else
                {
                    Console.WriteLine($"Error accessing S3: {s3Exception.Message}");
                }
            }
            catch (Exception ex)
            {
                // Handle other general errors
                Console.WriteLine($"Unexpected error: {ex.Message}");
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
