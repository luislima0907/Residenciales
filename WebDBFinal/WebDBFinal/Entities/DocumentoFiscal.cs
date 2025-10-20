using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[PrimaryKey("CodigoTipoDocumento", "Serie", "Numero")]
[Table("DocumentoFiscal")]
public partial class DocumentoFiscal
{
    [Key]
    public byte CodigoTipoDocumento { get; set; }

    [Key]
    [StringLength(15)]
    [Unicode(false)]
    public string Serie { get; set; } = null!;

    [Key]
    public int Numero { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime FechaEmision { get; set; }

    public int CodigoPersonaRol { get; set; }

    public int CodigoPersona { get; set; }

    public byte CodigoTipoRol { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string NIT { get; set; } = null!;

    [InverseProperty("DocumentoFiscal")]
    public virtual ICollection<AplicacionDocumento> AplicacionDocumentos { get; set; } = new List<AplicacionDocumento>();

    [ForeignKey("CodigoTipoDocumento")]
    [InverseProperty("DocumentoFiscals")]
    public virtual TipoDocumentoFiscal CodigoTipoDocumentoNavigation { get; set; } = null!;

    [ForeignKey("CodigoPersonaRol, CodigoPersona, CodigoTipoRol")]
    [InverseProperty("DocumentoFiscals")]
    public virtual PersonaRol PersonaRol { get; set; } = null!;
}
