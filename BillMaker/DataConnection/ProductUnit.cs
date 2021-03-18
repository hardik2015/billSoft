//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BillMaker.DataConnection
{
    using System;
    using System.Collections.Generic;
    
    public partial class ProductUnit
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ProductUnit()
        {
            this.order_details = new HashSet<order_details>();
            this.StockLogs = new HashSet<StockLog>();
        }
    
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string UnitName { get; set; }
        public int Conversion { get; set; }
        public decimal Stock { get; set; }
        public bool IsBasicUnit { get; set; }
        public bool IsPurchaseUnit { get; set; }
        public decimal UnitBuyPrice { get; set; }
        public decimal UnitSellPrice { get; set; }
        public bool IsActive { get; set; }
    
        public virtual Product Product { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<order_details> order_details { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StockLog> StockLogs { get; set; }
    }
}
