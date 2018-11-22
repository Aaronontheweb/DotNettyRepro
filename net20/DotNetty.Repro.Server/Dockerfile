FROM microsoft/dotnet:2.0-sdk AS builder

EXPOSE 1099

ARG sc=/DotNetty.Repro.Server

# copy project files to image
COPY ${sc}/*.csproj ${sc}/

## Restore packages
RUN dotnet restore /${sc} 

### copy all sources
COPY ${sc}/. ${sc}/

##### Publish Project
RUN dotnet publish /${sc} --output /publish 

FROM microsoft/dotnet:2.0-runtime
WORKDIR /publish
COPY --from=builder /publish .
ENTRYPOINT [ "dotnet", "DotNetty.Repro.Server.dll" ]
