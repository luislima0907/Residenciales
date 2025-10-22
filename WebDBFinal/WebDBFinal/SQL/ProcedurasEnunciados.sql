--CONSULTA 16
CREATE PROCEDURE sp_Consulta16_PersonaMasVisitaCondominio
    AS
BEGIN
SELECT TOP 1 p.CodigoPersona,
    CONCAT(p.PrimerNombre, ' ', p.PrimerApellido) AS NombreCompleto, tr.Descripcion AS TipoRol,
       COUNT(*) AS cantidadVisitas
FROM Persona AS p
         INNER JOIN PersonaRol AS pr
                    ON p.CodigoPersona = pr.CodigoPersona
         INNER JOIN TipoRol AS tr
                    ON tr.CodigoTipoRol = pr.CodigoTipoRol
         INNER JOIN RegistroMovimientoResidencial AS rm
                    ON rm.CodigoPersona = pr.CodigoPersona
                        AND rm.CodigoPersonaRol = pr.CodigoPersonaRol
                        AND rm.CodigoTipoRol = pr.CodigoTipoRol
         INNER JOIN RegistroIngresoOSalida AS ris
                    ON ris.CodigoMovimientoResidencial = rm.CodigoMovimientoResidencial
GROUP BY p.CodigoPersona, p.PrimerNombre, p.PrimerApellido, tr.Descripcion
ORDER BY cantidadVisitas DESC
END
GO

--CONSUTA 17
CREATE PROCEDURE sp_Consulta17_InquilinosSonPropietarios
    AS
BEGIN
SELECT p.CodigoPersona,
       CONCAT(p.PrimerNombre, ' ', p.PrimerApellido) AS NombreCompleto
FROM PersonaRol AS pr
         INNER JOIN Persona AS p
                    ON pr.CodigoPersona = p.CodigoPersona
         INNER JOIN TipoRol AS tr
                    ON tr.CodigoTipoRol = pr.CodigoTipoRol
WHERE tr.Descripcion IN('Inquilino','Propietario')
GROUP BY p.CodigoPersona, p.PrimerNombre, p.PrimerApellido
HAVING COUNT(DISTINCT tr.Descripcion) = 2
END
GO 

EXECUTE sp_Consulta17_InquilinosSonPropietarios
GO

--CONSULTA 18
CREATE PROCEDURE sp_Consulta18_PropietariosLicenciaA
    AS
BEGIN
SELECT tr.Descripcion AS TipoPersona ,tl.TipoLicencia, tl.Descripcion, COUNT(*) AS TotalPersonasLicenciaA
FROM Persona AS p
         INNER JOIN PersonaRol AS pr
                    ON p.CodigoPersona = pr.CodigoPersona
         INNER JOIN TipoRol AS tr
                    ON pr.CodigoTipoRol = tr.CodigoTipoRol
         INNER JOIN Licencia AS l
                    ON l.CodigoPersona = pr.CodigoPersona
         INNER JOIN TipoLicencia AS tl
                    ON tl.CodigoTipoLicencia = l.CodigoTipoLicencia
WHERE tl.TipoLicencia = 'A'
  AND tr.Descripcion = 'Propietario'
GROUP BY tr.Descripcion, tl.TipoLicencia, tl.Descripcion
END
GO
EXECUTE sp_Consulta18_PropietariosLicenciaA
GO
--CONSULTA 19 
CREATE PROCEDURE sp_Consulta19_MultaPorCantidadCarros
    AS
BEGIN
SELECT  p.CodigoPersona, c.NumeroCasa, CONCAT(p.PrimerNombre, ' ', p.PrimerApellido) AS NombreCompleto,
        4 AS CarrosPermitidosPorVivienda,
        COUNT(rvr.CodigoVehiculoResidente) AS VehiculosRegistrados,
        (COUNT(rvr.CodigoVehiculoResidente) - 4) * 150.00 AS MultaAPagar
FROM Persona AS p
         INNER JOIN PersonaRol AS pr
                    ON p.CodigoPersona = pr.CodigoPersona
         INNER JOIN RegistroVehiculoResidente AS rvr
                    ON rvr.CodigoPersona = pr.CodigoPersona
                        AND rvr.CodigoPersonaRol = pr.CodigoPersonaRol
                        AND rvr.CodigoTipoRol = pr.CodigoTipoRol
         INNER JOIN Casa AS c
                    ON c.NumeroCasa = rvr.NumeroCasa
                        AND c.CodigoCluster = rvr.CodigoCluster
                        AND c.CodigoSucursal = rvr.CodigoSucursal
                        AND c.CodigoSector = rvr.CodigoSector
