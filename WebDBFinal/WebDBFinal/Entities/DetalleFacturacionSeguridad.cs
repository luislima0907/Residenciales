using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[PrimaryKey("CodigoDetalleFacturacion", "CodigoPeriodoFacturacion")]
[Table("DetalleFacturacionSeguridad")]
public partial class DetalleFacturacionSeguridad
{
    [Key]
    public int CodigoDetalleFacturacion { get; set; }

    [Key]
    public int CodigoPeriodoFacturacion { get; set; }

    public int? CodigoMarcaje { get; set; }

    public int? CodigoControl { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal TotalHorasTrabajadas { get; set; }

    [Column(TypeName = "money")]
    public decimal TarifaPorHora { get; set; }

    [Column(TypeName = "money")]
    public decimal MontoAPagar { get; set; }
    
    [ForeignKey("CodigoMarcaje")]
    [InverseProperty("DetalleFacturacionSeguridads")]
    public virtual MarcajeLaboral? CodigoMarcajeNavigation { get; set; }

    [ForeignKey("CodigoPeriodoFacturacion")]
    [InverseProperty("DetalleFacturacionSeguridads")]
    public virtual PeriodoFacturacionSeguridad CodigoPeriodoFacturacionNavigation { get; set; } = null!;
}
