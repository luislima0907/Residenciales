using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("PersonaTelefono")]
public partial class PersonaTelefono
{
    [Key]
    public int CodigoPersonaTelefono { get; set; }

    public int CodigoPersona { get; set; }

    public byte CodigoTipoTelefono { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string Numero { get; set; } = null!;

    [ForeignKey("CodigoPersona")]
    [InverseProperty("PersonaTelefonos")]
    public virtual Persona CodigoPersonaNavigation { get; set; } = null!;

    [ForeignKey("CodigoTipoTelefono")]
    [InverseProperty("PersonaTelefonos")]
    public virtual TipoTelefono CodigoTipoTelefonoNavigation { get; set; } = null!;

    [InverseProperty("CodigoPersonaTelefonoNavigation")]
    public virtual ICollection<RegistroMovimientoResidencial> RegistroMovimientoResidencials { get; set; } = new List<RegistroMovimientoResidencial>();
}
