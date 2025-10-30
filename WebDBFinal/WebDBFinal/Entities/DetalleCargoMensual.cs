using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[PrimaryKey("CodigoDetalleCargoMensual", "CodigoCargoMensual", "CodigoTipoCargo")]
[Table("DetalleCargoMensual")]
public partial class DetalleCargoMensual
{
    [Key]
    public int CodigoDetalleCargoMensual { get; set; }

    [Key]
    public int CodigoCargoMensual { get; set; }

    [Key]
    public byte CodigoTipoCargo { get; set; }


    [Column(TypeName = "decimal(10, 2)")]
    public decimal Unidades { get; set; }

    [Column(TypeName = "money")]
    public decimal ValorTotal { get; set; }
    
    [ForeignKey("CodigoCargoMensual")]
    [InverseProperty("DetalleCargoMensuals")]
    public virtual CargoMensualCasa CodigoCargoMensualNavigation { get; set; } = null!;

    [ForeignKey("CodigoTipoCargo")]
    [InverseProperty("DetalleCargoMensuals")]
    public virtual TipoCargo CodigoTipoCargoNavigation { get; set; } = null!;
}
