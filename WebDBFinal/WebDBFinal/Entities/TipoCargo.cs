using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("TipoCargo")]
public partial class TipoCargo
{
    [Key]
    public byte CodigoTipoCargo { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Descripcion { get; set; } = null!;

    [Column(TypeName = "money")]
    public decimal MontoFijo { get; set; }

    [InverseProperty("CodigoTipoCargoNavigation")]
    public virtual ICollection<DetalleCargoMensual> DetalleCargoMensuals { get; set; } = new List<DetalleCargoMensual>();
}
