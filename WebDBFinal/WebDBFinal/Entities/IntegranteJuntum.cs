using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Index("CodigoJunta", "CodigoCluster", "CodigoSucursal", "CodigoSector", "CodigoTipoIntegrante", Name = "UK_IntegranteJunta_JuntaTipo", IsUnique = true)]
public partial class IntegranteJuntum
{
    [Key]
    public int CodigoIntegranteJunta { get; set; }

    public int CodigoJunta { get; set; }

    public int CodigoCluster { get; set; }

    public int CodigoSucursal { get; set; }

    public int CodigoSector { get; set; }

    public int CodigoPersonaRol { get; set; }

    public byte CodigoTipoRol { get; set; }

    public int CodigoPersona { get; set; }

    public byte CodigoTipoIntegrante { get; set; }

    public DateOnly FechaDesignacion { get; set; }

    [ForeignKey("CodigoTipoIntegrante")]
    [InverseProperty("IntegranteJunta")]
    public virtual TipoIntegrante CodigoTipoIntegranteNavigation { get; set; } = null!;

    [ForeignKey("CodigoJunta, CodigoCluster, CodigoSucursal, CodigoSector")]
    [InverseProperty("IntegranteJunta")]
    public virtual JuntaDirectiva JuntaDirectiva { get; set; } = null!;

    [ForeignKey("CodigoPersonaRol, CodigoPersona, CodigoTipoRol")]
    [InverseProperty("IntegranteJunta")]
    public virtual PersonaRol PersonaRol { get; set; } = null!;
}
