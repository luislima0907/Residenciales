using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("Persona")]
[Index("CUI", Name = "UK_Persona_CUI", IsUnique = true)]
public partial class Persona
{
    [Key]
    public int CodigoPersona { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string CUI { get; set; } = null!;

    [StringLength(25)]
    [Unicode(false)]
    public string PrimerNombre { get; set; } = null!;

    [StringLength(25)]
    [Unicode(false)]
    public string? SegundoNombre { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string? TercerNombre { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string PrimerApellido { get; set; } = null!;

    [StringLength(25)]
    [Unicode(false)]
    public string? SegundoApellido { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string? TercerApellido { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string Genero { get; set; } = null!;

    public DateOnly FechaDeNacimiento { get; set; }

    public byte CodigoEstadoCivil { get; set; }

    [ForeignKey("CodigoEstadoCivil")]
    [InverseProperty("Personas")]
    public virtual EstadoCivil CodigoEstadoCivilNavigation { get; set; } = null!;

    [InverseProperty("CodigoPersonaNavigation")]
    public virtual ICollection<Licencium> Licencia { get; set; } = new List<Licencium>();

    [InverseProperty("CodigoPersonaNavigation")]
    public virtual ICollection<PersonaRol> PersonaRols { get; set; } = new List<PersonaRol>();

    [InverseProperty("CodigoPersonaNavigation")]
    public virtual ICollection<PersonaTelefono> PersonaTelefonos { get; set; } = new List<PersonaTelefono>();
}
