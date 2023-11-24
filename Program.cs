using System.Collections.Generic;
using System.Linq;
using Amazon.SQS;
using newkilibraries;
using Microsoft.Extensions.Configuration;
using newki_inventory_pallet.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Net.Http;
using System.Text;
using Amazon;
using System;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Threading;

namespace newki_inventory_pallet
{
    class Program
    {
        static ManualResetEvent _quitEvent = new ManualResetEvent(false);
        private static ServiceProvider serviceProvider;
        private static string _connectionString;
        private static IAwsService awsService;
        private static MongoDbConfiguration _mongoDbConfiguration = new MongoDbConfiguration();

        static void Main(string[] args)
        {

            //Reading configuration
            var pallets = new List<Pallet>();
            var awsStorageConfig = new AwsStorageConfig();
            var builder = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json", true, true);
            var Configuration = builder.Build();

            Configuration.GetSection("AwsStorageConfig").Bind(awsStorageConfig);
            _connectionString = Configuration.GetConnectionString("DefaultConnection");
            Configuration.GetSection("MongoDbConfiguration").Bind(_mongoDbConfiguration);

            var services = new ServiceCollection();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));
            services.AddTransient<IAwsService, AwsService>();
            services.AddTransient<IPalletService, PalletService>();
            services.AddSingleton<IAwsStorageConfig>(awsStorageConfig);

            var serviceProvider = services.BuildServiceProvider();
            awsService = serviceProvider.GetService<IAwsService>();

            var requestQueueName = "PalletRequest";
            var responseQueueName = "PalletResponse";

            ConnectionFactory factory = new ConnectionFactory();
            factory.UserName = "user";
            factory.Password = "password";
            factory.HostName = "localmq";

            var connection = factory.CreateConnection();

            var channel = connection.CreateModel();
            channel.QueueDeclare(requestQueueName, false, false, false);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                var updatePalletFullNameModel = JsonSerializer.Deserialize<InventoryMessage>(content);

                ProcessRequest(updatePalletFullNameModel);

            }; ;
            channel.BasicConsume(queue: requestQueueName,
                   autoAck: true,
                   consumer: consumer);


            _quitEvent.WaitOne();

        }

        private static void ProcessRequest(InventoryMessage inventoryMessage)
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseNpgsql(_connectionString);

                using (var appDbContext = new ApplicationDbContext(optionsBuilder.Options))
                {
                    var palletService = new PalletService(appDbContext, awsService);
                    var palletDataViewService = new PalletDataViewService(_mongoDbConfiguration.ConnectionString,"DataViews","Pallet");
                    var messageType = Enum.Parse<InventoryMessageType>(inventoryMessage.Command);

                    switch (messageType)
                    {
                        case InventoryMessageType.Search:
                            {
                                palletDataViewService.Clear();
                                var pallets = palletService.Get();
                                foreach (var pallet in pallets)
                                {
                                    var palletView = new PalletDataView{
                                      Pallet = pallet
                                    };
                                    palletDataViewService.Insert(palletView);
                                }
                                break;
                            }
                        case InventoryMessageType.Get:
                            {
                                Console.WriteLine("Loading a pallet...");
                                var id = JsonSerializer.Deserialize<int>(inventoryMessage.Message);
                                var pallet = palletService.GetPallet(id);
                                var content = JsonSerializer.Serialize(pallet);

                                var responseMessageNotification = new InventoryMessage();
                                responseMessageNotification.Command = InventoryMessageType.Get.ToString();
                                responseMessageNotification.RequestNumber = inventoryMessage.RequestNumber;
                                responseMessageNotification.MessageDate = DateTimeOffset.UtcNow;

                                var inventoryResponseMessage = new InventoryMessage();
                                inventoryResponseMessage.Message = content;
                                inventoryResponseMessage.Command = inventoryMessage.Command;
                                inventoryResponseMessage.RequestNumber = inventoryMessage.RequestNumber;

                                Console.WriteLine("Sending the message back");

                                break;

                            }
                        case InventoryMessageType.Insert:
                            {
                                Console.WriteLine("Adding new pallet");
                                var pallet = JsonSerializer.Deserialize<PalletDataView>(inventoryMessage.Message);
                                palletService.Insert(pallet.Pallet);
                                palletDataViewService.Insert(pallet);                               
                                break;
                            }
                        case InventoryMessageType.Update:
                            {
                                Console.WriteLine("Updating a pallet");
                                var pallet = JsonSerializer.Deserialize<PalletDataView>(inventoryMessage.Message);
                                palletService.UpdateAsync(pallet.Pallet);     
                                palletDataViewService.Update(pallet);                                                          
                                break;
                            }
                        case InventoryMessageType.Delete:
                            {
                                Console.WriteLine("Deleting a pallet");
                                var id = JsonSerializer.Deserialize<int>(inventoryMessage.Message);
                                palletService.Remove(id);                                
                                palletDataViewService.Delete(id);
                                break;
                            }
                        case InventoryMessageType.Print:
                            {
                                Console.WriteLine("Printing");
                                var palletPrint = JsonSerializer.Deserialize<PalletPrint>(inventoryMessage.Message);
                                palletService.Print(palletPrint.Id, palletPrint.CustomerName);                                
                                break;
                            }
                        default: break;

                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

        }
    }
}
