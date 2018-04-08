using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorks
{
    [Route("api/[controller]")]
    public class CustomersController : APIController<int, Customer, AdventureWorksContext>
    {
        private static Query advSearch = new Query() {
            SqlTableName = "Sales.Customer",
            KeyPropName = "CustomerId",
            FiltersFields = new List<string>() {
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