using System;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace AWSHelper;

public class AmazonS3Operations {
    private static AmazonS3Client? client;

    public AmazonS3Operations() {
        AmazonS3Config config = new AmazonS3Config();
        client = new AmazonS3Client(
            Environment.GetEnvironmentVariable("accessKey"),
            Environment.GetEnvironmentVariable("secretKey"),
            config
        );
    }

    public static async Task<bool> CreateBucket(string bucketName) {
        try {
            var request = new PutBucketRequest
            {
                BucketName = bucketName,
                UseClientRegion = true,
            };

            var response = await client!.PutBucketAsync(request);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        } catch {
            return false;
        }
    }

    public static async Task<bool> BucketExists(string bucketName) {
        try {
            var res = await client!.ListBucketsAsync();
            for (int i = 0; i < res.Buckets.Count; ++i) {
                if (res.Buckets[i].BucketName == bucketName) {
                    return true;
                }
            }
            return false;
        } catch {
            return false;
        }
    }

    private static async Task<bool> DeleteAllObjects(string bucketName) {
        try {
            ListObjectsRequest req = new ListObjectsRequest();
            req.BucketName = bucketName;
            ListObjectsResponse res = await client!.ListObjectsAsync(req);
            for (int i = 0; i < res.S3Objects.Count; ++i) {
                var deleteRes = await client!.DeleteObjectAsync(bucketName, res.S3Objects[i].Key);
            }
            return true;
        } catch {
            return false;
        }
    }

    public static async Task<bool> DeleteBucket(string bucketName) {
        try {
            var objRes = await DeleteAllObjects(bucketName);
            if (objRes) {
                DeleteBucketRequest delReq = new DeleteBucketRequest();
                delReq.BucketName = bucketName;
                var delBucket = await client!.DeleteBucketAsync(delReq);
                return delBucket.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            return false;
        } catch {
            return false;
        }
    }

    public static async Task<bool> AddRecordingToBucket(string bucketName, string filePath) {
        try {
            var transferUtil = new TransferUtility(client);
            await transferUtil.UploadAsync(filePath, bucketName, new CancellationToken());
            return true;
        } catch {
            return false;
        }
    }

    public static async Task<bool> DownloadVideoFromBucket(string bucketName, string objectName, string filePath) {
        try {
            var transferUtil = new TransferUtility(client);
            TransferUtilityDownloadRequest req = new TransferUtilityDownloadRequest {
                BucketName = bucketName,
                Key = objectName,
                FilePath = filePath,
            };
            await transferUtil.DownloadAsync(req);
            return !File.Exists(filePath);
        } catch {
            return false;
        }
    }
}
