//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BillMaker.DataLib
{
    using System;
    using System.Collections.Generic;
    
    public partial class Product
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Product()
        {
            this.order_details = new HashSet<order_details>();
            this.ProductUnits = new HashSet<ProductUnit>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string HSNCode { get; set; }
        public decimal Cgst { get; set; }
        public decimal Sgst { get; set; }
        public string description { get; set; }
        public bool IsRawMaterial { get; set; }
        public bool IsProduct { get; set; }
        public bool IsUnitsConnected { get; set; }
        public bool IsActive { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<order_details> order_details { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductUnit> ProductUnits { get; set; }
    }
}