namespace MyStockAnalyzer.EntityModels
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class StockEntities : DbContext
    {
        public StockEntities()
            : base("name=StockEntities")
        {
        }

        public virtual DbSet<Memo> Memo { get; set; }
        public virtual DbSet<SelectionHistory> SelectionHistory { get; set; }
        public virtual DbSet<StockData> StockData { get; set; }
        public virtual DbSet<StockDividend> StockDividend { get; set; }
        public virtual DbSet<StockPrice> StockPrice { get; set; }
        public virtual DbSet<EtfStock> EtfStock { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
