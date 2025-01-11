# aspnetcore-server-details

Open CMD in the root folder

1. To build the image
docker build -t pradolabincacr01.azurecr.io/serverinfo.api -f ./ServerDetails/Dockerfile ./ServerDetails/

2. To push the image
docker push pradolabincacr01.azurecr.io/serverinfo.api

3. Create a Helm package
helm package --version 1.0.0 --destination ./DevOps/artefacts ./DevOps/deploy/helm/serverinfo-api/

4. Deploy Helm package
helm upgrade --namespace serverinfo-ns --install --set image.tag=latest --wait serverinfo-api ./DevOps/artefacts/serverinfo-api-1.0.0.tgz

5. Uninstall Helm package
helm uninstall serverinfo-api -n serverinfo-ns

6. To call the k8s service directly
kubectl port-forward svc/serverinfo-api-svc 20004:80 -n serverinfo-ns


Used by Visual Studio
docker run -dt -v "C:\Users\ChouhaP\vsdbg\vs2017u5:/remote_debugger:rw" -v "C:\repos\others\aspnetcore-server-details\ServerDetails:/app" -v "C:\repos\others\aspnetcore-server-details\ServerDetails:/src/" -v "C:\Users\ChouhaP\AppData\Roaming\Microsoft\UserSecrets:/root/.microsoft/usersecrets:ro" -v "C:\Users\ChouhaP\AppData\Roaming\ASP.NET\Https:/root/.aspnet/https:ro" -v "C:\Users\ChouhaP\.nuget\packages\:/root/.nuget/fallbackpackages" -e "DOTNET_USE_POLLING_FILE_WATCHER=1" -e "ASPNETCORE_LOGGING__CONSOLE__DISABLECOLORS=true" -e "ASPNETCORE_ENVIRONMENT=Development" -e "ASPNETCORE_URLS=https://+:443;http://+:80" -e "NUGET_PACKAGES=/root/.nuget/fallbackpackages" -e "NUGET_FALLBACK_PACKAGES=/root/.nuget/fallbackpackages" -P --name ServerDetails --entrypoint tail serverdetails:dev -f /dev/null 