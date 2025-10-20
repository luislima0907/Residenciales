using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("PeriodoFacturacionSeguridad")]
public partial class PeriodoFacturacionSeguridad
{
    [Key]
    public int CodigoPeriodoFacturacion { get; set; }

    public int CodigoSucursal { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    [ForeignKey("CodigoSucursal")]
    [InverseProperty("PeriodoFacturacionSeguridads")]
    public virtual Sucursal CodigoSucursalNavigation { get; set; } = null!;

    [InverseProperty("CodigoPeriodoFacturacionNavigation")]
    public virtual ICollection<DetalleFacturacionSeguridad> DetalleFacturacionSeguridads { get; set; } = new List<DetalleFacturacionSeguridad>();
}
