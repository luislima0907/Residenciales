using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

public partial class RegistroPersonaNoGrata
{
    [Key]
    public int CodigoRegistroPersonaNoGrata { get; set; }

    public int CodigoPersonaRol { get; set; }

    public int CodigoPersona { get; set; }

    public byte CodigoTipoRol { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime FechaDeclaracion { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string MotivoDeclaracion { get; set; } = null!;

    public bool? Estado { get; set; }

    [ForeignKey("CodigoPersonaRol, CodigoPersona, CodigoTipoRol")]
    [InverseProperty("RegistroPersonaNoGrata")]
    public virtual PersonaRol PersonaRol { get; set; } = null!;
}
