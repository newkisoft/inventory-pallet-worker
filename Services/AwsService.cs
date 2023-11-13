using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace newki_inventory_pallet.Services
{
     public interface IAwsService
    {
        void UploadFile(string fileName,Stream stream);
         Task<GetObjectResponse> DownloadFileAsync(string fileName);
    }
    public class AwsService : IAwsService
    {
        private IAwsStorageConfig _awsStorageConfig;
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.USEast1;
        private static IAmazonS3 s3Client;
        

        public AwsService(IAwsStorageConfig awsStorageConfig)
        {
            _awsStorageConfig = awsStorageConfig;
        }

        public async Task<GetObjectResponse> DownloadFileAsync(string fileName)
        {
                var config = new AmazonS3Config();
                config.ServiceURL = _awsStorageConfig.FilePath;                
                config.RegionEndpoint = RegionEndpoint.USEast1;
                s3Client = new AmazonS3Client(_awsStorageConfig.AccessKey,
                        _awsStorageConfig.SecretKey,config);
                
              try
            {
                 GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = _awsStorageConfig.BucketName,
                    Key = fileName
                };                
                var response = await s3Client.GetObjectAsync(request).ConfigureAwait(true);              
                                    
                return response;               
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }

            return null;         
        }
        
        public void UploadFile(string fileName, Stream content)
        {

            var config = new AmazonS3Config();
            config.ServiceURL = _awsStorageConfig.FilePath;
            config.RegionEndpoint = RegionEndpoint.USEast1;

            s3Client = new AmazonS3Client(_awsStorageConfig.AccessKey,
                                _awsStorageConfig.SecretKey, config);
            UploadFileAsync(fileName, content);

        }
        private void UploadFileAsync(string fileName, Stream fileToUpload)
        {
            try
            {
                var fileTransferUtility =
                    new TransferUtility(s3Client);
                fileToUpload.Position = 0;
                fileTransferUtility.Upload(fileToUpload,
                                               _awsStorageConfig.BucketName,
                                               fileName);

            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }

        }       

    }
}