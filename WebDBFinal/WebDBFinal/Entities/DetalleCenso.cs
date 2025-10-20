using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[PrimaryKey("CodigoDetalleCenso", "CodigoCenso")]
[Table("DetalleCenso")]
public partial class DetalleCenso
{
    [Key]
    public int CodigoDetalleCenso { get; set; }

    [Key]
    public int CodigoCenso { get; set; }

    public int NumeroCasa { get; set; }

    public int CodigoCluster { get; set; }

    public int CodigoSucursal { get; set; }

    public int CodigoSector { get; set; }

    public int? CodigoPersonaRol { get; set; }

    public int? CodigoPersona { get; set; }

    public byte? CodigoTipoRol { get; set; }

    [ForeignKey("NumeroCasa, CodigoCluster, CodigoSucursal, CodigoSector")]
    [InverseProperty("DetalleCensos")]
    public virtual Casa Casa { get; set; } = null!;

    [ForeignKey("CodigoCenso")]
    [InverseProperty("DetalleCensos")]
    public virtual Censo CodigoCensoNavigation { get; set; } = null!;

    [ForeignKey("CodigoPersonaRol, CodigoPersona, CodigoTipoRol")]
    [InverseProperty("DetalleCensos")]
    public virtual PersonaRol? PersonaRol { get; set; }
}
