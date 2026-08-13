# ============================================================
# Pruebas mínimas obligatorias (P1-P8) - Ordering.API
# Uso:  .\ordenes-pruebas.ps1 [-BaseUrl http://localhost:8083]
# Requiere: Basket.API y Catalog.API disponibles (local o Railway)
# ============================================================
param(
    [string]$BaseUrl = "http://localhost:8083",
    [string]$CustomerId = "comprador1",
    [string]$BasketId = "comprador1"
)

$ErrorActionPreference = "Stop"
$key = [guid]::NewGuid().ToString()
$orderId = $null

function Show($title, $ok) {
    $status = if ($ok) { "PASS" } else { "FAIL" }
    Write-Host ("[{0}] {1}" -f $status, $title) -ForegroundColor $(if ($ok) { "Green" } else { "Red" })
}

Write-Host "=== P1: Crear orden valida ===" -ForegroundColor Cyan
$res = Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/api/orders" -Method Post `
    -Headers @{ "Idempotency-Key" = $key; "Content-Type" = "application/json" } `
    -Body (@{ customerId = $CustomerId; basketId = $BasketId } | ConvertTo-Json)
$order = $res.Content | ConvertFrom-Json
$orderId = $order.id
Show "P1: HTTP $($res.StatusCode) esperado 201, orden $orderId" ($res.StatusCode -eq 201 -and $order.total -gt 0)
Write-Host ("    Subtotal=$($order.subtotal) Tax=$($order.tax) Total=$($order.total) Status=$($order.status) Items=$($order.items.Count)") -ForegroundColor DarkGray

Write-Host "=== P2: Consultar orden ===" -ForegroundColor Cyan
$res2 = Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/api/orders/$orderId" -Method Get
$order2 = $res2.Content | ConvertFrom-Json
Show "P2: HTTP $($res2.StatusCode) esperado 200, datos completos" ($res2.StatusCode -eq 200 -and $order2.id -eq $orderId -and $order2.items.Count -eq $order.items.Count)

Write-Host "=== P3: Basket vacio -> 400 ===" -ForegroundColor Cyan
try {
    Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/api/orders" -Method Post `
        -Headers @{ "Idempotency-Key" = [guid]::NewGuid().ToString(); "Content-Type" = "application/json" } `
        -Body (@{ customerId = "usuario-sin-carrito"; basketId = "usuario-sin-carrito" } | ConvertTo-Json) | Out-Null
    Show "P3: esperado 400" $false
} catch {
    $code = [int]$_.Exception.Response.StatusCode
    Show "P3: HTTP $code esperado 400" ($code -eq 400)
    $detail = $_.ErrorDetails.Message
    if ($detail) { Write-Host ("    " + $detail) -ForegroundColor DarkGray }
}

Write-Host "=== P4: Repetir Idempotency-Key -> sin duplicado ===" -ForegroundColor Cyan
$res4 = Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/api/orders" -Method Post `
    -Headers @{ "Idempotency-Key" = $key; "Content-Type" = "application/json" } `
    -Body (@{ customerId = $CustomerId; basketId = $BasketId } | ConvertTo-Json)
$order4 = $res4.Content | ConvertFrom-Json
Show "P4: HTTP $($res4.StatusCode) esperado 200, misma orden $($order4.id)" ($res4.StatusCode -eq 200 -and $order4.id -eq $orderId)

$countByCustomer = (Invoke-RestMethod -Uri "$BaseUrl/api/orders/customer/$CustomerId").Count
$duplicated = $countByCustomer -gt (Invoke-RestMethod -Uri "$BaseUrl/api/orders/customer/$CustomerId" | Where-Object { $_.id -eq $orderId }).Count
Show "P4b: solo una orden con id $orderId en el cliente" (-not $duplicated)

Write-Host "=== P5: Pending -> Confirmed ===" -ForegroundColor Cyan
$res5 = Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/api/orders/$orderId/status" -Method Patch `
    -Headers @{ "Content-Type" = "application/json" } `
    -Body (@{ status = "Confirmed" } | ConvertTo-Json)
$order5 = $res5.Content | ConvertFrom-Json
Show "P5: HTTP $($res5.StatusCode) esperado 200, estado $($order5.status)" ($res5.StatusCode -eq 200 -and $order5.status -eq "Confirmed")

Write-Host "=== P6: Transicion invalida (Confirmed -> Cancelled) -> 409 ===" -ForegroundColor Cyan
try {
    Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/api/orders/$orderId/status" -Method Patch `
        -Headers @{ "Content-Type" = "application/json" } `
        -Body (@{ status = "Cancelled" } | ConvertTo-Json) | Out-Null
    Show "P6: esperado 409" $false
} catch {
    $code = [int]$_.Exception.Response.StatusCode
    Show "P6: HTTP $code esperado 409" ($code -eq 409)
    Write-Host ("    " + $_.ErrorDetails.Message) -ForegroundColor DarkGray
}

Write-Host "=== P7: MongoDB no disponible -> 500 controlado ===" -ForegroundColor Cyan
Write-Host "    (Manual: detener el cluster/servicio y crear una orden; debe responder 500 con mensaje generico sin stack trace)" -ForegroundColor Yellow

Write-Host "=== P8: Flujo React ===" -ForegroundColor Cyan
Write-Host "    (Manual: tienda -> agregar producto -> carrito -> Finalizar compra -> confirmacion visible)" -ForegroundColor Yellow

Write-Host ""
Write-Host "Orden generada: $orderId" -ForegroundColor Green
