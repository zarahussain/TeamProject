using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks
{
    [Route("api/[controller]")]
    public class CustomersController : APIController<int, Customer, AdventureWorksContext>
    {
        private static AdvSearch advSearch = new AdvSearch() {
            SqlTableName = "Sales.Customer",
            KeyPropName = "CustomerId",
            QuerableFields = new List<string>() {
                    "CustomerID",
                    "TerritoryID",
                    "AccountNumber",
                    "CustomerType",
                    "rowguid",
                    "ModifiedDate"
                }
        };
        public CustomersController( AdventureWorksContext dbContext)
            :base(dbContext, advSearch)
        { }
    }
}