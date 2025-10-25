using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("AplicacionDocumento")]
public partial class AplicacionDocumento
{
    [Key]
    public int CodigoAplicacionDocumento { get; set; }

    public byte CodigoTipoDocumento { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string Serie { get; set; } = null!;

    public int Numero { get; set; }

    public int? CodigoDetalleCargoMensual { get; set; }

    public int? CodigoCargoMensual { get; set; }

    public byte? CodigoTipoConcepto { get; set; }

    public int? CodigoMulta { get; set; }

    [Column(TypeName = "money")]
    public decimal MontoTotal { get; set; }

    [Column(TypeName = "money")]
    public decimal? IVA { get; set; }

    [ForeignKey("CodigoMulta")]
    [InverseProperty("AplicacionDocumentos")]
    public virtual Multa? CodigoMultaNavigation { get; set; }

    [ForeignKey("CodigoDetalleCargoMensual, CodigoCargoMensual, CodigoTipoConcepto")]
    [InverseProperty("AplicacionDocumentos")]
    public virtual DetalleCargoMensual? DetalleCargoMensual { get; set; }

    [InverseProperty("CodigoAplicacionDocumentoNavigation")]
    public virtual ICollection<DetallePagoDocumento> DetallePagoDocumentos { get; set; } = new List<DetallePagoDocumento>();

    [ForeignKey("CodigoTipoDocumento, Serie, Numero")]
    [InverseProperty("AplicacionDocumentos")]
    public virtual DocumentoFiscal DocumentoFiscal { get; set; } = null!;
}
