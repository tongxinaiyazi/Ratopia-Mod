param(
    [Parameter(Mandatory = $true)]
    [string]$Path,

    [switch]$ListCitizens,

    [switch]$SkinCategorySummary,

    [switch]$DetailedSkin
)

$ErrorActionPreference = 'Stop'
$gameRoot = 'E:\steam\steamapps\common\Ratopia'
$script:AssemblySearchRoots = @(
    (Join-Path $gameRoot 'Ratopia_Data\Managed'),
    (Join-Path $gameRoot 'BepInEx\core')
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
    [Reflection.Assembly]::LoadFrom((Join-Path $gameRoot 'Ratopia_Data\Managed\Assembly-CSharp.dll')) | Out-Null

    $stream = [IO.File]::OpenRead((Resolve-Path -LiteralPath $Path))
    try {
        $formatter = New-Object Runtime.Serialization.Formatters.Binary.BinaryFormatter
        $root = $formatter.Deserialize($stream)
    }
    finally {
        $stream.Dispose()
    }

    "ROOT TYPE: $($root.GetType().FullName)"
    $flags = [Reflection.BindingFlags]'Instance,Public,NonPublic'
    foreach ($field in $root.GetType().GetFields($flags)) {
        $value = $field.GetValue($root)
        $valueType = if ($null -eq $value) { '<null>' } else { $value.GetType().FullName }
        $count = if ($value -is [Collections.ICollection]) { " Count=$($value.Count)" } else { '' }
        "FIELD $($field.Name): $valueType$count"
    }

    if ($ListCitizens) {
        $citizenListField = $root.GetType().GetField('List_Citizen', $flags)
        $citizens = $citizenListField.GetValue($root)
        foreach ($citizen in $citizens) {
            $citizenType = $citizen.GetType()
            $id = $citizenType.GetField('m_ID', $flags).GetValue($citizen)
            $name = $citizenType.GetField('m_UnitName', $flags).GetValue($citizen)
            $gender = $citizenType.GetField('m_Gender', $flags).GetValue($citizen)
            $skinDictionary = $citizenType.GetField('_skinDic', $flags).GetValue($citizen)
            $skinEntries = @(
                foreach ($key in $skinDictionary.Keys | Sort-Object) {
                    "$key=$($skinDictionary[$key])"
                }
            )
            "CITIZEN ID=$id NAME=$name GENDER=$gender SKINS={$($skinEntries -join ';')}"

            if ($DetailedSkin) {
                foreach ($field in $citizenType.GetFields($flags)) {
                    if ($field.Name -match 'Gender|Skin|UnitName') {
                        $value = $field.GetValue($citizen)
                        "  DETAIL FIELD=$($field.Name) TYPE=$($field.FieldType.FullName) VALUE=$value"
                    }
                }
            }
        }
    }

    if ($SkinCategorySummary) {
        $citizenListField = $root.GetType().GetField('List_Citizen', $flags)
        $citizens = $citizenListField.GetValue($root)
        $categories = @('Skin', 'Face', 'Hair', 'Dress', 'Cheek', 'Glasses', 'Hat', 'Makeup')
        foreach ($category in $categories) {
            $present = 0
            $nonEmpty = 0
            foreach ($citizen in $citizens) {
                $skinDictionary = $citizen.GetType().GetField('_skinDic', $flags).GetValue($citizen)
                if ($skinDictionary.ContainsKey($category)) {
                    $present++
                    if (-not [string]::IsNullOrEmpty([string]$skinDictionary[$category])) {
                        $nonEmpty++
                    }
                }
            }
            "CATEGORY $category PRESENT=$present NONEMPTY=$nonEmpty TOTAL=$($citizens.Count)"
        }
    }
}
finally {
    [AppDomain]::CurrentDomain.remove_AssemblyResolve($resolver)
}
