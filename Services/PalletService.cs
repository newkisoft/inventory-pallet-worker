using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using newki_inventory_pallet.Helpers;
using newkilibraries;
using newki_inventory_pallet.Extentions;
using System.Text.Json;
using System;

namespace newki_inventory_pallet.Services
{
    public interface IPalletService
    {

        IEnumerable<Pallet> Get();
        List<Pallet> GetAvailablePallets();
        Pallet GetPallet(int id);
        List<Pallet> GetPallet(List<string> keywords);
        Pallet Insert(Pallet pallet);
        Pallet UpdateAsync(Pallet pallet);
        void Print(int id, string customerName);
        Pallet Remove(int id);
        List<string> GetWarehouseFilters();
        List<string> GetYarnTypeFilters();
        void UpdateFilters();
    }
    public class PalletService : IPalletService
    {
        ApplicationDbContext _context;
        IAwsService _awsService;

        public PalletService(ApplicationDbContext context, IAwsService awsService)
        {
            _context = context;
            _awsService = awsService;
        }

        public IEnumerable<Pallet> Get()
        {
            return _context.Pallet.OrderByDescending(p => p.PalletId);
        }

        public List<Pallet> GetAvailablePallets()
        {
            return _context.Pallet.Where(p => p.Sold == false).ToList();

        }
        public Pallet GetPallet(int id)
        {
            return _context.Pallet
            .FirstOrDefault(p => p.PalletId == id);
        }

        public List<Pallet> GetPallet(List<string> keywords)
        {
            var keyword = "";
            foreach (var key in keywords)
            {
                keyword = $"{keyword}{key},";
            }

            List<Pallet> Items = new List<Pallet>();
            var hasWarehouses = false;
            var hasYarnType = false;
            var hasSold = false;

            var wareHouses = GetWarehouseFilters();
            var yarnTypes = GetYarnTypeFilters();

            hasWarehouses = wareHouses.Any(p => keywords.Contains(p));

            hasYarnType = yarnTypes.Any(p => keywords.Contains(p));

            hasSold = keywords.Contains("Sold") || keywords.Contains("NotSold");
            if (!hasWarehouses)
            {
                keyword = $"{keyword}{string.Join(",", wareHouses)}";
            }
            if (!hasYarnType)
            {
                keyword = $"{keyword}{string.Join(",", yarnTypes)}";
            }
            if (!hasSold)
            {
                keyword = $"{keyword}False,True,";
            }
            if (hasSold)
            {
                if (keywords.Contains("Sold"))
                    keyword = $"{keyword}True,";
                if (keywords.Contains("NotSold"))
                    keyword = $"{keyword}False,";
            }

            return _context.Pallet.Where(p => keyword.Contains(p.Warehouse)).ToList().Where(p =>
                       keyword.Contains(p.YarnType) && keyword.Contains(p.Sold.ToString())).OrderByDescending(p => p.PalletId).ToList();
        }

        public Pallet Insert(Pallet pallet)
        {
            if (_context.Pallet.FirstOrDefault(p => p.PalletId == pallet.PalletId) == null)
            {
                _context.Pallet.Add(pallet);
            }
            _context.SaveChanges();
            return pallet;
        }

        public Pallet UpdateAsync(Pallet pallet)
        {
            Console.WriteLine(pallet.PalletId);
            var existingPallet = _context.Pallet.Find(pallet.PalletId);
            Console.WriteLine(existingPallet.PalletId);
            if (!string.IsNullOrEmpty(pallet.Image))
            {
                var imageStream = _awsService.DownloadFileAsync(pallet.Image).Result;
                if (imageStream != null)
                {
                    var image = ImageHelper.ResizeImage(imageStream.ResponseStream.ToBytes(), new System.Drawing.Size(600, 800));
                    using (var stream = new MemoryStream())
                    {
                        var imageName = Path.GetFileName(pallet.Image);
                        pallet.ThumbnailImage = $"thumbnail-{imageName}";
                        image.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                        _awsService.UploadFile(pallet.ThumbnailImage, stream);
                    }
                }
            }
            _context.Entry(existingPallet).CurrentValues.SetValues(pallet);
            _context.SaveChanges();
            // _loggingService.Log(User.Identity.Name, LogAction.UPDATE, pallet);
            return pallet;
        }
        public void Print(int id, string customerName)
        {
            var pallet = _context.Pallet.FirstOrDefault(p => p.PalletId == id);
            var spltDetails = pallet.PalletName.Split(' ');

            var color = spltDetails.Length > 2 ? spltDetails[2] : " ";
            var colorCode = spltDetails.Length > 3 ? spltDetails[3] : " ";
            var denier = spltDetails.Length > 4 ? spltDetails[4] : " ";
            var type = spltDetails.Length > 1 ? spltDetails[1] : " ";

            string command = $"python3 htmltopdf.py {customerName} {color} {colorCode} {denier} {type} {pallet.Barcode} {pallet.Weight} {pallet.PalletId}";
            string result = "";


            using (System.Diagnostics.Process proc = new System.Diagnostics.Process())
            {
                proc.StartInfo.FileName = "/bin/bash";
                proc.StartInfo.Arguments = "-c \" " + command + " \"";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.Start();

                result += proc.StandardOutput.ReadToEnd();
                result += proc.StandardError.ReadToEnd();

                proc.WaitForExit();
            }
        }

        public Pallet Remove(int id)
        {
            var pallet = _context.Pallet
                .FirstOrDefault(x => x.PalletId == id);
            _context.Pallet.Remove(pallet);
            _context.SaveChanges();
            //_loggingService.Log(User.Identity.Name, LogAction.DELETE, pallet);
            return pallet;
        }
        public List<string> GetWarehouseFilters()
        {
            var wareHouses = _context.Pallet.Select(p => p.Warehouse).Distinct().ToList();
            wareHouses.RemoveAll(string.IsNullOrEmpty);
            return wareHouses;

        }
        public List<string> GetYarnTypeFilters()
        {
            var yarnTypes = _context.Pallet.Select(p => p.YarnType).Distinct().ToList();
            yarnTypes.RemoveAll(string.IsNullOrEmpty);
            return yarnTypes;

        }

        public void UpdateFilters()
        {
            _context.PalletFilter.RemoveRange(_context.PalletFilter);
            var wareHouses = GetWarehouseFilters();
            var wareHouseFilter = new PalletFilter();
            wareHouseFilter.ColumnName = "Warehouse";
            wareHouseFilter.Keywords = JsonSerializer.Serialize(wareHouses);
            _context.PalletFilter.Add(wareHouseFilter);

            var yarnTypes = GetYarnTypeFilters();
            var yarnTypesFilter = new PalletFilter();
            yarnTypesFilter.ColumnName = "YarnType";
            yarnTypesFilter.Keywords = JsonSerializer.Serialize(yarnTypes);
            _context.PalletFilter.Add(yarnTypesFilter);

            var soldFilter = new PalletFilter();
            soldFilter.ColumnName = "Sold";
            var soldTypes = new List<bool> { true, false };
            soldFilter.Keywords = JsonSerializer.Serialize(soldTypes);
            _context.PalletFilter.Add(soldFilter);

            var deliverFilter = new PalletFilter();
            deliverFilter.ColumnName = "IsDelivered";
            var deliverTypes = new List<bool> { true, false };
            deliverFilter.Keywords = JsonSerializer.Serialize(deliverTypes);
            _context.PalletFilter.Add(deliverFilter);

            _context.SaveChanges();

        }
    }
}