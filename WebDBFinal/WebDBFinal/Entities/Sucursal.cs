using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("Sucursal")]
public partial class Sucursal
{
    [Key]
    public int CodigoSucursal { get; set; }

    public int? CodigoEmpresa { get; set; }

    public int? CodigoResidencial { get; set; }

    [StringLength(80)]
    [Unicode(false)]
    public string Descripcion { get; set; } = null!;

    [InverseProperty("CodigoSucursalNavigation")]
    public virtual ICollection<Cluster> Clusters { get; set; } = new List<Cluster>();

    [ForeignKey("CodigoEmpresa")]
    [InverseProperty("Sucursals")]
    public virtual EmpresaSeguridad? CodigoEmpresaNavigation { get; set; }

    [ForeignKey("CodigoResidencial")]
    [InverseProperty("Sucursals")]
    public virtual Residencial? CodigoResidencialNavigation { get; set; }

    [InverseProperty("CodigoSucursalNavigation")]
    public virtual ICollection<DireccionSucursal> DireccionSucursals { get; set; } = new List<DireccionSucursal>();

    [InverseProperty("CodigoSucursalNavigation")]
    public virtual ICollection<PeriodoFacturacionSeguridad> PeriodoFacturacionSeguridads { get; set; } = new List<PeriodoFacturacionSeguridad>();

    [InverseProperty("CodigoSucursalNavigation")]
    public virtual ICollection<TelefonoSucursal> TelefonoSucursals { get; set; } = new List<TelefonoSucursal>();
}
