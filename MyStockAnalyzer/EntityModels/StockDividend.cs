namespace MyStockAnalyzer.EntityModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("StockDividend")]
    public partial class StockDividend
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int StockId { get; set; }

        public double? Year { get; set; }

        public decimal? CashDividends { get; set; }

        public decimal? StockDividends { get; set; }
    }
}
