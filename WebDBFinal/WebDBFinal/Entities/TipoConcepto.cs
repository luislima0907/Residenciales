using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("TipoConcepto")]
public partial class TipoConcepto
{
    [Key]
    public byte CodigoTipoConcepto { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Descripcion { get; set; } = null!;

    [Column(TypeName = "money")]
    public decimal MontoFijo { get; set; }

    [InverseProperty("CodigoTipoConceptoNavigation")]
    public virtual ICollection<DetalleCargoMensual> DetalleCargoMensuals { get; set; } = new List<DetalleCargoMensual>();
}
