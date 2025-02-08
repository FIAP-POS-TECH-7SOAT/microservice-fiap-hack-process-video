Feature: VideoUploadSubscriber

Video Processing Pipeline: Download video from AWS S3, process it, 
upload a .zip file with snapshots from the video, 
and publish status messages to RabbitMQ.

@scenario1
Scenario: Successful Video Processing
    Given the processing service detects the new video
    When the system generates a ZIP file of the extracted frames
    Then the ZIP file should be uploaded to the S3 bucket
    Then the system should return a public URL of the ZIP file
    And send a notification to RabbitMQ