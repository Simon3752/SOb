using CsvHelper.Configuration;
using System.Globalization;
using SOb.Models;
namespace SOb.Models
{
    public class ValueEntryMap : ClassMap<ValueEntry>
    {
        public ValueEntryMap()
        {
            Map(m => m.date).Name("Date");
            Map(m => m.executionTime).Name("ExecutionTime");
            Map(m => m.value).Name("Value");
        }
    }
}
