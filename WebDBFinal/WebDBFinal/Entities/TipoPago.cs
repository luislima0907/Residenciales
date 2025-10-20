using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("TipoPago")]
public partial class TipoPago
{
    [Key]
    public byte CodigoTipoPago { get; set; }

    [StringLength(45)]
    [Unicode(false)]
    public string Descripcion { get; set; } = null!;

    [InverseProperty("CodigoTipoPagoNavigation")]
    public virtual ICollection<DetalleEstadoCuentum> DetalleEstadoCuenta { get; set; } = new List<DetalleEstadoCuentum>();

    [InverseProperty("CodigoTipoPagoNavigation")]
    public virtual ICollection<DetallePagoDocumento> DetallePagoDocumentos { get; set; } = new List<DetallePagoDocumento>();
}
