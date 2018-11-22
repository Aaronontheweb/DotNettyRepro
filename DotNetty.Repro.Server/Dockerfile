FROM microsoft/dotnet:2.1-sdk AS builder

ARG sc=/DotNetty.Repro.Server

# copy project files to image
COPY *.csproj ${sc}/

## Restore packages
RUN dotnet restore /${sc} 

### copy all sources
COPY . ${sc}/

##### Publish Project
RUN dotnet publish /${sc} --output /publish 

FROM microsoft/dotnet:2.1-runtime
WORKDIR /publish
COPY --from=builder /publish .
ENTRYPOINT [ "dotnet", "DotNetty.Repro.Server.dll" ]
