namespace newki_inventory_pallet.Services
{
        public interface IAwsStorageConfig
    {
        string AccessKey{get;set;}
        string SecretKey{get;set;}
        string BucketName{get;set;}
        string FilePath{get;set;}
    }
     public class AwsStorageConfig:IAwsStorageConfig
    {
        public string AccessKey{get;set;}
        public string SecretKey{get;set;}
        public string BucketName{get;set;}
        public string FilePath{get;set;}
    }
}