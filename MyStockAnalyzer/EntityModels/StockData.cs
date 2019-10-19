namespace MyStockAnalyzer.EntityModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("StockData")]
    public partial class StockData
    {
        [Key]
        [StringLength(250)]
        public string StockId { get; set; }

        [StringLength(250)]
        public string StockName { get; set; }

        [StringLength(250)]
        public string Class { get; set; }

        [StringLength(250)]
        public string Industry { get; set; }


    
        public DateTime? Updated { get; set; }

        [StringLength(250)]
        public string WarrantTarget { get; set; }
    }
}
