$projectName = "wuma-backend"
$templateFile = "azuredeploy/deployment.json"
$resourceGroupName = "${projectName}"

$location = "northeurope"

Connect-AzAccount

New-AzResourceGroup -Name $resourceGroupName -Location $location

New-AzResourceGroupDeployment `
	-Force `
	-Name azuredeploy `
	-ResourceGroupName $resourceGroupName `
	-TemplateFile $templateFile `
	-Location $location `
	-verbose