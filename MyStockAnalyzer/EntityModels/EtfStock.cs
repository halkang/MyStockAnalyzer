namespace MyStockAnalyzer.EntityModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("EtfStock")]
    public partial class EtfStock
    {
        public int id { get; set; }

        [StringLength(250)]
        public string EtfId { get; set; }

        [StringLength(250)]
        public string StockId { get; set; }
    }
}
