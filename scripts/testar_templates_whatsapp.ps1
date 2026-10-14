# Testa os templates do WhatsApp Cloud API enviando pelo numero NOVO (+55 41 3067-0280).
# Le o token do API\appsettings.json, monta as mensagens igual ao WhatsAppService.cs.
# Uso:  .\testar_templates_whatsapp.ps1 -Destino 41999999999
param(
    [Parameter(Mandatory = $true)][string]$Destino,
    [string]$PhoneId = "1401429289718239"
)

$cfgPath = Join-Path $PSScriptRoot "..\API\appsettings.json"
$cfg = Get-Content $cfgPath -Raw -Encoding UTF8 | ConvertFrom-Json
$wa = $cfg.WhatsApp
$base = ($wa.BaseUrl).TrimEnd('/')
$headers = @{ Authorization = "Bearer $($wa.ApiToken)" }

# Mesma regra do NormalizarNumeroBrasil: remove o 9 em DDDs fora de 11-19, 21, 22, 24, 27, 28
function Normalizar([string]$n) {
    $d = ($n -replace '\D', '')
    if ($d.StartsWith('0')) { $d = $d.Substring(1) }
    if (-not $d.StartsWith('55')) { $d = '55' + $d }
    if ($d.Length -ne 13) { return $d }
    $ddd = [int]$d.Substring(2, 2)
    $mantem = ($ddd -ge 11 -and $ddd -le 19) -or $ddd -in 21, 22, 24, 27, 28
    if (-not $mantem -and $d[4] -eq '9') { return $d.Remove(4, 1) }
    return $d
}

$to = Normalizar $Destino
Write-Host "Enviando de PhoneId $PhoneId para $to`n"

function Enviar($nome, $lang, $bodyParams, $botao) {
    $components = @()
    if ($bodyParams) {
        $components += @{ type = 'body'; parameters = @($bodyParams | ForEach-Object { @{ type = 'text'; text = $_ } }) }
    }
    if ($botao) {
        $components += @{ type = 'button'; sub_type = 'url'; index = '0'; parameters = @(@{ type = 'text'; text = $botao }) }
    }
    $tpl = @{ name = $nome; language = @{ code = $lang } }
    if ($components.Count -gt 0) { $tpl.components = $components }
    $payload = @{ messaging_product = 'whatsapp'; to = $to; type = 'template'; template = $tpl } | ConvertTo-Json -Depth 10
    try {
        $r = Invoke-RestMethod -Method Post -Uri "$base/$PhoneId/messages" -Headers $headers `
             -ContentType 'application/json; charset=utf-8' -Body ([Text.Encoding]::UTF8.GetBytes($payload))
        Write-Host ("[OK]   {0,-20} id={1}" -f $nome, $r.messages[0].id) -ForegroundColor Green
    } catch {
        $msg = $_.ErrorDetails.Message
        if (-not $msg) { $msg = $_.Exception.Message }
        Write-Host ("[ERRO] {0,-20} {1}" -f $nome, $msg) -ForegroundColor Red
    }
}

Enviar 'hello_world'         'en_US' $null $null
Enviar $wa.TemplateCotacao   $wa.TemplateCotacaoLanguage   @('Cliente Teste', '25.90', 'PAC')                          'teste123'
Enviar $wa.TemplateRastreio  $wa.TemplateRastreioLanguage  @('Cliente Teste', 'Correios', 'AA123456789BR')             'AA123456789BR'
Enviar $wa.TemplateResumoCompra $wa.TemplateResumoCompraLanguage @('Cliente Teste', '9999', '1x Blusa teste', '49.90') 'teste123'
