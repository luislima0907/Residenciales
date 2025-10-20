using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[PrimaryKey("CodigoDetalleEstadoCuenta", "NumeroEstadoCuenta")]
public partial class DetalleEstadoCuentum
{
    [Key]
    public int CodigoDetalleEstadoCuenta { get; set; }

    [Key]
    public int NumeroEstadoCuenta { get; set; }

    public int CodigoCargoMensual { get; set; }

    public byte MesCargo { get; set; }

    public int AnioCargo { get; set; }

    public DateOnly FechaSugeridaPago { get; set; }

    public byte CodigoTipoPago { get; set; }

    [Column(TypeName = "money")]
    public decimal ValorPendiente { get; set; }

    [ForeignKey("CodigoCargoMensual")]
    [InverseProperty("DetalleEstadoCuenta")]
    public virtual CargoMensualCasa CodigoCargoMensualNavigation { get; set; } = null!;

    [ForeignKey("CodigoTipoPago")]
    [InverseProperty("DetalleEstadoCuenta")]
    public virtual TipoPago CodigoTipoPagoNavigation { get; set; } = null!;

    [ForeignKey("NumeroEstadoCuenta")]
    [InverseProperty("DetalleEstadoCuenta")]
    public virtual EstadoCuentum NumeroEstadoCuentaNavigation { get; set; } = null!;
}
