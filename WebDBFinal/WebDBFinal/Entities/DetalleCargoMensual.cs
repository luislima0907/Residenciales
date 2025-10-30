using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[PrimaryKey("CodigoDetalleCargoMensual", "CodigoCargoMensual", "CodigoTipoConcepto")]
[Table("DetalleCargoMensual")]
public partial class DetalleCargoMensual
{
    [Key]
    public int CodigoDetalleCargoMensual { get; set; }

    [Key]
    public int CodigoCargoMensual { get; set; }

    [Key]
    public byte CodigoTipoConcepto { get; set; }

    public int CodigoIntegranteJunta { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Unidades { get; set; }

    [Column(TypeName = "money")]
    public decimal ValorTotal { get; set; }
    
    [ForeignKey("CodigoCargoMensual")]
    [InverseProperty("DetalleCargoMensuals")]
    public virtual CargoMensualCasa CodigoCargoMensualNavigation { get; set; } = null!;

    [ForeignKey("CodigoTipoConcepto")]
    [InverseProperty("DetalleCargoMensuals")]
    public virtual TipoConcepto CodigoTipoConceptoNavigation { get; set; } = null!;
}
