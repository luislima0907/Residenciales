using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

public partial class EstadoCuentum
{
    [Key]
    public int NumeroEstadoCuenta { get; set; }

    public int CodigoPersonaRol { get; set; }

    public int CodigoPersona { get; set; }

    public byte CodigoTipoRol { get; set; }

    public int NumeroCasa { get; set; }

    public int CodigoCluster { get; set; }

    public int CodigoSucursal { get; set; }

    public int CodigoSector { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime FechaGeneracion { get; set; }

    public DateOnly FechaCorte { get; set; }

    [Column(TypeName = "money")]
    public decimal TotalVencido { get; set; }

    [Column(TypeName = "money")]
    public decimal TotalPendiente { get; set; }

    public int MesesVencidos { get; set; }

    [ForeignKey("NumeroCasa, CodigoCluster, CodigoSucursal, CodigoSector")]
    [InverseProperty("EstadoCuenta")]
    public virtual Casa Casa { get; set; } = null!;

    [InverseProperty("NumeroEstadoCuentaNavigation")]
    public virtual ICollection<DetalleEstadoCuentum> DetalleEstadoCuenta { get; set; } = new List<DetalleEstadoCuentum>();

    [ForeignKey("CodigoPersonaRol, CodigoPersona, CodigoTipoRol")]
    [InverseProperty("EstadoCuenta")]
    public virtual PersonaRol PersonaRol { get; set; } = null!;
}
