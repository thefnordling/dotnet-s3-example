using System;
using Amazon;
using Amazon.S3;
using Amazon.S3.Util;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Amazon.S3.Transfer;

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
        await p.ListBucketsAsync().ConfigureAwait(false);        
        var created = await p.CreateBucketAsync().ConfigureAwait(false);
        await p.PutCatInBucketAsync(created).ConfigureAwait(false);
        await p.ClearBucketAsync(created).ConfigureAwait(false);
        await p.DeleteBucketAsync(created).ConfigureAwait(false);
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

        if (!await AmazonS3Util.DoesS3BucketExistV2Async(S3, newBucket).ConfigureAwait(false))
        {
            var putbucket = new PutBucketRequest
            {
                BucketName = newBucket,
                UseClientRegion = false
            };
            var response = await S3.PutBucketAsync(putbucket).ConfigureAwait(false);
        }
        return newBucket;
    }
    public async Task ClearBucketAsync(string bucket)
    {

        var lor = new ListObjectsV2Request
        {
            BucketName = bucket
        };

        var response = await S3.ListObjectsV2Async(lor).ConfigureAwait(false);
        var getObjects = ContinueGettingObjectsAsync(response, Settings.MaxPageContinuations).ConfigureAwait(false);
        await foreach (var o in getObjects)
        {
            var dor = new Amazon.S3.Model.DeleteObjectRequest
            {
                BucketName = bucket,
                Key = o.Key
            };

            Console.WriteLine($"Deleting {dor.Key} from {dor.BucketName}");

            await S3.DeleteObjectAsync(dor).ConfigureAwait(false);
        }

    }
    public async Task ListBucketsAsync()
    {
        var list = await S3.ListBucketsAsync().ConfigureAwait(false);
        Console.WriteLine($"Listing Buckets");
        foreach (var b in list.Buckets)
        {
            Console.WriteLine($"Bucket {b.BucketName}, created at {b.CreationDate}");
            await ListObjectsAsync(b.BucketName).ConfigureAwait(false);
        }
    }
    protected async IAsyncEnumerable<Amazon.S3.Model.S3Object> ContinueGettingObjectsAsync(ListObjectsV2Response response, int maxDepth = 5)
    {
        maxDepth--;

        foreach (var entry in response.S3Objects)
        {
            yield return entry;
        }

        if (response.IsTruncated)
        {
            if (maxDepth > 0)
            {
                var lor = new ListObjectsV2Request
                {
                    BucketName = response.S3Objects.First().BucketName,
                    ContinuationToken = response.ContinuationToken
                };
                var recurse = ContinueGettingObjectsAsync(await S3.ListObjectsV2Async(lor).ConfigureAwait(false), maxDepth).ConfigureAwait(false);
                await foreach (var entry in recurse)
                {
                    yield return entry;
                }
            }
            else
            {
                throw new Exception($"This bucket is too deep; exceeded maximum page continuations");
            }
        }
    }
    public async Task ListObjectsAsync(string bucket)
    {
        Console.WriteLine($"Listing Objects in Bucket {bucket}");
        var lor = new ListObjectsV2Request
        {
            BucketName = bucket
        };

        var response = await S3.ListObjectsV2Async(lor).ConfigureAwait(false);
        var getObjects = ContinueGettingObjectsAsync(response, Settings.MaxPageContinuations).ConfigureAwait(false);
        await foreach (var o in getObjects)
        {
            Console.WriteLine($"Found Object {o.Key} in {o.BucketName}");
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
        var response = await S3.DeleteBucketAsync(delbucket).ConfigureAwait(false);
    }

    ///i know brunnhilde is not a chungus so i didn't need to take care about memory usage
    ///if you wanted to see a nice implementation of chunked streaming: https://github.com/mlhpdx/s3-upload-stream
    public async Task PutCatInBucketAsync(string bucket)
    {
        Console.WriteLine($"Putting Brunnhilde into a bucket.");
        var xu = new TransferUtility(S3);
        var xur = new TransferUtilityUploadRequest
        {
            InputStream = File.OpenRead(Settings.CatPicture),
            AutoCloseStream = true,
            BucketName = bucket,
            ContentType = "image/jpeg",
            Key = Settings.CatPicture
        };
        await xu.UploadAsync(xur).ConfigureAwait(false);
    }
}
