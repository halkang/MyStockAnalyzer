namespace MyStockAnalyzer.EntityModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("StockPrice")]
    public partial class StockPrice
    {
        [Key]
        [StringLength(250)]
        public string StockId { get; set; }

        public DateTime Date { get; set; }

        public decimal Open { get; set; }

        public decimal Close { get; set; }

        public decimal High { get; set; }

        public decimal Low { get; set; }

        public long Amount { get; set; }
    }
}
