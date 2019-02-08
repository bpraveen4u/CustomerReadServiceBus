using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace CustomerReadServiceBus
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run([ServiceBusTrigger("CustomerCreatedQueue", AccessRights.Manage, Connection = "SBConn")]string myQueueItem, TraceWriter log)
        {
            log.Info($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
            Customer data = JsonConvert.DeserializeObject<Customer>(myQueueItem);

            //Write to sql server

            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = "Server=tcp:customerdomainsrv.database.windows.net,1433;Initial Catalog=CustomerDomain;Persist Security Info=False;User ID=custlogin;Password=Password@3;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;";
                conn.Open();
                var sqlQuery = string.Format("insert into Customers (Id, Name, Address, Phone) values({0}, '{1}', '{2}', '{3}')", data.Id, data.Name, data.Address, data.Phone);
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    int result = cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
    }
}
