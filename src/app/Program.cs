using System;
using Amazon;
using Amazon.S3;
using Amazon.S3.Util;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;

namespace app;

public class Program
{
    async static Task Main(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json", true, true)
            .AddEnvironmentVariables();

        var config = builder.Build();

        var settings = new AppSettings();
        config.GetSection(AppSettings.CONFIG).Bind(settings);

        var p = new Program(settings);
        var created = await p.CreateBucketAsync();
        await p.ListBucketsAsync();
        await p.DeleteBucketAsync(created);


    }
    private AppSettings Settings { get; set; }
    private AmazonS3Client S3 { get; set; }
    public Program(AppSettings settings)
    {
        this.Settings = settings;

        var s3config = new AmazonS3Config();
        s3config.ServiceURL = settings.ServiceUrl;
        s3config.IgnoreConfiguredEndpointUrls = true;

        S3 = new AmazonS3Client(settings.AccessKey, settings.SecretKey, s3config);
    }

    public async Task<string> CreateBucketAsync()
    {
        var newBucket = Guid.NewGuid().ToString();

        Console.WriteLine($"Making new bucket {newBucket}");

        if (!await AmazonS3Util.DoesS3BucketExistV2Async(S3, newBucket))
        {
            var putbucket = new PutBucketRequest
            {
                BucketName = newBucket,
                UseClientRegion = false
            };
            var response = await S3.PutBucketAsync(putbucket);
        }
        return newBucket;
    }
    public async Task ListBucketsAsync()
    {
        var list = await S3.ListBucketsAsync();
        Console.WriteLine($"Listing Buckets");
        foreach (var b in list.Buckets)
        {
            Console.WriteLine($"Bucket {b.BucketName}, created at {b.CreationDate}");
        }

    }
    public async Task DeleteBucketAsync(string bucket)
    {
        Console.WriteLine($"Deleting a bucket {bucket}");

        var delbucket = new DeleteBucketRequest
        {
            BucketName = bucket,
            UseClientRegion = false
        };
        var response = await S3.DeleteBucketAsync(delbucket);
    }
}