GROUP BY p.CodigoPersona, c.NumeroCasa, p.PrimerNombre, p.PrimerApellido
HAVING COUNT(rvr.CodigoVehiculoResidente) > 4;
END
GO

EXECUTE sp_Consulta19_MultaPorCantidadCarros
GO
--CONSULTA 20
CREATE PROCEDURE sp_Consulta20_ResidenciaConMasVisitas
    @FechaInicio DATETIME,
@FechaFin DATETIME
AS
BEGIN 
	IF @FechaInicio IS NOT NULL AND @FechaFin IS NOT NULL
BEGIN
SELECT TOP 1 c.NumeroCasa, c.CodigoCluster, c.CodigoSector, c.CodigoSucursal,
       COUNT(rios.CodigoEntradaOSalida) CantidadVisitas
FROM RegistroMovimientoResidencial AS rmr
         INNER JOIN Casa AS c
                    ON rmr.NumeroCasa = c.NumeroCasa
                        AND rmr.CodigoCluster = c.CodigoCluster
                        AND rmr.CodigoSucursal = c.CodigoSucursal
                        AND rmr.CodigoSector = c.CodigoSector
         INNER JOIN RegistroIngresoOSalida AS rios
                    ON rios.CodigoMovimientoResidencial = rmr.CodigoMovimientoResidencial
WHERE rios.TipoMovimiento = 'I'
  AND rios.FechaHora BETWEEN @FechaInicio AND @FechaFin
GROUP BY c.NumeroCasa, c.CodigoCluster, c.CodigoSector, c.CodigoSucursal
ORDER BY CantidadVisitas DESC
END
ELSE
BEGIN
        PRINT 'Debe proporcionar valores para @FechaInicio y @FechaFin'
END
END
GO

EXECUTE sp_Consulta20_ResidenciaConMasVisitas '2024-01-01 00:00:00.000', '2024-03-31 23:59:59.997'
GO

--CONSULTA 21
CREATE OR ALTER PROCEDURE sp_Consulta21_ConceptoMultaMasUsado
    @FechaInicio DATE,
    @FechaFin DATE
    AS
BEGIN 
	IF @FechaInicio IS NOT NULL AND @FechaFin IS NOT NULL
BEGIN
SELECT TOP 1 tm.Descripcion AS ConceptoMultaMasUsado,
    tm.ValorMulta,
       COUNT(tm.CodigoTipoMulta) AS CantidadUsadaMulta
FROM Multa AS m
         INNER JOIN TipoMulta AS tm
                    ON m.CodigoTipoMulta = tm.CodigoTipoMulta
WHERE m.Fecha BETWEEN @FechaInicio AND @FechaFin
GROUP BY tm.Descripcion,tm.ValorMulta
ORDER BY CantidadUsadaMulta DESC
END
ELSE
BEGIN 
	PRINT 'Debe proporcionar valores para @FechaInicio y @FechaFin'
	RETURN
END
END
GO 
EXECUTE sp_Consulta21_ConceptoMultaMasUsado '2024-01-01', '2024-03-31'
GO
--CONSULTA 22
CREATE PROCEDURE sp_Consulta22_CasasPendienteMultas
    AS
BEGIN
SELECT c.NumeroCasa, c.CodigoCluster, c.CodigoSector, c.CodigoSucursal,
       tm.Descripcion AS CausaMulta,
       tm.ValorMulta AS ValorDeuda
FROM  Multa AS m
          INNER JOIN TipoMulta AS tm
                     ON m.CodigoTipoMulta = tm.CodigoTipoMulta
          INNER JOIN Casa AS c
                     ON c.NumeroCasa = m.NumeroCasa
                         AND c.CodigoCluster = m.CodigoCluster
                         AND c.CodigoSector = m.CodigoSector
                         AND c.CodigoSucursal = m.CodigoSucursal
WHERE m.Pagada = 0
END

EXECUTE sp_Consulta22_CasasPendienteMultas