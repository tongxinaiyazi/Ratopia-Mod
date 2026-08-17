param(
    [string]$ModRoot = 'E:\steam\steamapps\common\Ratopia\BepInEx\plugins\SpecialRatizens'
)

$ErrorActionPreference = 'Stop'
$gameRoot = 'E:\steam\steamapps\common\Ratopia'
$script:AssemblySearchRoots = @(
    (Join-Path $gameRoot 'Ratopia_Data\Managed'),
    (Join-Path $gameRoot 'BepInEx\core'),
    $ModRoot
)

$resolver = [ResolveEventHandler] {
    param($sender, $eventArgs)

    $assemblyName = New-Object Reflection.AssemblyName($eventArgs.Name)
    foreach ($root in $script:AssemblySearchRoots) {
        $candidate = Join-Path $root ($assemblyName.Name + '.dll')
        if (Test-Path -LiteralPath $candidate) {
            return [Reflection.Assembly]::LoadFrom($candidate)
        }
    }

    return $null
}

[AppDomain]::CurrentDomain.add_AssemblyResolve($resolver)
try {
    [Reflection.Assembly]::LoadFrom((Join-Path $gameRoot 'Ratopia_Data\Managed\Newtonsoft.Json.dll')) | Out-Null
    [Reflection.Assembly]::LoadFrom((Join-Path $gameRoot 'Ratopia_Data\Managed\Assembly-CSharp.dll')) | Out-Null
    $modAssembly = [Reflection.Assembly]::LoadFrom((Join-Path $ModRoot 'SpecialRatizens.dll'))
    $baseCommandType = $modAssembly.GetType('RatopiaMod.BaseCommand', $true)
    $specialUnitType = $modAssembly.GetType('RatopiaMod.CustomSpecialUnit', $true)
    $flags = [Reflection.BindingFlags]'Static,Public,NonPublic'
    $loadMethod = $baseCommandType.GetMethods($flags) |
        Where-Object {
            $_.Name -eq 'LoadCsvData' -and
            $_.IsGenericMethodDefinition -and
            $_.GetParameters().Count -eq 4
        } |
        Select-Object -First 1
    $closedMethod = $loadMethod.MakeGenericMethod($specialUnitType)
    [object[]]$arguments = @(
        [string](Join-Path $ModRoot 'Data\CustomSpecialUnit.csv'),
        $null,
        [int]0,
        [string]''
    )
    $loaded = $closedMethod.Invoke($null, $arguments)
    "LOADED=$loaded COUNT=$($arguments[1].Count)"

    $instanceFlags = [Reflection.BindingFlags]'Instance,Public,NonPublic'
    $nameField = $specialUnitType.GetField('name', $instanceFlags)
    $genderField = $specialUnitType.GetField('gender', $instanceFlags)
    $unitGenderProperty = $specialUnitType.GetProperty('UnitGender', $instanceFlags)
    foreach ($unit in $arguments[1]) {
        $name = $nameField.GetValue($unit)
        $gender = $genderField.GetValue($unit)
        $unitGender = $unitGenderProperty.GetValue($unit, $null)
        "UNIT NAME=$name GENDER=$gender UNIT_GENDER=$unitGender"
    }
}
finally {
    [AppDomain]::CurrentDomain.remove_AssemblyResolve($resolver)
}
