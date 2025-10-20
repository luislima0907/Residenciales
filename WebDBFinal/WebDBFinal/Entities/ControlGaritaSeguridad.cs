using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("ControlGaritaSeguridad")]
public partial class ControlGaritaSeguridad
{
    [Key]
    public int CodigoControl { get; set; }

    public int CodigoPersonaRol { get; set; }

    public int CodigoPersona { get; set; }

    public byte CodigoTipoRol { get; set; }

    public int CodigoMarcaje { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime FechaHoraInicio { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime FechaHoraFin { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string Observaciones { get; set; } = null!;

    [ForeignKey("CodigoMarcaje")]
    [InverseProperty("ControlGaritaSeguridads")]
    public virtual MarcajeLaboral CodigoMarcajeNavigation { get; set; } = null!;

    [InverseProperty("CodigoControlNavigation")]
    public virtual ICollection<DetalleFacturacionSeguridad> DetalleFacturacionSeguridads { get; set; } = new List<DetalleFacturacionSeguridad>();

    [ForeignKey("CodigoPersonaRol, CodigoPersona, CodigoTipoRol")]
    [InverseProperty("ControlGaritaSeguridads")]
    public virtual PersonaRol PersonaRol { get; set; } = null!;
}
