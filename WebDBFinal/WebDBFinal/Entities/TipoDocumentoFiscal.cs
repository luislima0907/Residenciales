using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("TipoDocumentoFiscal")]
public partial class TipoDocumentoFiscal
{
    [Key]
    public byte CodigoTipoDocumento { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string Descripcion { get; set; } = null!;

    [InverseProperty("CodigoTipoDocumentoNavigation")]
    public virtual ICollection<DocumentoFiscal> DocumentoFiscals { get; set; } = new List<DocumentoFiscal>();
}
