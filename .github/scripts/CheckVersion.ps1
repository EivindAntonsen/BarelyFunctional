param (
    [string]$githubRepository,
    [string]$version,
    [string]$accessToken
)

$uri = "https://api.github.com/repos/$githubRepository/releases/tags/v$version"

try {
    $response = Invoke-RestMethod -Method Get -Headers @{ Authorization = "Bearer $accessToken" } -Uri $uri
    Write-Output "Version already exists"
    
    exit 1
}
catch {
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Output "Version does not exist, safe to continue"
        exit 0
    }
    else {
        throw
    }
}