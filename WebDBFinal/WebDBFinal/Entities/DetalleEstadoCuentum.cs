using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[PrimaryKey("CodigoDetalleEstadoCuenta", "NumeroEstadoCuenta")]
[Table("DetalleEstadoCuenta")]
public partial class DetalleEstadoCuentum
{
  [Key]
  public int CodigoDetalleEstadoCuenta { get; set; }

  [Key]
  public int NumeroEstadoCuenta { get; set; }

  public int AnioCargo { get; set; }

  public byte MesCargo { get; set; }

  public DateOnly FechaSugeridaPago { get; set; }

  public int CodigoDetallePagoDocumento { get; set; }

  public int CodigoAplicacionDocumento { get; set; }

  [ForeignKey("NumeroEstadoCuenta")]
  [InverseProperty("DetalleEstadoCuenta")]
  public virtual EstadoCuentum NumeroEstadoCuentaNavigation { get; set; } = null!;

  [ForeignKey("CodigoDetallePagoDocumento, CodigoAplicacionDocumento")]
  [InverseProperty("DetalleEstadoCuenta")]
  public virtual DetallePagoDocumento DetallePagoDocumentoNavigation { get; set; } = null!;
}
