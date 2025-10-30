using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[PrimaryKey("CodigoPersonaRol", "CodigoPersona", "CodigoTipoRol")]
[Table("PersonaRol")]
public partial class PersonaRol
{
    [Key]
    public int CodigoPersonaRol { get; set; }

    [Key]
    public int CodigoPersona { get; set; }

    [Key]
    public byte CodigoTipoRol { get; set; }

    public int? NumeroCasa { get; set; }

    public int? CodigoCluster { get; set; }

    public int? CodigoSucursal { get; set; }

    public int? CodigoSector { get; set; }

    [ForeignKey("NumeroCasa, CodigoCluster, CodigoSucursal, CodigoSector")]
    [InverseProperty("PersonaRols")]
    public virtual Casa? Casa { get; set; }

    [ForeignKey("CodigoPersona")]
    [InverseProperty("PersonaRols")]
    public virtual Persona CodigoPersonaNavigation { get; set; } = null!;

    [ForeignKey("CodigoTipoRol")]
    [InverseProperty("PersonaRols")]
    public virtual TipoRol CodigoTipoRolNavigation { get; set; } = null!;
    
    [InverseProperty("PersonaRol")]
    public virtual ICollection<DetalleCenso> DetalleCensos { get; set; } = new List<DetalleCenso>();

    [InverseProperty("PersonaRol")]
    public virtual ICollection<DocumentoFiscal> DocumentoFiscals { get; set; } = new List<DocumentoFiscal>();
    
    [InverseProperty("PersonaRol")]
    public virtual ICollection<IntegranteJunta> IntegranteJunta { get; set; } = new List<IntegranteJunta>();

    [InverseProperty("PersonaRol")]
    public virtual ICollection<MarcajeLaboral> MarcajeLaborals { get; set; } = new List<MarcajeLaboral>();

    [InverseProperty("PersonaRol")]
    public virtual ICollection<RegistroMovimientoResidencial> RegistroMovimientoResidencials { get; set; } = new List<RegistroMovimientoResidencial>();

    [InverseProperty("PersonaRol")]
    public virtual ICollection<RegistroPersonaNoGrata> RegistroPersonaNoGrata { get; set; } = new List<RegistroPersonaNoGrata>();

    [InverseProperty("PersonaRol")]
    public virtual ICollection<RegistroVehiculoResidente> RegistroVehiculoResidentes { get; set; } = new List<RegistroVehiculoResidente>();
}
