using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Index("NumeroLicencia", Name = "UK_Licencia_Numero", IsUnique = true)]
public partial class Licencia
{
    [Key]
    public int CodigoLicencia { get; set; }

    public int CodigoPersona { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string NumeroLicencia { get; set; } = null!;

    public byte CodigoTipoLicencia { get; set; }

    public DateOnly FechaEmision { get; set; }

    public DateOnly FechaVencimiento { get; set; }

    [ForeignKey("CodigoPersona")]
    [InverseProperty("Licencia")]
    public virtual Persona CodigoPersonaNavigation { get; set; } = null!;

    [ForeignKey("CodigoTipoLicencia")]
    [InverseProperty("Licencia")]
    public virtual TipoLicencium CodigoTipoLicenciaNavigation { get; set; } = null!;

    [InverseProperty("CodigoLicenciaNavigation")]
    public virtual ICollection<RegistroMovimientoResidencial> RegistroMovimientoResidencials { get; set; } = new List<RegistroMovimientoResidencial>();
}
