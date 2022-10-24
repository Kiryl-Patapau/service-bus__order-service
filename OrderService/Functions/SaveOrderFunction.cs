using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using OrderReservator.Models;

namespace OrderService.Functions;

public class SaveOrderFunction
{
    [FunctionName("SaveOrderFunction")]
    public static async Task Run(
        [ServiceBusTrigger("sbq-savingorder", Connection = "ServiceBusConnectionString")] string message,
        IBinder binder)
    {
        var order = JsonConvert.DeserializeObject<Order>(message);
        var serializedOrder = JsonConvert.SerializeObject(order);

        // Dynamic binding is used to avoid creating empty blobs in case of invalid order
        var blobAttribute = new BlobAttribute("orders/{datetime:yyyy-MM-dd}/{rand-guid}.json")
        {
            Access = FileAccess.Write,
            Connection = "OrdersStorage"
        };
        var blob = binder.Bind<BlobClient>(blobAttribute);
        await blob.UploadAsync(new BinaryData(serializedOrder));
    }
}
