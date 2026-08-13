# ============================================================
# Asigna stock a los productos del catálogo (requiere Catalog.API
# desplegado con el campo Stock, es decir, después de re-publicar).
# Uso: .\actualizar-stock.ps1 [-BaseUrl https://catalog-production-xxxx.up.railway.app] [-Stock 20]
# ============================================================
param(
    [string]$BaseUrl = "https://catalog-production-3284.up.railway.app",
    [int]$Stock = 20
)

$ErrorActionPreference = "Stop"

$products = (Invoke-RestMethod -UseBasicParsing -Uri "$BaseUrl/api/products").data

foreach ($p in $products) {
    $body = @{
        name        = $p.name
        description = $p.descripcion
        category    = if ($p.category -is [string]) { @($p.category) } else { @($p.category) }
        imagesFiles = $p.imageFiles
        imageUrl    = $p.imageUrl
        price       = $p.price
        stock       = $Stock
    } | ConvertTo-Json

    try {
        $r = Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/api/products/$([uri]::EscapeDataString($p.name))" `
            -Method Put -ContentType "application/json" -Body $body
        Write-Host ("[OK] {0} -> stock {1} (HTTP {2})" -f $p.name, $Stock, $r.StatusCode) -ForegroundColor Green
    }
    catch {
        Write-Host ("[FAIL] {0}: {1}" -f $p.name, $_.ErrorDetails.Message) -ForegroundColor Red
    }
}

$verificacion = Invoke-RestMethod -UseBasicParsing -Uri "$BaseUrl/api/products"
Write-Host ("`nVerificación: {0} productos, ejemplo: {1} -> stock {2}" -f $verificacion.data.Count, $verificacion.data[0].name, $verificacion.data[0].stock) -ForegroundColor Cyan