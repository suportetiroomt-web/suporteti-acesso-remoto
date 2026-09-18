[CmdletBinding()]
param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot "..\artifacts")
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$buildDirectory = Join-Path $projectRoot "build"
$outputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
$rustDeskUrl = "https://github.com/rustdesk/rustdesk/releases/download/1.4.9/rustdesk-1.4.9-x86_64.exe"
$rustDeskSha256 = "EAEDEB0088E687BF46F7C46A9C6EA5493CE51F3134DFD6ACBEDB47B5B9136274"
$rustDeskBinary = Join-Path $buildDirectory "rustdesk-1.4.9-x86_64.exe"
$compiler = Join-Path $env:WINDIR "Microsoft.NET\Framework64\v4.0.30319\csc.exe"

New-Item -ItemType Directory -Force -Path $buildDirectory, $outputDirectory | Out-Null

if (-not (Test-Path -LiteralPath $rustDeskBinary)) {
    Invoke-WebRequest -Uri $rustDeskUrl -OutFile $rustDeskBinary
}

$actualHash = (Get-FileHash -LiteralPath $rustDeskBinary -Algorithm SHA256).Hash
if ($actualHash -ne $rustDeskSha256) {
    throw "RustDesk SHA-256 inválido. Esperado: $rustDeskSha256; obtido: $actualHash"
}

$signature = Get-AuthenticodeSignature -LiteralPath $rustDeskBinary
if ($signature.Status -ne "Valid") {
    throw "A assinatura Authenticode do RustDesk não é válida: $($signature.Status)"
}

if (-not (Test-Path -LiteralPath $compiler)) {
    throw "Compilador C# do .NET Framework não encontrado em $compiler"
}

$outputFile = Join-Path $outputDirectory "SuporteTI-AcessoRemoto.exe"
$arguments = @(
    "/nologo",
    "/target:winexe",
    "/platform:anycpu",
    "/optimize+",
    "/out:$outputFile",
    "/win32icon:$projectRoot\assets\SuporteTI-brand.ico",
    "/resource:$rustDeskBinary,RustDeskPayload",
    "/resource:$projectRoot\assets\suporteti-logo.jpeg,SuporteTILogo",
    "/reference:System.dll",
    "/reference:System.Drawing.dll",
    "/reference:System.Windows.Forms.dll",
    "$projectRoot\src\AssemblyInfo.cs",
    "$projectRoot\src\SuporteTIRemote.cs"
)

& $compiler @arguments
if ($LASTEXITCODE -ne 0) {
    throw "Falha na compilação (código $LASTEXITCODE)."
}

$artifactHash = (Get-FileHash -LiteralPath $outputFile -Algorithm SHA256).Hash
Write-Host "Artefato: $outputFile"
Write-Host "SHA-256: $artifactHash"
