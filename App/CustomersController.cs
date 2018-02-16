using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks
{
    [Route("api/[controller]")]
    public class CustomersController : APIController<int, Customer, AdventureWorksContext>
    {
        private static string keyProp = "CustomerId";
        private static string sqlTableName = "Sales.Customer";
        private static List<string> querableFields = new List<string>() {
                    "CustomerID",
                    "TerritoryID",
                    "AccountNumber",
                    "CustomerType",
                    "rowguid",
                    "ModifiedDate"
                };
        public CustomersController( AdventureWorksContext dbContext)
            :base(dbContext, keyProp, sqlTableName, querableFields)
        {

        }
    }
}