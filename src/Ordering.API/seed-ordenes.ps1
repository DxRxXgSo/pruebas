param(
    [string]$BaseUrl = "http://localhost:8083",
    [string]$BasketUrl = "http://localhost:8082",
    [string]$CatalogUrl = "http://localhost:8080"
)

$ErrorActionPreference = "Stop"
$utf8 = [System.Text.UTF8Encoding]::new($false)
$tempDir = Join-Path $env:TEMP "opencode"
New-Item -ItemType Directory -Force -Path $tempDir | Out-Null

function Invoke-Json([string]$Method, [string]$Url, [string]$Body, [string]$IdempotencyKey = "") {
    $file = Join-Path $tempDir ("seed-body-" + [guid]::NewGuid().ToString("N") + ".json")
    [System.IO.File]::WriteAllText($file, $Body, $utf8)
    $args = @("-s", "-o", "-", "-w", "`n%{http_code}", "-X", $Method, $Url, "--data-binary", "@$file", "-H", "Content-Type: application/json")
    if ($IdempotencyKey) { $args += @("-H", "Idempotency-Key: $IdempotencyKey") }
    $out = & curl.exe @args 2>&1
    Remove-Item $file -Force
    $lines = $out -split "`n"
    $code = $lines[-1].Trim()
    $body = ($lines[0..($lines.Count - 2)] -join "`n").Trim()
    [pscustomobject]@{ Code = [int]$code; Body = $body }
}

$products = @(
    @{ Id = "019ff86d-cccc-4600-8272-6daa47511864"; Name = "sprite de 600 mililitros"; Price = 19.34 },
    @{ Id = "019ff86d-cd23-40a8-bc9a-61b6c99a7bd7"; Name = "coca-cola 6000"; Price = 25.34 },
    @{ Id = "019ff86d-cd4f-4f94-a107-9f630d1cf607"; Name = "Pepsicola"; Price = 18.00 }
)

$created = @()
for ($i = 1; $i -le 10; $i++) {
    $user = "comprador$i"
    $items = $products | ForEach-Object {
        "{ `"productId`": `"$($_.Id)`", `"productName`": `"$($_.Name)`", `"price`": $($_.Price), `"quantity`": 1, `"color`": `"Bebidas`", `"imageFile`": `"product-1.png`", `"imageUrl`": `"`" }"
    }
    $basketBody = "{ `"cart`": { `"userName`": `"$user`", `"items`": [ $($items -join ", ") ] } }"
    $basket = Invoke-Json "POST" "$BasketUrl/api/basket" $basketBody
    if ($basket.Code -ne 201 -and $basket.Code -ne 200) { Write-Host "[$user] basket fallo: $($basket.Code) $($basket.Body)" }

    $orderBody = "{ `"customerId`": `"$user`", `"basketId`": `"$user`" }"
    $order = Invoke-Json "POST" "$BaseUrl/api/orders" $orderBody ("seed-$user-1")
    if ($order.Code -ne 201 -and $order.Code -ne 200) { Write-Host "[$user] orden fallo: $($order.Code) $($order.Body)" }
    else {
        $id = ($order.Body | ConvertFrom-Json).id
        $created += [pscustomobject]@{ User = $user; OrderId = $id; Code = $order.Code }
        Write-Host "[OK] $user -> $id ($($order.Code))"
    }
}

Write-Host ""
Write-Host "=== Variar estados (transiciones) ==="
$pending = $created | Select-Object -First 1
$confirmed1 = $created[1]
$confirmed2 = $created[2]
$cancelled = $created[3]
foreach ($t in @(
    @{ Id = $confirmed1.OrderId; Status = "Confirmed" },
    @{ Id = $confirmed2.OrderId; Status = "Confirmed" },
    @{ Id = $cancelled.OrderId; Status = "Cancelled" }
)) {
    $r = Invoke-Json "PATCH" "$BaseUrl/api/orders/$($t.Id)/status" "{ `"status`": `"$($t.Status)`" }"
    Write-Host "PATCH $($t.Status) -> $($r.Code)"
}
$pendingId = $pending.OrderId
Write-Host "Pedido pendiente de muestra: $pendingId"

Write-Host ""
Write-Host "=== TOTALES ==="
$all = curl.exe -s "$BaseUrl/api/orders" | ConvertFrom-Json
Write-Host "Todas las ordenes: $($all.Count)"
$porCliente = $all | Group-Object customerId | ForEach-Object { "$($_.Name): $($_.Count)" }
Write-Host "Por cliente: $($porCliente -join ', ')"
