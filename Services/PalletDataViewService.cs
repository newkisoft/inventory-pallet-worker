using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using MongoDB.Driver;
using newkilibraries;

namespace newki_inventory_pallet.Services
{
    public interface IPalletDataViewService
    {   
        void Delete(int id);
        void Insert(PalletDataView pallet);
        void Update(PalletDataView pallet);
        PalletDataView Get(string id);
    }
    public class PalletDataViewService : IPalletDataViewService
    {
        private MongoClient _client ;
        private IMongoDatabase _database;
        private IMongoCollection<PalletDataView> _collection;
        private string _collectionName;

        public PalletDataViewService(string connectionString,string databaseName,string collectionName)
        {
            _client = new MongoClient(connectionString);
            _database = _client.GetDatabase(databaseName);
            _collection = _database.GetCollection<PalletDataView>(collectionName);
            _collectionName= collectionName;
        }

        public void Insert(PalletDataView pallet)
        {
            _collection.InsertOne(pallet);
        }        

        public void Update(PalletDataView pallet)
        {
            var filter = Builders<PalletDataView>.Filter.Eq("Id",pallet.Id);
            _collection.ReplaceOne(filter, pallet);
        }

        public void Delete(int id)
        {
            var pallets = _collection.Find(p=>p.Pallet.PalletId == id).ToList<PalletDataView>();
            foreach(var pallet in pallets)
            {
                var filter = Builders<PalletDataView>.Filter.Eq("Id",pallet.Id);
                _collection.DeleteOne(filter);
            }
        }

        public void Clear()
        {            
            _collection.Database.DropCollection(_collectionName);
        }

        public PalletDataView Get(string id)
        {
            return _collection.Find(p=>p.Id == id).FirstOrDefault();
        }
    }
}