namespace MyStockAnalyzer.EntityModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Memo")]
    public partial class Memo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MemoId { get; set; }

        public string MemoText { get; set; }
    }
}
