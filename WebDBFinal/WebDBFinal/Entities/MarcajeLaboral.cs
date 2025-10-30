using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("MarcajeLaboral")]
public partial class MarcajeLaboral
{
    [Key]
    public int CodigoMarcaje { get; set; }

    public int CodigoPersonaRol { get; set; }

    public int CodigoPersona { get; set; }

    public byte CodigoTipoRol { get; set; }

    public int CodigoGaritaSeguridad { get; set; }

    public byte CodigoTipoMarcaje { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime FechaInicio { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime FechaFin { get; set; }

    [ForeignKey("CodigoGaritaSeguridad")]
    [InverseProperty("MarcajeLaborals")]
    public virtual GaritaSeguridad CodigoGaritaSeguridadNavigation { get; set; } = null!;

    [ForeignKey("CodigoTipoMarcaje")]
    [InverseProperty("MarcajeLaborals")]
    public virtual TipoMarcaje CodigoTipoMarcajeNavigation { get; set; } = null!;
    
    [InverseProperty("CodigoMarcajeNavigation")]
    public virtual ICollection<DetalleFacturacionSeguridad> DetalleFacturacionSeguridads { get; set; } = new List<DetalleFacturacionSeguridad>();

    [ForeignKey("CodigoPersonaRol, CodigoPersona, CodigoTipoRol")]
    [InverseProperty("MarcajeLaborals")]
    public virtual PersonaRol PersonaRol { get; set; } = null!;
}
