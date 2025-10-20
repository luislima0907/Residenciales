using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

public partial class RegistroIngresoOSalidum
{
    [Key]
    public int CodigoEntradaOSalida { get; set; }

    public int CodigoMovimientoResidencial { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string TipoMovimiento { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime FechaHora { get; set; }

    [ForeignKey("CodigoMovimientoResidencial")]
    [InverseProperty("RegistroIngresoOSalida")]
    public virtual RegistroMovimientoResidencial CodigoMovimientoResidencialNavigation { get; set; } = null!;
}
