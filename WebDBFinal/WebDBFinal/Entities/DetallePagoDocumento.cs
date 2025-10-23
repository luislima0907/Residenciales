using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[PrimaryKey("CodigoDetallePagoDocumento", "CodigoAplicacionDocumento")]
[Table("DetallePagoDocumento")]
public partial class DetallePagoDocumento
{
    [Key]
    public int CodigoDetallePagoDocumento { get; set; }

    [Key]
    public int CodigoAplicacionDocumento { get; set; }

    public byte CodigoTipoPago { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime FechaPago { get; set; }

    [Column(TypeName = "money")]
    public decimal MontoPagado { get; set; }

    [ForeignKey("CodigoAplicacionDocumento")]
    [InverseProperty("DetallePagoDocumentos")]
    public virtual AplicacionDocumento CodigoAplicacionDocumentoNavigation { get; set; } = null!;

    [ForeignKey("CodigoTipoPago")]
    [InverseProperty("DetallePagoDocumentos")]
    public virtual TipoPago CodigoTipoPagoNavigation { get; set; } = null!;

    [InverseProperty("DetallePagoDocumentoNavigation")]
    public virtual ICollection<DetalleEstadoCuentum> DetalleEstadoCuenta { get; set; } = new List<DetalleEstadoCuentum>();
}
