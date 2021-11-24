Param(
    [parameter(Mandatory=$false)][string]$appName="aspnetrun",
    [parameter(Mandatory=$false)][bool]$useLocalk8s=$true,
    [parameter(Mandatory=$false)][bool]$clean=$true
    )

function Install-Chart  {
    Param([string]$chart, [string]$initialOptions)
    $options=$initialOptions

    # if ($sslEnabled) {
    #     $options = "$options --set ingress.tls[0].secretName=$tlsSecretName --set ingress.tls[0].hosts={$dns}" 
    #     if ($sslSupport -ne "custom") {
    #         $options = "$options --set inf.tls.issuer=$sslIssuer"
    #     }
    # }
    # if ($customRegistry) {
    #     $options = "$options --set inf.registry.server=$registry --set inf.registry.login=$dockerUser --set inf.registry.pwd=$dockerPassword --set inf.registry.secretName=eshop-docker-scret"
    # }
    
    # if ($chart -ne "eshop-common" -or $customRegistry)  {       # eshop-common is ignored when no secret must be deployed        
    $command = "helm install $chart $chart $options"
    # $command = "install $appName-$chart $path/$chart"
    Write-Host "Helm Command: $command" -ForegroundColor Gray
    Invoke-Expression $command
    #}
}


Write-Host "Begin installation using Helm" -ForegroundColor Green

$ingressValuesFile="ingress_values_dockerk8s.yaml"
$dns="my-minikube"

if ($clean) {    
    #$listOfReleases=$(helm ls --filter aspnetrun -q)    
    $listOfReleases=$(helm ls -q)    
    if ([string]::IsNullOrEmpty($listOfReleases)) {
        Write-Host "No previous releases found!" -ForegroundColor Green
	}else{
        Write-Host "Previous releases found" -ForegroundColor Green
        Write-Host "Cleaning previous helm releases..." -ForegroundColor Green
        helm uninstall $listOfReleases
        Write-Host "Previous releases deleted" -ForegroundColor Green
	}        
}

$infras = ("basketdb", "catalogdb", "discountdb", "elasticsearch", "kibana", "orderdb", "rabbitmq")
$apis = ("basket","catalog", "ordering","discount", "discount-grpc","webstatus")
$charts = ("aspnetrunbasics")
$gateways = ("ocelotapigw", "shoppingaggregator")

foreach ($infra in $infras) {
    Write-Host "Installing infrastructure: $infra" -ForegroundColor Green
    Write-Host "$infra"
    helm install "$infra" --values app.yaml --values inf.yaml --values $ingressValuesFile --set app.name=$appName --set inf.k8s.dns=$dns --set "ingress.hosts={$dns}" $infra     
}

foreach ($api in $apis) {
    Write-Host "Installing: $api" -ForegroundColor Green
    Install-Chart $api "--values app.yaml --values inf.yaml --values $ingressValuesFile --set app.name=$appName --set inf.k8s.dns=$dns --set ingress.hosts=``{$dns``} --set inf.tls.enabled=false --set inf.mesh.enabled=false --set inf.k8s.local=$useLocalk8s"
}

foreach ($chart in $charts) {
    Write-Host "Installing: $chart" -ForegroundColor Green
    Install-Chart $chart "--values app.yaml --values inf.yaml --values $ingressValuesFile --set app.name=$appName --set inf.k8s.dns=$dns --set ingress.hosts=``{$dns``} --set inf.tls.enabled=false --set inf.mesh.enabled=false --set inf.k8s.local=$useLocalk8s"
}

foreach ($chart in $gateways) {
    Write-Host "Installing Api Gateway Chart: $chart" -ForegroundColor Green
    Install-Chart $chart "--values app.yaml --values inf.yaml --values $ingressValuesFile --set app.name=$appName --set inf.k8s.dns=$dns --set ingress.hosts=``{$dns``} --set inf.tls.enabled=false --set inf.mesh.enabled=false --set inf.k8s.local=$useLocalk8s"
}

Write-Host "helm charts installed." -ForegroundColor Green