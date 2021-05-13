$posh = Get-Module posh-git
Write-Output $posh

if ($posh -eq $null)
{
	Install-Module posh-git -Scope CurrentUser -Force
}
else
{
	Update-Module posh-git
}

Import-Module posh-git


#install-module posh-git -scope currentuser -force
#or
#update-module posh-git