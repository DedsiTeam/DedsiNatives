FROM mcr.microsoft.com/dotnet/aspnet:10.0
ENV TZ=Asia/Shanghai
ENV ASPNETCORE_ENVIRONMENT=Production
RUN ln -sf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
COPY ./ app/
WORKDIR /app
ENTRYPOINT ["dotnet", "DedsiNative.Host.dll"]
