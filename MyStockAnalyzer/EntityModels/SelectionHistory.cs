namespace MyStockAnalyzer.EntityModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SelectionHistory")]
    public partial class SelectionHistory
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        [StringLength(250)]
        public string StockId { get; set; }

        [StringLength(250)]
        public string MethodName { get; set; }

        public string Memo { get; set; }
    }
}
